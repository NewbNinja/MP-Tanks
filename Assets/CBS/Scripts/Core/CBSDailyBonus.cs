using CBS.Playfab;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSDailyBonus : CBSModule, IDailyBonus
    {
        /// <summary>
        /// Notifies when a user has received a reward
        /// </summary>
        public event Action<CollectDailyBonusResult> OnRewardCollected;

        private IFabDailyBonus FabDaily { get; set; }
        private IProfile Profile { get; set; }

        protected override void Init()
        {
            Profile = Get<CBSProfile>();
            FabDaily = FabExecuter.Get<FabDailyBonus>();
        }

        /// <summary>
        /// Get information about the status of the current user's daily rewards. Also get a list of all daily rewards.
        /// </summary>
        /// <param name="result"></param>
        public void GetDailyBonus(Action<GetDailyBonusResult> result)
        {
            string profileID = Profile.PlayerID;

            FabDaily.GetDailyBonusState(profileID, onGet => {
                if (onGet.Error != null)
                {
                    result?.Invoke(new GetDailyBonusResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onGet.Error)
                    });
                }
                else
                {
                    var rawData = onGet.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var resultObject = jsonPlugin.DeserializeObject<DailyBonusResultData>(rawData);
                    var prizes = resultObject.DailyData.DaliyPrizes;
                    var prizesInfo = prizes.Select(x => new DailyBonusInfo {
                        DayNumber = prizes.IndexOf(x) + 1,
                        IsPicked = prizes.IndexOf(x) == resultObject.CurrentDailyIndex ? resultObject.Picked : prizes.IndexOf(x) < resultObject.CurrentDailyIndex,
                        IsCurrent = prizes.IndexOf(x) == resultObject.CurrentDailyIndex,
                        Prize = x
                    }).ToList();

                    result?.Invoke(new GetDailyBonusResult
                    {
                        IsSuccess = true,
                        CurrentDailyIndex = resultObject.CurrentDailyIndex,
                        Picked = resultObject.Picked,
                        DaliyPrizes = prizesInfo
                    });
                }
            }, onFailed => {
                result?.Invoke(new GetDailyBonusResult {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }

        /// <summary>
        /// Collect the daily reward.
        /// </summary>
        /// <param name="result"></param>
        public void CollectDailyBonus(Action<CollectDailyBonusResult> result)
        {
            string profileID = Profile.PlayerID;

            FabDaily.CollectDailyBonus(profileID, onCollect => {
                if (onCollect.Error != null)
                {
                    result?.Invoke(new CollectDailyBonusResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onCollect.Error)
                    });
                }
                else
                {
                    Debug.Log(onCollect.FunctionResult);
                    var rawData = onCollect.FunctionResult.ToString();
                    var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                    var jsonResult = jsonPlugin.DeserializeObject<PrizeObject>(rawData);

                    if (jsonResult != null)
                    {
                        var currencies = jsonResult.BundledVirtualCurrencies;
                        if (currencies != null)
                        {
                            var codes = currencies.Select(x => x.Key).ToArray();
                            Get<CBSCurrency>().ChangeRequest(codes);
                        }
                    }

                    var resultObject = new CollectDailyBonusResult
                    {
                        IsSuccess = true,
                        Prize = jsonResult
                    };

                    result?.Invoke(resultObject);

                    OnRewardCollected?.Invoke(resultObject);
                }
            }, onError => {
                result?.Invoke(new CollectDailyBonusResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        /// <summary>
        /// Reset daily bonus reward state of current user.
        /// </summary>
        /// <param name="result"></param>
        public void ResetDailyBonus(Action<ResetDailyBonusResult> result)
        {
            string profileID = Profile.PlayerID;

            FabDaily.ResetDailyBonus(profileID, onReset => { 
                if (onReset.Error != null)
                {
                    result?.Invoke(new ResetDailyBonusResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(onReset.Error)
                    });
                }
                else
                {
                    result?.Invoke(new ResetDailyBonusResult
                    {
                        IsSuccess = true
                    });
                }
            }, onFailed => {
                result?.Invoke(new ResetDailyBonusResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onFailed)
                });
            });
        }
    }

    public struct GetDailyBonusResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public int CurrentDailyIndex;
        public bool Picked;
        public List<DailyBonusInfo> DaliyPrizes;
    }

    public struct ResetDailyBonusResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    public struct CollectDailyBonusResult
    {
        public bool IsSuccess;
        public SimpleError Error;

        public PrizeObject Prize;
    }
}
