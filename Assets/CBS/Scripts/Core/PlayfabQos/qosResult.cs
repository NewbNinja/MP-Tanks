#if !DISABLE_PLAYFABCLIENT_API && !DISABLE_PLAYFABENTITY_API
namespace PlayFab.QoS
{
    using System;
    using System.Collections.Generic;

    public class QosResult
    {
        public List<QosRegionResult> RegionResults;

        public int ErrorCode;

        public string ErrorMessage;
    }

    /// <summary>
    /// This class is used to json serialize the content to send to PlayFab without the ErrorMessage
    /// </summary>
    internal class QosResultPlayFabEvent
    {
        public static QosResultPlayFabEvent CreateFrom(QosResult result)
        {
            return new QosResultPlayFabEvent
            {
                RegionResults = result.RegionResults.ConvertAll(x => new QosRegionResultSummary()
                {
                    Region = x.Region,
                    ErrorCode = x.ErrorCode,
                    LatencyMs = x.LatencyMs
                }),
                ErrorCode = result.ErrorCode
            };
            throw new NotSupportedException("QoS ping library is only supported on .net standard 2.0 and newer, .net core or full .net framework");
        }

        public List<QosRegionResultSummary> RegionResults;

        public int ErrorCode;
    }
}
#endif