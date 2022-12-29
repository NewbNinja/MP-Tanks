#if ENABLE_PLAYFABADMIN_API
using CBS.Scriptable;
using PlayFab.AdminModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class AddCurrencyWindow : EditorWindow
    {
        private static Action<VirtualCurrencyData> AddCallback { get; set; }
        private static VirtualCurrencyData CurrentData { get; set; }
        private static CurrencyAction Action { get; set; }

        private string CurrencyCode { get; set; }
        private string DisplayName { get; set; }
        private int InitialDeposit;
        private int RechargeRate;
        private int RechargeMaximum;

        private Sprite IconSprite { get; set; }

        private bool IsInited { get; set; } = false;

        private CurrencyIcons Icons { get; set; }

        public static void Show(VirtualCurrencyData current, Action<VirtualCurrencyData> addCallback, CurrencyAction action)
        {
            AddCallback = addCallback;
            CurrentData = current;
            Action = action;

            AddCurrencyWindow window = ScriptableObject.CreateInstance<AddCurrencyWindow>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2 + 200, 400, 700);
            window.ShowUtility();
        }

        private void Hide()
        {
            this.Close();
        }

        private void Init()
        {
            Icons = CBSScriptable.Get<CurrencyIcons>();

            CurrencyCode = CurrentData.CurrencyCode;
            DisplayName = CurrentData.DisplayName;
            InitialDeposit = CurrentData.InitialDeposit ?? 0;
            RechargeRate = CurrentData.RechargeRate ?? 0;
            RechargeMaximum = CurrentData.RechargeMax ?? 0;

            IconSprite = Icons.GetSprite(CurrencyCode);

            IsInited = true;
        }

        private void CheckInputs()
        {
            CurrentData.CurrencyCode = CurrencyCode;
            CurrentData.DisplayName = DisplayName;
            CurrentData.InitialDeposit = InitialDeposit == 0 ? default : InitialDeposit;
            CurrentData.RechargeRate = RechargeRate == 0 ? default : RechargeRate;
            CurrentData.RechargeMax = RechargeMaximum == 0 ? default : RechargeMaximum;
        }

        void OnGUI()
        {
            // init start values
            if (!IsInited)
            {
                Init();
            }
            GUILayout.Space(15);
            if (Action == CurrencyAction.ADD)
            {
                CurrencyCode = EditorGUILayout.TextField("Currency code", CurrencyCode);
            }
            if (Action == CurrencyAction.EDIT)
            {
                EditorGUILayout.LabelField("Currency code", CurrencyCode);
            }
            EditorGUILayout.HelpBox("Unique code for game currency. Consists of two uppercase characters. For example for gold - GD", MessageType.Info);

            // check for max length
            if (!string.IsNullOrEmpty(CurrencyCode) && CurrencyCode.Length > 2)
            {
                CurrencyCode = CurrencyCode.Substring(0, 2).ToUpper();
            }
            // check for upper case
            if (!string.IsNullOrEmpty(CurrencyCode) && CurrencyCode.Length > 0)
            {
                CurrencyCode = CurrencyCode.ToUpper();
            }
            GUILayout.Space(15);
            DisplayName = EditorGUILayout.TextField("Display name", DisplayName);
            EditorGUILayout.HelpBox("Full name of the game currency", MessageType.Info);
            GUILayout.Space(15);
            InitialDeposit = EditorGUILayout.IntField("Initial deposit", InitialDeposit);
            EditorGUILayout.HelpBox("The value of the game currency, which will be automatically credited to the player after registration", MessageType.Info);
            GUILayout.Space(15);
            GUILayout.Label("Recharge setting (Optional)");
            GUILayout.Space(15);
            RechargeRate = EditorGUILayout.IntField("Recharge rate per day", RechargeRate);
            EditorGUILayout.HelpBox("The amount of game currency that will be credited to the player every day", MessageType.Info);
            GUILayout.Space(15);
            RechargeMaximum = EditorGUILayout.IntField("Recharge maximum", RechargeMaximum);
            EditorGUILayout.HelpBox("The maximum value of the game currency at which the daily accrual will be stopped", MessageType.Info);
            GUILayout.Space(15);
            GUILayout.BeginVertical();
            // draw icon
            
            EditorGUILayout.LabelField("Sprite", new GUILayoutOption[] { GUILayout.MaxWidth(150) });
            IconSprite = (Sprite)EditorGUILayout.ObjectField((IconSprite as UnityEngine.Object), typeof(Sprite), false, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
            EditorGUILayout.HelpBox("Sprite for game currency. ATTENTION! The sprite is not saved on the server, it will be included in the build", MessageType.Info);
            GUILayout.EndVertical();

            GUILayout.Space(70);
            string buttonTitle = Action == CurrencyAction.ADD ? "Add" : "Save";
            if (GUILayout.Button(buttonTitle))
            {
                if (IconSprite == null)
                {
                    Icons.RemoveSprite(CurrencyCode);
                }
                else
                {
                    Icons.SaveSprite(CurrencyCode, IconSprite);
                }
                CheckInputs();
                AddCallback?.Invoke(CurrentData);
                Hide();
            }
        }
    }
}
#endif
