using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace CBS.Playfab
{
    public class FabItems : FabExecuter, IFabItems
    {
        public void GetCatalogItems(string catalog, Action<GetCatalogItemsResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new GetCatalogItemsRequest{ CatalogVersion = catalog };
            PlayFabClientAPI.GetCatalogItems(request, OnGet, OnFailed);
        }

        public void PurchaseItem(CBSPurchaseRequest requestData, Action<PurchaseItemResult> OnPurchase, Action<PlayFabError> OnFailed)
        {
            var request = new PurchaseItemRequest {
                ItemId = requestData.ItemID,
                VirtualCurrency = requestData.CurrencyCode,
                Price = requestData.CurrencyValue,
                CatalogVersion = requestData.Catalog
            };
            PlayFabClientAPI.PurchaseItem(request, OnPurchase, OnFailed);
        }

        public void GrandItems(string [] itemsIDs, string catalogVersion, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            var itemsRawData = jsonPlugin.SerializeObject(itemsIDs);

            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GrantItemMethod,
                FunctionParameter = new
                {
                    items = itemsRawData,
                    catalogID = catalogVersion
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void GrandBundle(string itemsID, string catalogVersion, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GrantBundleMethod,
                FunctionParameter = new
                {
                    item = itemsID,
                    catalogID = catalogVersion
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void GrandRegistrationPrize(string playerId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.GrantRegistrationPrizeMethod,
                FunctionParameter = new
                {
                    playerID = playerId,
                    levelGroupId = CBSConstants.LevelTitleKey
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }
    }

    [Serializable]
    public class GrandItemObject
    {
        public GrandItemResult [] ItemGrantResults;
    }

    [Serializable]
    public class GrandItemResult
    {
        public string PlayFabId;
        public bool Result;
        public string ItemId;
        public string ItemInstanceId;
        public string PurchaseDate;
        public string CatalogVersion;
        public string DisplayName;
        public string UnitPrice;
    }

    public struct CBSPurchaseRequest
    {
        public string Catalog;
        public string ItemID;
        public string CurrencyCode;
        public int CurrencyValue;
    }
}
