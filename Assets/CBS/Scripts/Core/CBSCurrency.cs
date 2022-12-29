using CBS.Playfab;
using CBS.Scriptable;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public class CBSCurrency : CBSModule, ICurrency
    {
        public event Action<CBSUpdateCurrencyResult> OnCurrencyUpdated;

        private readonly string CatalogID = CBSConstants.CurrencyCatalogID;

        public Dictionary<string, int> CacheCurrencies { get; private set; }

        private IAuth Auth { get; set; }
        private IFabCurrency FubCurrency { get; set; }
        private IFabItems FabProducts { get; set; }
        private AuthData AuthData { get; set; }

        protected override void Init()
        {
            Auth = Get<CBSAuth>();
            FubCurrency = FabExecuter.Get<FabCurrency>();
            FabProducts = FabExecuter.Get<FabItems>();
            AuthData = CBSScriptable.Get<AuthData>();

            Auth.OnLoginEvent += OnUserLogined;
        }

        // API calls
        public void GetCurrencies(Action<CBSGetCurrenciesResult> result = null)
        {
            FubCurrency.GetUserCurrencies(onGet =>
            {
                // parse and cache current currencies
                ParseCurrency(onGet.VirtualCurrency);

                var callback = new CBSGetCurrenciesResult
                {
                    IsSuccess = true,
                    Currencies = CacheCurrencies,
                    Result = onGet
                };
                result?.Invoke(callback);
            },
            onError => 
            {
                var callback = new CBSGetCurrenciesResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        public void AddUserCurrency(int amount, string currency, Action<CBSUpdateCurrencyResult> result = null)
        {
            FubCurrency.AddPlayerCurrerncy(currency, amount, success => {
                // update/get values
                GetCurrencies(onGet => {
                    if (onGet.IsSuccess)
                    {
                        if (success.Error == null)
                        {
                            var callback = new CBSUpdateCurrencyResult
                            {
                                IsSuccess = true,
                                CurrencyCode = currency,
                                CurrentValue = GetFromCache(currency)
                            };
                            OnCurrencyUpdated?.Invoke(callback);
                            result?.Invoke(callback);
                        }
                        else
                        {
                            var callback = new CBSUpdateCurrencyResult
                            {
                                IsSuccess = false,
                                Error = SimpleError.FromTemplate(success.Error)
                            };
                            result?.Invoke(callback);
                        }
                    }
                    else
                    {
                        var callback = new CBSUpdateCurrencyResult { 
                            IsSuccess = false, 
                            Error = onGet.Error 
                        };
                        result?.Invoke(callback);
                    }
                });
            }, 
            onError => {
                var callback = new CBSUpdateCurrencyResult { 
                    IsSuccess = false, 
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        public void DecreaseUserCurrency(int amount, string currency, Action<CBSUpdateCurrencyResult> result = null)
        {
            FubCurrency.DecreasePlayerCurrerncy(currency, amount, success => {
                // update/get values
                GetCurrencies(onGet => {
                    if (onGet.IsSuccess)
                    {
                        if (success.Error == null)
                        {
                            var callback = new CBSUpdateCurrencyResult
                            {
                                IsSuccess = true,
                                CurrencyCode = currency,
                                CurrentValue = GetFromCache(currency)
                            };
                            OnCurrencyUpdated?.Invoke(callback);
                            result?.Invoke(callback);
                        }
                        else
                        {
                            var callback = new CBSUpdateCurrencyResult
                            {
                                IsSuccess = false,
                                Error = SimpleError.FromTemplate(success.Error)
                            };
                            result?.Invoke(callback);
                        }
                    }
                    else
                    {
                        var callback = new CBSUpdateCurrencyResult { 
                            IsSuccess = false, 
                            Error = onGet.Error 
                        };
                        result?.Invoke(callback);
                    }
                });
            }, 
            onError => {
                var callback = new CBSUpdateCurrencyResult {
                    IsSuccess = false, 
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        public void GetPacks(Action<CBSGetPacksResult> result)
        {
            FabProducts.GetCatalogItems(CBSConstants.CurrencyCatalogID, success => {
                var itemList = new List<CurrencyPack>();
                foreach (var item in success.Catalog)
                {
                    itemList.Add(new CurrencyPack(item));
                }
                result?.Invoke(new CBSGetPacksResult {
                    IsSuccess = true,
                    Packs = itemList
                });
            }, onError => {
                var callback = new CBSGetPacksResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        public void GetPacksByTag(string tag, Action<CBSGetPacksResult> result)
        {
            FabProducts.GetCatalogItems(CatalogID, success => {
                var itemList = new List<CurrencyPack>();
                foreach (var item in success.Catalog)
                {
                    itemList.Add(new CurrencyPack(item));
                }

                result?.Invoke(new CBSGetPacksResult
                {
                    IsSuccess = true,
                    Packs = itemList.Where(x => x.Tag == tag).ToList()
                });
            }, onError => {
                var callback = new CBSGetPacksResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        public void GrantPack(string packID, Action<CBSGrandPackResult> result)
        {
            FabProducts.GrandBundle(packID, CatalogID, success => {

                if (success.Error == null)
                {
                    string rawData = success.FunctionResult.ToString();
                    var functionResult = JsonUtility.FromJson<GrantItemsToUserResult>(rawData);
                    bool successOperation = functionResult.ItemGrantResults.FirstOrDefault().Result;
                    if (successOperation)
                    {
                        GetPacks(onGet =>
                        {
                            GetCurrencies(onGetCurrencies => {
                                var packs = onGet.Packs;
                                var currentPack = packs.FirstOrDefault(x => x.PackID == packID);
                                if (currentPack != null)
                                {
                                    foreach (var currency in currentPack.Currencies)
                                    {
                                        OnCurrencyUpdated?.Invoke(new CBSUpdateCurrencyResult
                                        {
                                            IsSuccess = true,
                                            CurrencyCode = currency.Key,
                                            CurrentValue = GetFromCache(currency.Key)
                                        });
                                    }
                                }
                            });
                            var callback = new CBSGrandPackResult
                            {
                                IsSuccess = true,
                                packID = packID
                            };
                            result?.Invoke(callback);
                        });
                    }
                    else
                    {
                        var callback = new CBSGrandPackResult
                        {
                            IsSuccess = false,
                            Error = SimpleError.FailedToGrandPack()
                        };
                        result?.Invoke(callback);
                    }
                }
                else
                {
                    var callback = new CBSGrandPackResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.FromTemplate(success.Error)
                    };
                    result?.Invoke(callback);
                }
            }, onError => {
                var callback = new CBSGrandPackResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(callback);
            });
        }

        // internal
        internal void ParseCurrency(Dictionary<string, int> currencyList)
        {
            CacheCurrencies = currencyList;
        }

        internal void ChangeRequest(string code)
        {
            GetCurrencies(onGet =>
            {
                OnCurrencyUpdated?.Invoke(new CBSUpdateCurrencyResult
                {
                    IsSuccess = true,
                    CurrencyCode = code,
                    CurrentValue = GetFromCache(code)
                });
            });
        }

        internal void ChangeRequest(string [] codes)
        {
            GetCurrencies(onGet =>
            {
                foreach (var currency in codes)
                {
                    OnCurrencyUpdated?.Invoke(new CBSUpdateCurrencyResult
                    {
                        IsSuccess = true,
                        CurrencyCode = currency,
                        CurrentValue = GetFromCache(currency)
                    });
                }
            });
        }

        protected override void OnLogout()
        {
            CacheCurrencies = null;
        }

        public int GetFromCache(string code)
        {
            if (CacheCurrencies.ContainsKey(code))
                return CacheCurrencies[code];
            else
                return 0;
        }

        // events
        private void OnUserLogined(CBSLoginResult result)
        {
            if (result.IsSuccess)
            {
                if (AuthData.PreloadCurrency)
                {
                    var currency = result.Result.InfoResultPayload.UserVirtualCurrency;
                    ParseCurrency(currency);
                }
            }
        }
    }

    [Serializable]
    internal class GrandCurrencyPackResult
    {
        public GrandItemObject Result;
    }

    [Serializable]
    public class GrantItemsToUserResult
    {
        public List<GrantedItemInstance> ItemGrantResults;
    }

    [Serializable]
    public class GrantedItemInstance
    {
        public string Annotation;
        public List<string> BundleContents;
        public string BundleParent;
        public string CatalogVersion;
        public string CharacterId;
        public Dictionary<string, string> CustomData;
        public string DisplayName;
        public DateTime? Expiration;
        public string ItemClass;
        public string ItemId;
        public string ItemInstanceId;
        public string PlayFabId;
        public DateTime? PurchaseDate;
        public int? RemainingUses;
        public bool Result;
        public string UnitCurrency;
        public uint UnitPrice;
        public int? UsesIncrementedBy;
    }

    public struct CBSGetCurrenciesResult
    {
        public bool IsSuccess;
        public PlayFab.ClientModels.GetUserInventoryResult Result;
        public SimpleError Error;
        public Dictionary<string, int> Currencies;
    }

    public struct CBSUpdateCurrencyResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string CurrencyCode;
        public int CurrentValue;
    }

    public struct CBSGetPacksResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public List<CurrencyPack> Packs;
    }

    public struct CBSGrandPackResult
    {
        public bool IsSuccess;
        public SimpleError Error;
        public string packID;
    }
}
