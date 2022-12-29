
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
using CBS.Scriptable;
using System.IO;
using PlayFab.CloudScriptModels;

namespace CBS.Editor
{
    public class PlayfabConfigurator : BaseConfigurator
    {
        private readonly string PlayfabCloudScriptPath = "Assets/CBS/Scripts/ServerScript/PlayfabServer.txt";
        private readonly string PlayfabCustomCloudScriptPath = "Assets/CBS_External/ServerScript/CustomServerScript.txt";
        private readonly string PlayfabCustomCloudPath = "Assets/CBS_External/ServerScript";

        protected override string Title => "Playfab Congiguration";

        protected override bool DrawScrollView => true;

        private EditorData EditorData { get; set; }

        private PlayfabData PlayfabData { get; set; }

        private string TitleEntityToken;

        private string FunctionMasterKey { get; set; }

        private string FunctionURL { get; set; }

        private string ProgressTitle { get; set; }
        private float ProgressValue { get; set; }
        private bool IsShowFunctionProgress { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            PlayfabData = CBSScriptable.Get<PlayfabData>();
        }

        protected override void OnDrawInside()
        {
            var levelTitleStyle = new GUIStyle(GUI.skin.label);
            levelTitleStyle.fontStyle = FontStyle.Bold;
            levelTitleStyle.fontSize = 12;

            EditorGUILayout.LabelField("Azure Functions", levelTitleStyle);

            GUILayout.Space(10);

            if (EditorUtils.DrawButton("Register Azure Functions", EditorData.AddColor, 12, new GUILayoutOption[] { GUILayout.Width(250) }))
            {
                RegisterAzureFunctions();
            }

            GUILayout.Space(10);

            if (EditorUtils.DrawButton("Import Azure Functions Project", EditorData.AddColor, 12, new GUILayoutOption[] { GUILayout.Width(250) }))
            {
                ImportAzureFunctionsProject();
            }

            if (IsShowFunctionProgress)
            {
                EditorUtility.DisplayProgressBar("Register Azure Functions", ProgressTitle, ProgressValue);
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ImportAzureFunctionsProject()
        {
            ShowProgress();
            try
            {
                ZipUtils.UnzipAzureProject();
                //EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
                EditorUtility.DisplayDialog("Success", "Importing Azure Functions Project finished!", "Ok");
            }
            catch 
            {
                EditorUtility.DisplayDialog("Failed", "Failed to import Azure Functions project!", "Ok");
            }
            HideProgress();
        }

        private void RegisterAzureFunctions()
        {
            var allMethods = AzureFunctions.AllMethods;

            ShowFunctionProgress("Playfab login", 0);

            GetEntityToken(() => {
                // get azure data
                var keys = new List<string>();
                keys.Add(CBSConstants.FunctionMasterKey);
                keys.Add(CBSConstants.FunctionURLKey);
                var request = new GetTitleDataRequest
                {
                    Keys = keys
                };
                PlayFabAdminAPI.GetTitleInternalData(request, OnInternalDataGetted=> {
                    var dictionary = OnInternalDataGetted.Data;
                    bool masterKeyExist = dictionary.ContainsKey(CBSConstants.FunctionMasterKey);
                    FunctionMasterKey = masterKeyExist ? dictionary[CBSConstants.FunctionMasterKey] : string.Empty;
                    bool functionsURLKeyExist = dictionary.ContainsKey(CBSConstants.FunctionURLKey);
                    FunctionURL = functionsURLKeyExist ? dictionary[CBSConstants.FunctionURLKey] : string.Empty;

                    if (!masterKeyExist || !functionsURLKeyExist)
                    {
                        HideFunctionsPreogress();
                        EditorUtility.DisplayDialog("Falied", "There is something wrong with your Azure configurations", "OK");
                    }
                    else
                    {
                        RegisterFunctionLoop(0);
                    }
                    
                }, OnGetDataFailed=> {
                    AddErrorLog(OnGetDataFailed);
                    HideFunctionsPreogress();
                });
            });
        }

        private void RegisterFunctionLoop(int index)
        {
            var allMethods = AzureFunctions.AllMethods;
            var titleID = PlayFabSettings.TitleId;

            if (index >= allMethods.Count)
            {
                HideFunctionsPreogress();
                EditorUtility.DisplayDialog("Success", "Register Azure Function finished!", "Ok");
            }
            else
            {
                var functionName = allMethods[index];

                var registerRequest = new RegisterHttpFunctionRequest
                {
                    FunctionName = functionName,
                    FunctionUrl = AzureFunctions.GetFunctionFullURL(FunctionURL, functionName, FunctionMasterKey),
                    AuthenticationContext = new PlayFabAuthenticationContext
                    {
                        EntityToken = TitleEntityToken,
                        PlayFabId = PlayFabSettings.TitleId
                    }
                };

                var progressValue = (float)index / (float)allMethods.Count;
                ShowFunctionProgress("Register " + functionName, progressValue);

                PlayFabCloudScriptAPI.RegisterHttpFunction(registerRequest, onRegister => {
                    Debug.Log("onRegister = "+ functionName);
                    index++;
                    RegisterFunctionLoop(index);
                }, onFailed => {
                    HideFunctionsPreogress();
                    Debug.LogError(onFailed.ErrorMessage);
                    OnCloudScriptFailed(onFailed);
                });
            }
        }

        private void GetEntityToken(Action onGet)
        {
            var request = new PlayFab.AuthenticationModels.GetEntityTokenRequest {};
            PlayFabAuthenticationAPI.ForgetAllCredentials();

            TitleEntityToken = null;

            PlayFabAuthenticationAPI.GetEntityToken(
                request,
                result =>
                {
                    Debug.Log("On Get Entity Token " + result.EntityToken);
                    TitleEntityToken = result.EntityToken;
                    onGet?.Invoke();
                },
                error =>
                {
                    AddErrorLog(error);
                    HideFunctionsPreogress();
                }
            );
        }

        private string GetCloudScriptText()
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PlayfabCloudScriptPath);
            return textAsset == null ? string.Empty : textAsset.text;
        }

