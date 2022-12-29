using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabAzure : FabExecuter, IFabAzure
    {
        public void GetDataFromTable(AzureGetDataRequest getRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AzureGetTableMethod,
                FunctionParameter = new
                {
                    tableID = getRequest.TableId,
                    nTop = getRequest.nTop,
                    partitionKey = getRequest.PartitionKey,
                    rowKey = getRequest.RowKey
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void InsertDataToTable(AzureInsertDataRequest insertRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AzureInsertDataMethod,
                FunctionParameter = new
                {
                    tableID = insertRequest.TableId,
                    rowKey = insertRequest.RowKey,
                    partitionKey = insertRequest.PartitionKey,
                    rawData = insertRequest.RawData,
                    createTable = insertRequest.CreateTableIfNotExist
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void UpdateTableData(AzureUpdateDataRequest updateRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AzureUpdateDataMethod,
                FunctionParameter = new
                {
                    tableID = updateRequest.TableId,
                    rowKey = updateRequest.RowKey,
                    partitionKey = updateRequest.PartitionKey,
                    rawData = updateRequest.RawData
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void DeleteTableData(AzureDeleteDataRequest deleteRequest, Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AzureDeleteDataMethod,
                FunctionParameter = new
                {
                    tableID = deleteRequest.TableId,
                    rowKey = deleteRequest.RowKey,
                    partitionKey = deleteRequest.PartitionKey
                }
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }

        public void GetAllTables(Action<PlayFab.CloudScriptModels.ExecuteFunctionResult> OnUpdate, Action<PlayFabError> OnFailed)
        {
            var request = new PlayFab.CloudScriptModels.ExecuteFunctionRequest
            {
                FunctionName = AzureFunctions.AzureGetTablesMethod,
                FunctionParameter = new {}
            };
            PlayFabCloudScriptAPI.ExecuteFunction(request, OnUpdate, OnFailed);
        }
    }

    [Serializable]
    public class TableResult
    {
        public List<TableValue> value;
    }

    [Serializable]
    public class TableValue
    {
        public string PartitionKey;
        public string RowKey;
        public string Timestamp;
        public string RawData;
    }

    public struct AzureInsertDataRequest
    {
        public string TableId;
        public object RowKey;
        public object PartitionKey;
        public string RawData;
        public bool CreateTableIfNotExist;
    }

    public struct AzureGetDataRequest
    {
        public string TableId;
        public int? nTop;
        public string PartitionKey;
        public string RowKey;
    }

    public struct AzureUpdateDataRequest
    {
        public string TableId;
        public string RowKey;
        public string PartitionKey;
        public string RawData;
    }

    public struct AzureDeleteDataRequest
    {
        public string TableId;
        public string RowKey;
        public string PartitionKey;
    }
}
