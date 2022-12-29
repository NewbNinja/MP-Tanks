using CBS.Core;
using CBS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor.Window
{
    public class AddBattlePassInstanceWindow : EditorWindow
    {
        private static Action<BattlePassInstance> AddCallback { get; set; }

        private string EntereID { get; set; }
        private string EnteredName { get; set; }

        public static void Show(Action<BattlePassInstance> modifyCallback)
        {
            AddCallback = modifyCallback;

            AddBattlePassInstanceWindow window = ScriptableObject.CreateInstance<AddBattlePassInstanceWindow>();
            window.maxSize = new Vector2(400, 700);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }

        private void Hide()
        {
            this.Close();
        }

        void OnGUI()
        {
            using (var areaScope = new GUILayout.AreaScope(new Rect(0, 0, 400, 700)))
            {
                GUILayout.Space(10);

                GUILayout.BeginVertical();

                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.alignment = TextAnchor.MiddleCenter;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 18;
                EditorGUILayout.LabelField("Add new BattlePass Instance", titleStyle);

                GUILayout.Space(30);

                GUILayout.Label("BattlePass ID", GUILayout.Width(120));
                EntereID = GUILayout.TextField(EntereID);

                GUILayout.Space(5);

                EditorGUILayout.HelpBox("id cannot contain special characters (/*-+_@&$#%)", MessageType.Info);

                GUILayout.Space(5);

                GUILayout.Label("Display Name", GUILayout.Width(120));
                EnteredName = GUILayout.TextField(EnteredName);

                GUILayout.EndVertical();

                GUILayout.Space(30);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Save"))
                {
                    if (string.IsNullOrEmpty(EntereID) || string.IsNullOrEmpty(EnteredName))
                        return;
                    if (TextUtils.ContainSpecialSymbols(EntereID))
                        return;
                    var newInstance = new BattlePassInstance
                    {
                        ID = EntereID,
                        DisplayName = EnteredName
                    };
                    AddCallback?.Invoke(newInstance);
                    Hide();
                }
                if (GUILayout.Button("Close"))
                {
                    Hide();
                }

                GUILayout.EndVertical();
            }
        }
    }
}
