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
    public class AddTournamentWindow : EditorWindow
    {
        private static Action<TournamentObject> AddCallback { get; set; }

        private string EntereID { get; set; }
        private string EnteredName { get; set; }

        public static void Show(Action<TournamentObject> modifyCallback)
        {
            AddCallback = modifyCallback;

            AddTournamentWindow window = ScriptableObject.CreateInstance<AddTournamentWindow>();
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
                EditorGUILayout.LabelField("Add new Tournament", titleStyle);

                GUILayout.Space(30);

                GUILayout.Label("Tournament ID", GUILayout.Width(120));
                EntereID = GUILayout.TextField(EntereID);

                GUILayout.Space(5);

                EditorGUILayout.HelpBox("id cannot contain special characters (/*-+_@&$#%)", MessageType.Info);

                GUILayout.Space(5);

                GUILayout.Label("Tournament Name", GUILayout.Width(120));
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
                    var newTournament = new TournamentObject
                    {
                        TounamentID = EntereID,
                        TournamentName = EnteredName,
                        NextTournamentID = EntereID,
                        DowngradeTournamentID = EntereID
                    };
                    AddCallback?.Invoke(newTournament);
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
