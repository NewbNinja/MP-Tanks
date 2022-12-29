using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabItems
    {
        void GetCatalogItems(string catalog, Action<GetCatalogItemsResult> OnGet, Action<PlayFabError> OnFailed);

        void PurchaseItem(CBSPurchaseRequest requestData, Action<PurchaseItemResult> OnPurchase, Action<PlayFabError> OnFailed);

        void GrandItems(string[] itemsIDs, string catalogVersion, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void GrandBundle(string itemsID, string catalogVersion, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void GrandRegistrationPrize(string playerId, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
    }
}