        private string GetCloudCustomScriptText()
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PlayfabCustomCloudScriptPath);
            return textAsset == null ? string.Empty : textAsset.text;
        }

        private void UploadCloudScript()
        {
            var scriptText = GetCloudScriptText();

            var cloudFile = new CloudScriptFile {
                FileContents = scriptText,
                Filename = "PlayfabServer"
            };

            var fileList = new List<CloudScriptFile>();
            fileList.Add(cloudFile);

            if (PlayfabData.EnableCustomCloudScript)
            {
                CreateCustomScriptIfNotExist();
                var customScriptText = GetCloudCustomScriptText();
                var customCloudFile = new CloudScriptFile
                {
                    FileContents = customScriptText,
                    Filename = "PlayfabCustomServer"
                };
                fileList.Add(customCloudFile);
            }

            var request = new UpdateCloudScriptRequest {
                Files = fileList,
                Publish = true
            };
            PlayFabAdminAPI.UpdateCloudScript(request, OnCloudScriptUpdated, OnCloudScriptFailed);
            
            ShowProgress();
        }

        private void OnCloudScriptUpdated(UpdateCloudScriptResult result)
        {
            HideProgress();
            EditorUtility.DisplayDialog("Success", "Upload cloud script finished!", "Ok");
            Debug.Log("OnCloudScriptUpdated ");
        }

        private void OnCloudScriptFailed(PlayFabError error)
        {
            HideProgress();
            EditorUtility.DisplayDialog("PlayFab Error", "Failed to upload cloud script. "+ error.ErrorMessage, "Ok");
            Debug.Log("OnCloudScriptFailed " + error.ErrorMessage);
        }

        private void CreateCustomScriptIfNotExist()
        {
            var pathExist = Directory.Exists(PlayfabCustomCloudPath);
            if (!pathExist)
            {
                var directory = Directory.CreateDirectory(PlayfabCustomCloudPath);
                AssetDatabase.Refresh();
            }

            var pathToAsset = PlayfabCustomCloudPath + "/" + "CustomServerScript.txt";
            var fileExists = File.Exists(pathToAsset);

            if (!fileExists)
            {
                var asset = Environment.NewLine + "// custom cloud script" + Environment.NewLine;
                File.WriteAllText(pathToAsset, asset);
                AssetDatabase.SaveAssets();

                AssetDatabase.Refresh();
            }
        }

        private void ShowFunctionProgress(string title, float progress)
        {
            ProgressValue = progress;
            ProgressTitle = title;
            ShowProgress();
            IsShowFunctionProgress = true;
        }

        private void HideFunctionsPreogress()
        {
            IsShowFunctionProgress = false;
            HideProgress();
        }
    }
}
#endif
