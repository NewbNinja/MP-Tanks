#if ENABLE_PLAYFABADMIN_API
using CBS.Core;
using CBS.Scriptable;
using PlayFab.AdminModels;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor.Window
{
    public class AddStatisticsWindow : EditorWindow
    {
        private static Action<PlayerStatisticDefinition> ModifyCallback { get; set; }
        private static PlayerStatisticDefinition Statistic { get; set; }
        private static ItemAction Action { get; set; }

        private bool IsInited { get; set; }
        private Vector2 ScrollView { get; set; }

        private string StatisticName { get; set; }
        private StatisticAggregationMethod AggregationMethod;
        private StatisticResetIntervalOption ResetOption;

        public static void Show(Action<PlayerStatisticDefinition> modifyCallback, PlayerStatisticDefinition statistic, ItemAction action)
        {
            ModifyCallback = modifyCallback;
            Statistic = statistic;
            Action = action;

            AddStatisticsWindow window = ScriptableObject.CreateInstance<AddStatisticsWindow>();
            window.maxSize = new Vector2(400, 700);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }

        private void Hide()
        {
            this.Close();
        }

        private void OnInit()
        {
            if (Statistic == null)
            {
                Statistic = new PlayerStatisticDefinition();
            }
            StatisticName = Statistic.StatisticName;
            AggregationMethod = Statistic.AggregationMethod ?? StatisticAggregationMethod.Last;
            ResetOption = Statistic.VersionChangeInterval ?? StatisticResetIntervalOption.Never;

            IsInited = true;
        }

        private void OnApply()
        {
            Statistic.StatisticName = StatisticName;
            Statistic.AggregationMethod = AggregationMethod;
            Statistic.VersionChangeInterval = ResetOption;
        }

        private bool IsValidInputs()
        {
            return IsNameValid();
        }

        void OnGUI()
        {
            if (!IsInited)
            {
                OnInit();
            }

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 12;

            using (var areaScope = new GUILayout.AreaScope(new Rect(0, 0, 400, 700)))
            {
                if (Statistic == null)
                    return;

                ScrollView = GUILayout.BeginScrollView(ScrollView);

                GUILayout.Space(10);
                GUILayout.BeginVertical();

                GUILayout.Label("Statistics Name", titleStyle, GUILayout.Width(120));
                if (Action == ItemAction.ADD)
                {
                    StatisticName = GUILayout.TextField(StatisticName);
                }
                else
                {
                    EditorGUILayout.LabelField(StatisticName);
                }
                if (Action == ItemAction.ADD)
                {
                    EditorGUILayout.HelpBox("The name for a specific statistic.", MessageType.Info);
                    if (!IsNameValid())
                    {
                        EditorGUILayout.HelpBox("Statistic name cannot be empty", MessageType.Error);
                    }
                }
                GUILayout.Space(15);

                GUILayout.Space(15);
                EditorGUILayout.LabelField("Aggregation Method", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                AggregationMethod = (StatisticAggregationMethod)EditorGUILayout.EnumPopup(AggregationMethod);
                switch (AggregationMethod)
                {
                    case StatisticAggregationMethod.Last:
                        EditorGUILayout.HelpBox("This is the old behavior, where the new value reported overwrites the old value. This is a frequently used aggregation method for statistics which aren’t intended as leaderboards, but rather as tracked values that are updated due to in-game actions. Things like character attributes and hit points would be good examples of this.", MessageType.Info);
                        break;
                    case StatisticAggregationMethod.Max:
                        EditorGUILayout.HelpBox("Update the statistic only if the new value is higher than the current value. The high score called out above is probably the most common use case for Max aggregation, so that a player’s score can only ever increase.", MessageType.Info);
                        break;
                    case StatisticAggregationMethod.Min:
                        EditorGUILayout.HelpBox("Update the statistic only if the new value is lower than the current value. Racing games, where the amount of time a player took to complete the race, commonly need this aggregation form, as players continually push to run their tracks faster and with greater precision.", MessageType.Info);
                        break;
                    case StatisticAggregationMethod.Sum:
                        EditorGUILayout.HelpBox("Add the reported value to the current value, as a running total. An example here would be experience points for a player, where each gameplay session provides some experience which is to be added to the total.", MessageType.Info);
                        break;
                }       
                GUILayout.Space(15);

                GUILayout.Space(15);
                EditorGUILayout.LabelField("Reset Interval", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                ResetOption = (StatisticResetIntervalOption)EditorGUILayout.EnumPopup(ResetOption);

                GUILayout.Space(30);

                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    if (IsValidInputs())
                    {
                        OnApply();
                        ModifyCallback?.Invoke(Statistic);
                        Hide();
                    }
                }
                if (GUILayout.Button("Close"))
                {
                    Hide();
                }
                GUILayout.EndHorizontal();

                GUILayout.EndScrollView();
            }
        }

        private bool IsNameValid()
        {
            return !string.IsNullOrEmpty(StatisticName);
        }
    }
}
#endif