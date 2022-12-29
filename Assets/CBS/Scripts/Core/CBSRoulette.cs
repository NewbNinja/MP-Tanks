using CBS.Playfab;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSRoulette : CBSModule, IRoulette
    {
        private IFabRoulette FabRoulette { get; set; }
        private IProfile Profile { get; set; }

        protected override void Init()
        {
            FabRoulette = FabExecuter.Get<FabRoulette>();
            Profile = Get<CBSProfile>();
        }

        /// <summary>
        /// Get list of all roulette positions
        /// </summary>
        /// <param name="result"></param>
        public void GetRouletteTable(Action<GetRouletteTableResult> result)
        {
            string profileID = Profile.PlayerID;

            FabRoulette.GetRouletteTable(profileID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetRouletteTableResult {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<RouletteTable>(rawData);

                    result?.Invoke(new GetRouletteTableResult {
                        IsSuccess = true,
                        Table = resultObject
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetRouletteTableResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Start spin roulette and get spin result
        /// </summary>
        /// <param name="result"></param>
        public void Spin(Action<SpinRouletteResult> result)
        {
            string profileID = Profile.PlayerID;

            FabRoulette.SpinRoulette(profileID, onSpin => {
                if (onSpin.Error != null)
                {
                    result?.Invoke(new SpinRouletteResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onSpin.Error)
                    });
                }
                else
                {
                    var rawData = onSpin.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<RoulettePosition>(rawData);
                    var prize = resultObject.Prize;

                    if (resultObject != null && prize != null)
                    {
                        var currencies = prize.BundledVirtualCurrencies;
                        if (currencies != null)
                        {
                            var codes = currencies.Select(x => x.Key).ToArray();
                            Get<CBSCurrency>().ChangeRequest(codes);
                        }
                    }

                    result?.Invoke(new SpinRouletteResult {
                        IsSuccess = true,
                        Position = resultObject
                    });
                }
            }, onFailed => {
                result?.Invoke(new SpinRouletteResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetRouletteTableResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public RouletteTable Table;
    }

    public struct SpinRouletteResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public RoulettePosition Position;
    }

}

