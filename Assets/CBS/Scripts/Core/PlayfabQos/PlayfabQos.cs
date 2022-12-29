using RSG;
using UnityEngine;

#if !DISABLE_PLAYFABCLIENT_API && !DISABLE_PLAYFABENTITY_API
namespace PlayFab.QoS
{
    using EventsModels;
    using MultiplayerModels;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class PlayFabQosApi
    {
        private readonly PlayFabAuthenticationContext _authContext;
        private const int DefaultPingsPerRegion = 10;
        private const int DefaultDegreeOfParallelism = 4;
        private const int NumTimeoutsForError = 3;
        private const int DefaultTimeoutMs = 250;

        private readonly PlayFabMultiplayerInstanceAPI multiplayerApi;
        private readonly PlayFabEventsInstanceAPI eventsApi;

        private readonly Action<object> qosResultsReporter;

        private Dictionary<string, string> _dataCenterMap;

        private readonly bool _reportResults;

        public PlayFabQosApi(PlayFabApiSettings settings = null, PlayFabAuthenticationContext authContext = null,
            bool reportResults = true)
        {
            _authContext = authContext ?? PlayFabSettings.staticPlayer;

            multiplayerApi = new PlayFabMultiplayerInstanceAPI(settings, _authContext);
            eventsApi = new PlayFabEventsInstanceAPI(settings, _authContext);
            qosResultsReporter = SendSuccessfulQosResultsToPlayFab;
            _reportResults = reportResults;
        }

#pragma warning disable 4014
        public Promise<QosResult> GetQosResultAsync(
            int timeoutMs = DefaultTimeoutMs,
            int pingsPerRegion = DefaultPingsPerRegion,
            int degreeOfParallelism = DefaultDegreeOfParallelism)
        {
            Promise<QosResult> promise = new Promise<QosResult>();
            GetResultAsync(timeoutMs, pingsPerRegion, degreeOfParallelism).Then((result =>
            {
                if (result.ErrorCode != (int)QosErrorCode.Success)
                {
                    promise.Reject(new Exception(result.ErrorMessage));
                }

                if (_reportResults)
                {
                    qosResultsReporter(result);
                }

                promise.Resolve(result);
            }));

            return promise;
        }
#pragma warning restore 4014

        private Promise<QosResult> GetResultAsync(int timeoutMs, int pingsPerRegion, int degreeOfParallelism)
        {
            Promise<QosResult> promise = new Promise<QosResult>();
            if (!_authContext.IsClientLoggedIn())
            {
                var qosReult = new QosResult
                {
                    ErrorCode = (int)QosErrorCode.NotLoggedIn,
                    ErrorMessage = "Client is not logged in"
                };
                promise.Resolve(qosReult);
            }

            var dataCenterMapPromise = GetQoSServerList();

            dataCenterMapPromise.Then((async dataCenterMap =>
            {
                if (dataCenterMap == null || dataCenterMap.Count == 0)
                {
                    var QosResult = new QosResult
                    {
                        ErrorCode = (int)QosErrorCode.FailedToRetrieveServerList,
                        ErrorMessage = "Failed to get server list from PlayFab multiplayer servers"
                    };
                    promise.Resolve(QosResult);
                }

                var result =
                    await GetSortedRegionLatencies(timeoutMs, dataCenterMap, pingsPerRegion, degreeOfParallelism);
                promise.Resolve(result);
            }));


            return promise;
        }

        private Promise<Dictionary<string, string>> GetQoSServerList()
        {
            Promise<Dictionary<string, string>> promise = new Promise<Dictionary<string, string>>();
            if (_dataCenterMap?.Count > 0)
            {
                // If the dataCenterMap is already initialized, return
                return null;
            }

            var request = new ListQosServersForTitleRequest();
            multiplayerApi.ListQosServersForTitle(request, (titleResponse =>
            {
                var dataCenterMap = new Dictionary<string, string>(titleResponse.QosServers.Count);

                foreach (QosServer qosServer in titleResponse.QosServers)
                {
                    if (!string.IsNullOrEmpty(qosServer.Region))
                    {
                        dataCenterMap[qosServer.Region] = qosServer.ServerUrl;
                    }
                }

                promise.Resolve(_dataCenterMap = dataCenterMap);
            }), (Debug.LogError));

            return promise;
        }

        private async Task<QosResult> GetSortedRegionLatencies(int timeoutMs,
            Dictionary<string, string> dataCenterMap, int pingsPerRegion, int degreeOfParallelism)
        {
            RegionPinger[] regionPingers = new RegionPinger[dataCenterMap.Count];

            int index = 0;
            foreach (KeyValuePair<string, string> datacenter in dataCenterMap)
            {
                regionPingers[index] = new RegionPinger(datacenter.Value, datacenter.Key, timeoutMs,
                    NumTimeoutsForError, pingsPerRegion);
                index++;
            }

            // initialRegionIndexes are the index of the first region that a ping worker will use. Distribute the
            // indexes such that they are as far apart as possible to reduce the chance of sending all the pings
            // to the same region at the same time

            // Example, if there are 6 regions and 3 pings per region, we will start pinging at regions 0, 2, and 4
            // as shown in the table below

            //        Region 0    Region 1    Region 2    Region 3    Region 4    Region 5
            // Ping 1    x
            // Ping 2                           x
            // Ping 3                                                    x
            //
            ConcurrentBag<int> initialRegionIndexes = new ConcurrentBag<int>(Enumerable.Range(0, pingsPerRegion)
                .Select(i => i * dataCenterMap.Count / pingsPerRegion));

            Task[] pingWorkers = Enumerable.Range(0, degreeOfParallelism).Select(
                i => PingWorker(regionPingers, initialRegionIndexes)).ToArray();

            await Task.WhenAll(pingWorkers);

            List<QosRegionResult> results = regionPingers.Select(x => x.GetResult()).ToList();
            results.Sort((x, y) => x.LatencyMs.CompareTo(y.LatencyMs));

            QosErrorCode resultCode = QosErrorCode.Success;
            string errorMessage = null;
            if (results.All(x => x.ErrorCode == (int)QosErrorCode.NoResult))
            {
                resultCode = QosErrorCode.NoResult;
                errorMessage = "No valid results from any QoS server";
            }

            return new QosResult()
            {
                ErrorCode = (int)resultCode,
                RegionResults = results,
                ErrorMessage = errorMessage
            };
        }

        private async Task PingWorker(RegionPinger[] regionPingers,
            IProducerConsumerCollection<int> initialRegionIndexes)
        {
            // For each initialRegionIndex, walk through all regions and do a ping starting at the index given and
            // wrapping around to 0 when reaching the final index
            while (initialRegionIndexes.TryTake(out int initialRegionIndex))
            {
                for (int i = 0; i < regionPingers.Length; i++)
                {
                    int index = (i + initialRegionIndex) % regionPingers.Length;
                    await regionPingers[index].PingAsync();
                }
            }
        }

        private void SendSuccessfulQosResultsToPlayFab(object resultState)
        {
            var result = (QosResult)resultState;
            var eventContents = new EventContents
            {
                Name = "qos_result",
                EventNamespace = "playfab.servers",
                Payload = QosResultPlayFabEvent.CreateFrom(result)
            };

            var writeEventsRequest = new WriteEventsRequest
            {
                Events = new List<EventContents> { eventContents }
            };

            eventsApi.WriteTelemetryEvents(writeEventsRequest, null, null);
        }
    }
}

#endif