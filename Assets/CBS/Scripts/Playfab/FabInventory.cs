using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace CBS.Playfab
{
    public class FabInventory : FabExecuter, IFabInventory
    {
        public void GetInventory(Action<GetUserInventoryResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(request, OnGet, OnFailed);
        }

        public void RemoveInventoryItem(string inventoryID, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnRemove, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.RemoveInventoryItemMethod,
                FunctionParameter = new
                {
                    item = inventoryID
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnRemove, OnFailed);
        }

        public void ConsumeItem(string instanceID, int count, Action<ConsumeItemResult> OnConsume, Action<PlayFabError> OnFailed)
        {
            var request = new ConsumeItemRequest
            {
                ItemInstanceId = instanceID,
                ConsumeCount = count
            };
            PlayFabClientAPI.ConsumeItem(request, OnConsume, OnFailed);
        }

        public void SetItemCustomData(UpdateInventoryCustomDataRequest requestData, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.UpdateInventoryItemData,
                FunctionParameter = new
                {
                    item = requestData.InventoryItemID,
                    characterID = requestData.CharacterID,
                    dataKey = requestData.DataKey,
                    dataValue = requestData.DataValue
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void UnlockContainer(string instanceID, Action<UnlockContainerItemResult> OnUnlock, Action<PlayFabError> OnFailed)
        {
            var request = new UnlockContainerInstanceRequest
            {
                ContainerItemInstanceId = instanceID
            };
            PlayFabClientAPI.UnlockContainerInstance(request, OnUnlock, OnFailed);
        }
    }

    public struct UpdateInventoryCustomDataRequest
    {
        public string InventoryItemID;
        public string CharacterID;
        public string DataKey;
        public string DataValue;
    }
}
