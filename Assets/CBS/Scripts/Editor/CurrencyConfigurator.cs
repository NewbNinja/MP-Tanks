#if ENABLE_PLAYFABADMIN_API
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PlayFab;
using PlayFab.AdminModels;
using CBS.Scriptable;
using System.Linq;
using CBS.Editor.Window;

namespace CBS.Editor
{
    public class CurrencyConfigurator : BaseConfigurator
    {
        protected override string Title => "Currency Configurator";

        private int SelectedToolBar { get; set; }

        private List<VirtualCurrencyData> VirtualCurrencies { get; set; }
        private List<CatalogItem> Packs;
        private List<string> CurrenciesKeys
        {
            get
            {
                if (VirtualCurrencies == null)
                    return null;
                return VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
            }
        }

        private GUILayoutOption[] AddButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(30), GUILayout.Width(120) };
            }
        }

        private bool CurrencyInited { get; set; }
        private bool PackInited { get; set; }

        private CurrencyIcons Icons { get; set; }

        protected override bool DrawScrollView => false;

        private Vector2 CurrencyScroll { get; set; }
        private Vector2 PackScroll { get; set; }

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            Icons = CBSScriptable.Get<CurrencyIcons>();
        }

        protected override void OnDrawInside()
        {
            // draw sub titles
            SelectedToolBar = GUILayout.Toolbar(SelectedToolBar, new string[] { "Currencies", "Packs" });
            switch (SelectedToolBar)
            {
                case 0:
                    if (!CurrencyInited)
                    {
                        GetAllCurrencies();
                        CurrencyInited = true;
                    }
                    DrawCurrencyConfigs();
                    break;
                case 1:
                    if (!PackInited)
                    {
                        PackInited = true;
                        GetCurrencyCatalog();
                    }
                    DrawPacksConfigs();
                    break;
                default:
                    break;
            }
        }

        private void DrawCurrencyConfigs()
        {
            if (VirtualCurrencies == null)
                return;

            CurrencyScroll = EditorGUILayout.BeginScrollView(CurrencyScroll);

            if (VirtualCurrencies.Count == 0)
            {
                GUILayout.Space(15);
                if (EditorUtils.DrawButton("Import default", EditorData.AddColor, 12, AddButtonOptions))
                {
                    ImportDefaultCurrencies();
                }
            }
            GUILayout.Space(15);
            // draw all currencies
            foreach (var currency in VirtualCurrencies)
            {
                GUILayout.Space(35);
                GUILayout.BeginHorizontal();
                // title style
                var levelTitleStyle = new GUIStyle(GUI.skin.label);
                levelTitleStyle.fontStyle = FontStyle.Bold;
                // draw icon
                GUILayout.BeginVertical();
                var actvieSprite = Icons.GetSprite(currency.CurrencyCode);
                var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                GUILayout.Button(iconTexture, GUILayout.Width(50), GUILayout.Height(50));

                GUILayout.EndVertical();
                // draw code
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField("Code", levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(50) });
                EditorGUILayout.LabelField(currency.CurrencyCode, new GUILayoutOption[] { GUILayout.MaxWidth(50) });
                GUILayout.EndVertical();
                // draw display name
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField("Display name", levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                EditorGUILayout.LabelField(currency.DisplayName, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                GUILayout.EndVertical();
                // draw initial deposit
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField("Initial Deposit", levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                EditorGUILayout.LabelField(currency.InitialDeposit.ToString(), new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                GUILayout.EndVertical();
                // draw edit button
                GUILayout.BeginVertical();
                if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12))
                {
                    AddCurrencyWindow.Show(currency, newCurrency => {
                        AddOrChangeCurrency(newCurrency);
                    }, CurrencyAction.EDIT);
                    GUIUtility.ExitGUI();
                }
                // draw remove button
                if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12))
                {
                    int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to this currency?",
                            "Yes",
                            "No",
                            string.Empty);
                    switch (option)
                    {
                        // ok.
                        case 0:
                            RemoveCurrency(currency);
                            break;
                    }
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 1, 20);
            }

            GUILayout.Space(15);
            // add new currency
            if (EditorUtils.DrawButton("Add new currency", EditorData.AddColor, 12, AddButtonOptions))
            {
                AddCurrencyWindow.Show(new VirtualCurrencyData(), newCurrency => {
                    AddOrChangeCurrency(newCurrency);
                }, CurrencyAction.ADD);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPacksConfigs()
        {
            if (Packs == null)
                return;

            CurrencyScroll = EditorGUILayout.BeginScrollView(CurrencyScroll);

            // title style
            var levelTitleStyle = new GUIStyle(GUI.skin.label);
            levelTitleStyle.fontStyle = FontStyle.Bold;
            levelTitleStyle.alignment = TextAnchor.MiddleCenter;
            levelTitleStyle.fontSize = 18;

            var levelIDStyle = new GUIStyle(GUI.skin.label);
            levelIDStyle.alignment = TextAnchor.MiddleCenter;

            for (int i=0;i< Packs.Count;i++)
            {
                var item = Packs[i];
                GUILayout.Space(15);   
                // draw display name
                EditorGUILayout.LabelField(item.DisplayName, levelTitleStyle, GUILayout.Height(25));
                // draw id
                EditorGUILayout.LabelField(item.ItemId, levelIDStyle);
                // draw icon
                GUILayout.Space(15);
                var actvieSprite = Icons.GetSprite(item.ItemId);
                var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                //GUILayout.Button(iconTexture, iconStyle, GUILayout.Width(300), GUILayout.Height(300));
                GUIContent btnTxt = new GUIContent(iconTexture);
                var rt = GUILayoutUtility.GetRect(btnTxt, GUI.skin.button, GUILayout.Width(300), GUILayout.Height(300));
                rt.center = new Vector2(ConfiguratorWindow.ContentRect.width/2, rt.center.y);
                GUI.Button(rt, btnTxt, GUI.skin.button);
                // draw currencies
                var curList = item.Bundle.BundledVirtualCurrencies;
                if (curList != null)
                {
                    var curStyle = new GUIStyle(GUI.skin.label);
                    curStyle.alignment = TextAnchor.MiddleCenter;

                    GUILayout.BeginHorizontal();
                    foreach(var cur in curList)
                    {
                        GUILayout.BeginVertical();
                        EditorGUILayout.LabelField(cur.Key, curStyle, GUILayout.Width(100));

                        var sprite = Icons.GetSprite(cur.Key);
                        var icon = sprite == null ? null : sprite.texture;
                        GUILayout.Button(icon, GUILayout.Width(100), GUILayout.Height(100));

                        EditorGUILayout.LabelField(cur.Value.ToString(), curStyle, GUILayout.Width(100));
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }

                // draw edit/remove button
                GUILayout.Space(30);
                GUILayout.BeginHorizontal();
                if (EditorUtils.DrawButton("Edit", EditorData.EditColor, 12))
                {
                    AddCurrencyPackWindow.Show(item, newPack =>
                    {
                        SaveCatalog();
                    }, CurrencyAction.EDIT, CurrenciesKeys);
                    GUIUtility.ExitGUI();
                }
                if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12))
                {
                    int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to this pack?",
                            "Yes",
                            "No",
                            string.Empty);
                    switch (option)
                    {
                        // ok.
                        case 0:
                            RemovePack(item);
                            break;
                    }
                }
                GUILayout.EndHorizontal();

                EditorUtils.DrawUILine(Color.grey, 1, 20);
            }

            // add new pack
            GUILayout.Space(30);
            if (EditorUtils.DrawButton("Add new pack", EditorData.AddColor, 12, AddButtonOptions))
            {
                AddCurrencyPackWindow.Show(new CatalogItem(), newPack =>
                {
                    AddNewPack(newPack);
                }, CurrencyAction.ADD, CurrenciesKeys);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        // fab methods

        // add new pack
        private void AddNewPack(CatalogItem pack)
        {
            Packs.Add(pack);
            SaveCatalog();
        }

        // remove pack
        private void RemovePack(CatalogItem pack)
        {
            if (Packs.Contains(pack))
            {
                Icons.RemoveSprite(pack.ItemId);
                Packs.Remove(pack);
                Packs.TrimExcess();
                OverrideCatalog();
            }
        }

        // override catalog
        private void OverrideCatalog()
        {
            ShowProgress();
            Debug.Log(Packs);
            var dataRequest = new UpdateCatalogItemsRequest
            {
                Catalog = Packs,
                CatalogVersion = CBSConstants.CurrencyCatalogID,
                SetAsDefaultCatalog = false
            };
            PlayFabAdminAPI.SetCatalogItems(dataRequest, OnCatalogUpdated, OnCatalogUpdatedFailed);
        }

        // save catalog
        private void SaveCatalog()
        {
            ShowProgress();

            var dataRequest = new UpdateCatalogItemsRequest {
                Catalog = Packs,
                CatalogVersion = CBSConstants.CurrencyCatalogID,
                SetAsDefaultCatalog = false
            };

            PlayFabAdminAPI.UpdateCatalogItems(dataRequest, OnCatalogUpdated, OnCatalogUpdatedFailed);
        }

        private void OnCatalogUpdated(UpdateCatalogItemsResult result)
        {
            Debug.Log("OnCatalogUpdated");
            HideProgress();

            GetCurrencyCatalog();
        }

        private void OnCatalogUpdatedFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.GenerateErrorReport());
            AddErrorLog(error);
        }

        // get catalog
        private void GetCurrencyCatalog()
        {
            ShowProgress();
            var dataRequest = new GetCatalogItemsRequest
            {
                CatalogVersion = CBSConstants.CurrencyCatalogID
            };

            PlayFabAdminAPI.GetCatalogItems(dataRequest, OnCurrencyCatalogGetted, OnGetCatalogFailed);
        }

        private void OnCurrencyCatalogGetted(GetCatalogItemsResult result)
        {
            HideProgress();
            Debug.Log("OnCurrencyCatalogGetted");
            Packs = result.Catalog.Where(x => x.Bundle != null).ToList();
        }

        private void OnGetCatalogFailed(PlayFabError error)
        {
            HideProgress();
            Debug.Log(error.Error);
            AddErrorLog(error);
        }

        // get currency
        private void GetAllCurrencies()
        {
            ShowProgress();

            var dataRequest = new ListVirtualCurrencyTypesRequest();

            PlayFabAdminAPI.ListVirtualCurrencyTypes(dataRequest, OnCurrenciesListGetted, OnGetCurrenciesListFailed);
        }

        private void OnCurrenciesListGetted(ListVirtualCurrencyTypesResult result)
        {
            HideProgress();
            VirtualCurrencies = result.VirtualCurrencies;
            CurrencyInited = true;
        }

        private void OnGetCurrenciesListFailed(PlayFabError error)
        {
            HideProgress();
            AddErrorLog(error);
        }

        // add new currency
        private void AddOrChangeCurrency(VirtualCurrencyData newCurrencyData)
        {
            ShowProgress();

            var newCurrencyList = new List<VirtualCurrencyData>();
            newCurrencyList.Add(newCurrencyData);

            var dataRequest = new AddVirtualCurrencyTypesRequest {
                VirtualCurrencies = newCurrencyList
            };

            PlayFabAdminAPI.AddVirtualCurrencyTypes(dataRequest, OnNewCurrencyAdded, OnAddNewCurrencyFailed);
        }

        private void AddOrChangeCurrencies(List<VirtualCurrencyData> newCurrencyList)
        {
            ShowProgress();

            var dataRequest = new AddVirtualCurrencyTypesRequest
            {
                VirtualCurrencies = newCurrencyList
            };

            PlayFabAdminAPI.AddVirtualCurrencyTypes(dataRequest, OnNewCurrencyAdded, OnAddNewCurrencyFailed);
        }

        private void OnNewCurrencyAdded(BlankResult result)
        {
            HideProgress();
            GetAllCurrencies();
        }

        private void OnAddNewCurrencyFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        // remove currency
        private void RemoveCurrency(VirtualCurrencyData virtualCurrency)
        {
            ShowProgress();

            var newCurrencyList = new List<VirtualCurrencyData>();
            newCurrencyList.Add(virtualCurrency);

            var dataRequest = new RemoveVirtualCurrencyTypesRequest
            {
                VirtualCurrencies = newCurrencyList
            };

            Icons.RemoveSprite(virtualCurrency.CurrencyCode);

            PlayFabAdminAPI.RemoveVirtualCurrencyTypes(dataRequest, OnCurrencyRemoved, OnRemoveCurrencyFailed);
        }

        private void OnCurrencyRemoved(BlankResult result)
        {
            HideProgress();
            GetAllCurrencies();
        }

        private void OnRemoveCurrencyFailed(PlayFabError error)
        {
            if (error.Error == PlayFabErrorCode.VirtualCurrencyCannotBeDeleted)
            {
                EditorUtility.DisplayDialog("VirtualCurrencyCannotBeDeleted", "Unable to delete currency. Currency is part of other objects. Remove all links first", "OK");
            }
            AddErrorLog(error);
            HideProgress();
        }

        // import default
        private void ImportDefaultCurrencies()
        {
            var newCurrenices = new List<VirtualCurrencyData>();
            // add gold
            newCurrenices.Add(new VirtualCurrencyData {
                CurrencyCode = "GD",
                DisplayName = "Gold",
                InitialDeposit = 1500
            });
            // add crystall
            newCurrenices.Add(new VirtualCurrencyData
            {
                CurrencyCode = "CR",
                DisplayName = "Crystall",
                InitialDeposit = 10
            });

            AddOrChangeCurrencies(newCurrenices);
        }
    }

    public enum CurrencyAction
    {
        ADD,
        EDIT
    }
}
#endif
