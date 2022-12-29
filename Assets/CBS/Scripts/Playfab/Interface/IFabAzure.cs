using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabAzure
    {
        void GetDataFromTable(AzureGetDataRequest getRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void InsertDataToTable(AzureInsertDataRequest insertRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void UpdateTableData(AzureUpdateDataRequest insertRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void DeleteTableData(AzureDeleteDataRequest deleteRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);

        void GetAllTables(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed);
    }
}
