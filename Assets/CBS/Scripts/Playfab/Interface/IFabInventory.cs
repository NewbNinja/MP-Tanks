using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabInventory
    {
        void GetInventory(Action<GetUserInventoryResult> OnGet, Action<PlayFabError> OnFailed);
        void RemoveInventoryItem(string inventoryID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnRemove, Action<PlayFabError> OnFailed);
        void ConsumeItem(string instanceID, int count, Action<ConsumeItemResult> OnConsume, Action<PlayFabError> OnFailed);
        void SetItemCustomData(UpdateInventoryCustomDataRequest requestData, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
        void UnlockContainer(string instanceID, Action<UnlockContainerItemResult> OnUnlock, Action<PlayFabError> OnFailed);
    }
}
