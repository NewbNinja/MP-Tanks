/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

namespace SIS
{
    class IAPSettingsProvider : SettingsProvider
    {
        private SerializedObject serializedObject;
        private IAPScriptableObject asset;

        int toolbarIndex = 0;
        string[] toolbarNames = new string[] { "Setup", "Tools", "About" };
        string errorMessage = "";

        public bool isChanged = false;
        public bool isPackageImported = false;
        public bool isIAPEnabled = false;
        public ListRequest pckList;

        private BuildTargetIAP targetIAPGroup;
        private string[] iapNames = new string[] { "", "SIS_IAP" };

        private DesktopPlugin desktopPlugin = DesktopPlugin.UnityIAP;
        private WebPlugin webPlugin = WebPlugin.UnityIAP;

        private string packagesPath;
        private enum ExtensionPackages
        {
            VR = 0,
            PlayFab = 1
        }

        private UIAssetPlugin uiPlugin = UIAssetPlugin.UnityUI;

        private bool customStoreFoldout = false; 
        private string databaseContent;

        class Styles
        {
            public static GUIContent Info = new GUIContent("Welcome! This section contains the billing setup, store settings and other tools for Simple IAP System. When you are happy with your billing configuration, " +
                                                           "expand this section in the menu on the left, in order to define your in-app purchase categories and products.");
        }

        public IAPSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }


        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            serializedObject = IAPScriptableObject.GetSerializedSettings();
            asset = serializedObject.targetObject as IAPScriptableObject;

            var script = MonoScript.FromScriptableObject(asset);
            string thisPath = AssetDatabase.GetAssetPath(script);
            packagesPath = thisPath.Replace("/Scripts/IAPScriptableObject.cs", "/Editor/Packages/");

