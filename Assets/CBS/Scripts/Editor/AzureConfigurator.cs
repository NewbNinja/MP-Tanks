#if ENABLE_PLAYFABADMIN_API
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.AdminModels;
using UnityEditor;
using System.Linq;
using System;
using CBS.Playfab;
using UnityEngine.Networking;
using CBS.Scriptable;

namespace CBS.Editor
{
    public class AzureConfigurator : BaseConfigurator
    {
        protected override string Title => "Azure Congiguration";

        protected override bool DrawScrollView => true;

        private string AzureAPIKey { get; set; }
        private string AzureStorage { get; set; }
        private string FunctionMasterKey { get; set; }
        private string FunctionURL { get; set; }

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            GetApiKey();
        }

        protected override void OnDrawInside()
        {
            var levelTitleStyle = new GUIStyle(GUI.skin.label);
            levelTitleStyle.fontStyle = FontStyle.Bold;
            levelTitleStyle.fontSize = 12;
            // draw apa key
            EditorGUILayout.LabelField("Azure SAS Token", levelTitleStyle);
            GUILayout.BeginHorizontal();
            AzureAPIKey = EditorGUILayout.TextField(AzureAPIKey, new GUILayoutOption[] { GUILayout.Width(900) });
            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12))
            {
                SaveAzureKey(AzureAPIKey);
            }
            GUILayout.EndHorizontal();
            // draw storage
            EditorGUILayout.LabelField("Azure Storage", levelTitleStyle);
            GUILayout.BeginHorizontal();
            AzureStorage = EditorGUILayout.TextField(AzureStorage, new GUILayoutOption[] { GUILayout.Width(900) });
            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12))
            {
                SaveAzureStorage(AzureStorage);
            }
            GUILayout.EndHorizontal();
            // draw master key
            EditorGUILayout.LabelField("Azure Master Key", levelTitleStyle);
            GUILayout.BeginHorizontal();
            FunctionMasterKey = EditorGUILayout.TextField(FunctionMasterKey, new GUILayoutOption[] { GUILayout.Width(900) });
            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12))
            {
                SaveMasterKey(FunctionMasterKey);
            }
            GUILayout.EndHorizontal();
            // draw function URL
            EditorGUILayout.LabelField("Azure Function URL", levelTitleStyle);
            GUILayout.BeginHorizontal();
            FunctionURL = EditorGUILayout.TextField(FunctionURL, new GUILayoutOption[] { GUILayout.Width(900) });
            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12))
            {
                SaveFunctionURL(FunctionURL);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (EditorUtils.DrawButton("Check", EditorData.AddColor, 12, new GUILayoutOption[] { GUILayout.Width(100) }))
            {
                string fullUrl = "https://" + AzureStorage + ".table.core.windows.net/Tables/" + AzureAPIKey;
                var request = UnityWebRequest.Get(fullUrl);
                request.SetRequestHeader("content-Type", "application/json;odata=nometadata");
                var operation = request.SendWebRequest();
                operation.completed += (result) => {
                    string rawData = request.downloadHandler.text;
                    if (request.isHttpError || request.isNetworkError || string.IsNullOrEmpty(rawData))
                    {
                        EditorUtility.DisplayDialog("Falied", "There is something wrong with your configurations", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Success", "Azure is configured correctly!", "OK");
                    }
                };
                
            }
        }

        private void GetApiKey()
        {
            ShowProgress();
            var keys = new List<string>();
            keys.Add(CBSConstants.AzureKey);
            keys.Add(CBSConstants.AzureStorageKey);
            keys.Add(CBSConstants.FunctionMasterKey);
            keys.Add(CBSConstants.FunctionURLKey);
            var request = new GetTitleDataRequest {
                Keys = keys
            };
            PlayFabAdminAPI.GetTitleInternalData(request, OnInternalDataGetted, OnGetDataFailed);
        }

        private void OnInternalDataGetted(GetTitleDataResult result)
        {
            HideProgress();
            var dictionary = result.Data;
            bool keyExist = dictionary.ContainsKey(CBSConstants.AzureKey);
            AzureAPIKey = keyExist ? dictionary[CBSConstants.AzureKey] : string.Empty;
            bool storageExist = dictionary.ContainsKey(CBSConstants.AzureStorageKey);
            AzureStorage = storageExist ? dictionary[CBSConstants.AzureStorageKey] : string.Empty;
            bool masterKeyExist = dictionary.ContainsKey(CBSConstants.FunctionMasterKey);
            FunctionMasterKey = masterKeyExist ? dictionary[CBSConstants.FunctionMasterKey] : string.Empty;
            bool functionsURLKeyExist = dictionary.ContainsKey(CBSConstants.FunctionURLKey);
            FunctionURL = functionsURLKeyExist ? dictionary[CBSConstants.FunctionURLKey] : string.Empty;
        }

        private void OnGetDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        private void SaveAzureStorage(string storage)
        {
            ShowProgress();
            var request = new SetTitleDataRequest
            {
                Key = CBSConstants.AzureStorageKey,
                Value = storage
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSetAzureData, OnSetDataFailed);
        }

        private void SaveMasterKey(string masterKey)
        {
            ShowProgress();
            var request = new SetTitleDataRequest
            {
                Key = CBSConstants.FunctionMasterKey,
                Value = masterKey
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSetAzureData, OnSetDataFailed);
        }

        private void SaveFunctionURL(string functionURL)
        {
            ShowProgress();
            var request = new SetTitleDataRequest
            {
                Key = CBSConstants.FunctionURLKey,
                Value = functionURL
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSetAzureData, OnSetDataFailed);
        }

        private void SaveAzureKey(string key)
        {
            ShowProgress();
            var request = new SetTitleDataRequest
            {
                Key = CBSConstants.AzureKey,
                Value = key
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSetAzureData, OnSetDataFailed);
        }

        private void OnSetAzureData(SetTitleDataResult result)
        {
            Debug.Log("OnSetAzureData "+result.Request.ToJson());
            HideProgress();
            GetApiKey();
        }

        private void OnSetDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }
    }
}
#endif