            GetScriptingDefines();
            GetDatabaseContent();
        }


        public override void OnDeactivate()
        {
            AssetDatabase.SaveAssets();
        }


        public override void OnGUI(string searchContext)
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(Styles.Info.text, MessageType.None);
            GUILayout.Space(5);

            DrawListElement();

            EditorUtility.SetDirty(serializedObject.targetObject);
            serializedObject.ApplyModifiedProperties();
        }


        void DrawListElement()
        {
            toolbarIndex = GUILayout.Toolbar(toolbarIndex, toolbarNames);

            switch (toolbarIndex)
            {
                case 0:
                    DrawToolBar0();
                    break;
                case 1:
                    DrawToolBar1();
                    break;
                case 2:
                    DrawToolBar2();
                    break;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();

                GUI.color = Color.yellow;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUI.color = Color.white;
                EditorGUILayout.HelpBox(errorMessage, MessageType.Warning, true);
                if (GUILayout.Button("OK", GUILayout.Width(80), GUILayout.Height(38)))
                {
                    errorMessage = string.Empty;
                }

                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;
                EditorGUILayout.Space();
            }
        }


        void DrawToolBar0()
        {
            EditorGUILayout.Space();
            DrawBillingSetup();

            EditorGUILayout.Space(20);
            DrawCustomExtensions();

            EditorGUILayout.Space(20);
            DrawStoreExtensions();
        }


        void DrawBillingSetup()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Unity IAP", EditorStyles.boldLabel);
            //Check Unity PackageManager package
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Package Imported:", GUILayout.Width(IAPSettingsStyles.buttonWidth));
            if (pckList == null || !pckList.IsCompleted)
            {
                GUILayout.Label("CHECKING...");
            }
            else if (!isPackageImported)
            {
                PackageCollection col = pckList.Result;
                foreach (UnityEditor.PackageManager.PackageInfo info in col)
                {
                    if (info.packageId.StartsWith("com.unity.purchasing", System.StringComparison.Ordinal))
                    {
                        isPackageImported = true;
                        break;
                    }
                }

                if (!isPackageImported)
                    isIAPEnabled = false;
            }

            GUILayout.Label(isPackageImported == true ? "OK" : "NOT OK");
            EditorGUILayout.EndHorizontal();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                bool unityIAPActive = isPackageImported;
                GUI.enabled = unityIAPActive;
                isIAPEnabled = EditorGUILayout.Toggle("Enable Integration:", isIAPEnabled);
                EditorGUILayout.EndVertical();

                GUI.enabled = true;
                GUI.enabled = unityIAPActive == true ? isIAPEnabled : false;

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Custom Stores", EditorStyles.boldLabel);
                desktopPlugin = (DesktopPlugin)EditorGUILayout.EnumPopup("Standalone:", desktopPlugin);
                webPlugin = (WebPlugin)EditorGUILayout.EnumPopup("WebGL:", webPlugin);

                EditorGUILayout.EndVertical();
                GUI.enabled = true;

                if (check.changed)
                {
                    isChanged = check.changed;
                }
            }

            EditorGUILayout.EndHorizontal();
            if (isChanged) GUI.color = Color.yellow;

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply"))
            {
                ApplyScriptingDefines();
                isChanged = false;
            }
            GUI.color = Color.white;
        }


        void DrawCustomExtensions()
        {
            EditorGUILayout.LabelField("Custom Extensions", EditorStyles.boldLabel);

            if (GUILayout.Button("Virtual Reality") && EditorUtility.DisplayDialog("VR Package Import", "This imports an extension package into your project. Please confirm.", "Ok", "Cancel"))
            {
                AssetDatabase.ImportPackage(packagesPath + ExtensionPackages.VR.ToString() + ".unitypackage", true);
            }

            if (GUILayout.Button("PlayFab (External SDK)") && EditorUtility.DisplayDialog("PlayFab Package Import", "This imports an extension package into your project. Please confirm.", "Ok", "Cancel"))
            {
                AssetDatabase.ImportPackage(packagesPath + ExtensionPackages.PlayFab.ToString() + ".unitypackage", true);
            }
        }


        void DrawStoreExtensions()
        {
            EditorGUILayout.LabelField("Store Extensions", EditorStyles.boldLabel);
            customStoreFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(customStoreFoldout, "PayPal");
            if (customStoreFoldout)
            {
                asset.customStoreConfig.PayPal.enabled = EditorGUILayout.Toggle("Enabled:", asset.customStoreConfig.PayPal.enabled);

                GUI.enabled = asset.customStoreConfig.PayPal.enabled;
                asset.customStoreConfig.PayPal.currencyCode = EditorGUILayout.TextField("Currency Code:", asset.customStoreConfig.PayPal.currencyCode);
                asset.customStoreConfig.PayPal.sandbox.clientID = EditorGUILayout.TextField("Sandbox Client ID:", asset.customStoreConfig.PayPal.sandbox.clientID);
                asset.customStoreConfig.PayPal.sandbox.secretKey = EditorGUILayout.TextField("Sandbox Secret:", asset.customStoreConfig.PayPal.sandbox.secretKey);
                asset.customStoreConfig.PayPal.live.clientID = EditorGUILayout.TextField("Live Client ID:", asset.customStoreConfig.PayPal.live.clientID);
                asset.customStoreConfig.PayPal.live.secretKey = EditorGUILayout.TextField("Live Secret:", asset.customStoreConfig.PayPal.live.secretKey);
                asset.customStoreConfig.PayPal.returnUrl = EditorGUILayout.TextField("Return URL:", asset.customStoreConfig.PayPal.returnUrl);
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            //PayPal input validation
            if(asset.customStoreConfig.PayPal.enabled)
            {
                if(string.IsNullOrEmpty(asset.customStoreConfig.PayPal.returnUrl) || !asset.customStoreConfig.PayPal.returnUrl.StartsWith("http"))
                    errorMessage = "PayPal is enabled. Return URL value needs to be a valid web endpoint starting with http or https.";
            }
        }


        void DrawToolBar1()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Backup", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import from JSON"))
            {
                string path = EditorUtility.OpenFolderPanel("Import IAP Settings from JSON", "", "");
                if (path.Length != 0)
                {
                    asset.currencyList = IAPSettingsExporter.FromJSONCurrency(File.ReadAllText(path + "/SimpleIAPSystem_Currencies.json"));
                    asset.categoryList = IAPSettingsExporter.FromJSONCategory(File.ReadAllText(path + "/SimpleIAPSystem_IAPSettings.json"));
                    asset.productList = IAPSettingsExporter.FromJSONProduct(File.ReadAllText(path + "/SimpleIAPSystem_IAPSettings.json"));
                    return;
                }
            }

            if (GUILayout.Button("Export to JSON"))
            {
                string path = EditorUtility.SaveFolderPanel("Save IAP Settings as JSON", "", "");
                if (path.Length != 0)
                {
                    File.WriteAllBytes(path + "/SimpleIAPSystem_IAPSettings.json", System.Text.Encoding.UTF8.GetBytes(IAPSettingsExporter.ToJSON(asset.productList)));
                    File.WriteAllBytes(path + "/SimpleIAPSystem_IAPSettings_PlayFab.json", System.Text.Encoding.UTF8.GetBytes(IAPSettingsExporter.ToJSON(asset.productList, true)));
                    File.WriteAllBytes(path + "/SimpleIAPSystem_Currencies.json", System.Text.Encoding.UTF8.GetBytes(IAPSettingsExporter.ToJSON(asset.currencyList)));                    
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Local Storage", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
            {
                GetDatabaseContent();
            }

            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Clear Local Database Entries",
                                                "Are you sure you want to clear the PlayerPref data for this project? (This includes Simple IAP System data, but also all other PlayerPrefs)", "Clear", "Cancel"))
                {
                    string unityPurchasingPath = Path.Combine(Path.Combine(Application.persistentDataPath, "Unity"), "UnityPurchasing");
                    
                    #if SIS_IAP
                    if (Directory.Exists(unityPurchasingPath))
                        UnityEngine.Purchasing.UnityPurchasing.ClearTransactionLog();
                    #endif

                    if (Directory.Exists(unityPurchasingPath))
                        Directory.Delete(unityPurchasingPath, true);

                    DBManager.ClearAll();
                    if (DBManager.GetInstance() != null)
                        DBManager.GetInstance().Init();

                    GetDatabaseContent();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(databaseContent) ? "- no data -" : databaseContent, GUI.skin.GetStyle("HelpBox"), GUILayout.MaxHeight(150));

            EditorGUILayout.Space();
            DrawSourceCustomization();

            EditorGUILayout.Space(15);
            GUILayout.Label("Server-Side Receipt Validation", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("I am offering a receipt validation service, without requiring your own servers! It supports validation of all products types, detecting fake receipts, active/expired or billing issues within the user's subscription cycle, " +
                                "so you can always let them know to take action in order to stay subscribed. Additionally, security measures are implemented to ensure a transaction is only redeemed once across your app, preventing duplicate purchase attempts. " + 
                                "\n\nSimple IAP System can only be that cheap because of users (like you!) supporting it. Please consider using the receipt validation service to support me. There is a free plan too!", new GUIStyle(EditorStyles.label) { wordWrap = true });
                GUILayout.Space(5);

                GUI.color = Color.yellow;
                if (GUILayout.Button("Check it out!"))
                {
                    Help.BrowseURL("https://flobuk.com/validator");
                }
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
        }


        void DrawSourceCustomization()
        {
            EditorGUILayout.LabelField("Source Customization", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UI", GUILayout.Width(IAPSettingsStyles.buttonWidth));

            uiPlugin = (UIAssetPlugin)EditorGUILayout.EnumPopup(uiPlugin);

            if (GUILayout.Button("Convert", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
            {
                if (EditorUtility.DisplayDialog("Convert UI Source Code",
                                                "Are you sure you want to convert the UI references in code? If choosing a UI solution other than Unity UI, " +
                                                "all sample shop prefabs and demo scenes will break. You should not do this without a backup.", "Continue", "Cancel"))
                {
                    UISourceConverterData.Convert(uiPlugin);
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        void DrawToolBar2()
        {
            EditorGUILayout.Space();
            GUILayout.Label("Info", EditorStyles.boldLabel);
            if (GUILayout.Button("Homepage", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
            {
                Help.BrowseURL("https://flobuk.com");
            }

            if (GUILayout.Button("YouTube", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
            {
                Help.BrowseURL("https://www.youtube.com/channel/UCCY5CCgf96mbWYXawyW8RuQ");
            }

            EditorGUILayout.Space();
            GUILayout.Label("Support", EditorStyles.boldLabel);
            if (GUILayout.Button("Online Documentation", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
            {
                Help.BrowseURL("https://flobuk.gitlab.io/assets/docs/sis");
            }

            if (GUILayout.Button("Scripting Reference", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
            {
                Help.BrowseURL("https://flobuk.gitlab.io/assets/docs/sis/api/");
            }

            if (GUILayout.Button("Unity Forum", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
            {
                Help.BrowseURL("https://forum.unity3d.com/threads/194975");
            }

            EditorGUILayout.Space();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("Support me! :-)", EditorStyles.boldLabel);
                GUILayout.Label("Please consider leaving a positive rating on the Unity Asset Store. As a solo developer, each review counts! " +
                                "Your support helps me stay motivated, improving this asset and making it more popular. \n\nIf you are looking for support, please head over to the Unity forum thread instead.", new GUIStyle(EditorStyles.label) { wordWrap = true });
                GUILayout.Space(5f);

                GUI.color = Color.yellow;
                if (GUILayout.Button("Review Asset", GUILayout.Width(IAPSettingsStyles.buttonWidth)))
                {
                    Help.BrowseURL("https://assetstore.unity.com/packages/slug/192362?aid=1011lGiF&pubref=editor_sis");
                }
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
        }


        void GetScriptingDefines()
        {
            int targetBits = 0;
            foreach (var enumValue in System.Enum.GetValues(typeof(BuildTargetIAP)))
            {
                if (PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)System.Enum.Parse(typeof(BuildTargetGroup), enumValue.ToString())).Contains(iapNames[1]))
                    targetBits |= (int)enumValue;
            }

            if (targetBits > 0) isIAPEnabled = true;
            if (targetBits == 0 || targetBits == 63) targetBits = -1;
            targetIAPGroup = (BuildTargetIAP)targetBits;

            desktopPlugin = (DesktopPlugin)FindScriptingDefineIndex(BuildTargetGroup.Standalone);
            webPlugin = (WebPlugin)FindScriptingDefineIndex(BuildTargetGroup.WebGL);

            //check Unity IAP
            isPackageImported = false;

            //download PackageManager list, retrieved later
            pckList = Client.List(true);
        }


        int FindScriptingDefineIndex(BuildTargetGroup group)
        {
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup activeGroup = BuildPipeline.GetBuildTargetGroup(activeTarget);

            string str = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string[] defines = null;

            switch (group)
            {
                case BuildTargetGroup.Standalone:
                    defines = EnumHelper.GetEnumDescriptions(desktopPlugin);
                    break;
                case BuildTargetGroup.WebGL:
                    defines = EnumHelper.GetEnumDescriptions(webPlugin);
                    break;
                case BuildTargetGroup.Unknown:
                    str = PlayerSettings.GetScriptingDefineSymbolsForGroup(activeGroup);
                    break;
            }

            for (int i = 1; i < defines.Length; i++)
            {
                if (str.Contains(defines[i]))
                {
                    return i;
                }
            }

            return 0;
        }


        void ApplyScriptingDefines()
        {
            List<BuildTargetIAP> selectedElements = new List<BuildTargetIAP>();
            System.Array arrayElements = System.Enum.GetValues(typeof(BuildTargetIAP));
            for (int i = 0; i < arrayElements.Length; i++)
            {
                int layer = 1 << i;
                if (((int)targetIAPGroup & layer) != 0)
                {
                    selectedElements.Add((BuildTargetIAP)arrayElements.GetValue(i));
                }
            }

            for (int i = 0; i < selectedElements.Count; i++)
                SetScriptingDefine((BuildTargetGroup)System.Enum.Parse(typeof(BuildTargetGroup), selectedElements[i].ToString()), iapNames, isIAPEnabled ? 1 : 0);

            SetScriptingDefine(BuildTargetGroup.Standalone, EnumHelper.GetEnumDescriptions(desktopPlugin), (int)desktopPlugin);
            SetScriptingDefine(BuildTargetGroup.WebGL, EnumHelper.GetEnumDescriptions(webPlugin), (int)webPlugin);
        }


        void SetScriptingDefine(BuildTargetGroup target, string[] oldDefines, int newDefine)
        {
            string str = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            List<string> defs = new List<string>(str.Split(';'));
            if (defs.Count == 0 && !string.IsNullOrEmpty(str)) defs.Add(str);

            for (int i = 0; i < oldDefines.Length; i++)
            {
                if (string.IsNullOrEmpty(oldDefines[i])) continue;
                defs.Remove(oldDefines[i]);
            }

            if (newDefine > 0)
                defs.Add(oldDefines[newDefine]);

            str = "";
            for (int i = 0; i < defs.Count; i++)
                str = defs[i] + ";" + str;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, str);
        }


        void GetDatabaseContent()
        {
            if (Application.isPlaying && IAPManager.GetInstance() != null && DBManager.GetInstance() != null)
                databaseContent = DBManager.Read();
            else
                databaseContent = PlayerPrefs.GetString(DBManager.prefsKey, "");

            GUIUtility.keyboardControl = 0;
        }


        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new IAPSettingsProvider("Project/Simple IAP System", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }
    }
}
