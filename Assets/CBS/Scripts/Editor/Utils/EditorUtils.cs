using CBS.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public static class EditorUtils
    {
        private static readonly string[] MonthArray = new string[]
        {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };

        public static GUIStyle TitleStyle
        {
            get
            {
                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 14;
                return titleStyle;
            }
        }

        public static void StartBackgroundTask(IEnumerator update, Action end = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        if (end != null)
                            end();
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    if (end != null)
                        end();
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public static bool DrawButton(string text, Color color, int fontSize, params GUILayoutOption[] options)
        {
            var oldColor = GUI.color;
            GUI.backgroundColor = color;
            var style = new GUIStyle(GUI.skin.button);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = fontSize;
            var result = GUILayout.Button(text, style, options);
            GUI.backgroundColor = oldColor;
            return result;
        }

        public static DateTime DrawDateTimeField(DateTime initDate)
        {
            var init = initDate;
            var selectedDay = init.Day;
            var selecteMonth = init.Month;
            var selecteYear = init.Year;
            var selecteHours = init.Hour;
            var selecteMinutes = init.Minute;

            GUILayout.BeginHorizontal();

            // draw days
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(50)});
            EditorGUILayout.LabelField("Day", new GUILayoutOption[] { GUILayout.MaxWidth(30) });
            var daysInMonth = DateTime.DaysInMonth(selecteYear, selecteMonth);
            var daysArray = GetArrayFromInt(daysInMonth);
            var daysString = daysArray.Select(i => i.ToString()).ToArray();
            selectedDay = EditorGUILayout.IntPopup(selectedDay, daysString, daysArray, new GUILayoutOption[] { GUILayout.MaxWidth(40) });
            GUILayout.EndVertical();

            // draw month
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(90)});
            EditorGUILayout.LabelField("Month", new GUILayoutOption[] { GUILayout.MaxWidth(50) });
            var monthCount = 12;
            var monthArray = GetArrayFromInt(monthCount);
            selecteMonth = EditorGUILayout.IntPopup(selecteMonth, MonthArray, monthArray, new GUILayoutOption[] { GUILayout.MaxWidth(80) });
            GUILayout.EndVertical();

            // draw years
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(100) });
            EditorGUILayout.LabelField("Year", new GUILayoutOption[] { GUILayout.MaxWidth(60)});
            var yearsArray = GetYearsArray(selecteYear, 20);
            var yearsString = yearsArray.Select(i => i.ToString()).ToArray();
            selecteYear = EditorGUILayout.IntPopup(selecteYear, yearsString, yearsArray, new GUILayoutOption[] { GUILayout.MaxWidth(60) });
            GUILayout.EndVertical();

            // draw hours
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(50) });
            EditorGUILayout.LabelField("Hours", new GUILayoutOption[] { GUILayout.MaxWidth(40) });
            var hoursInDay = 24;
            var hoursArray = GetArrayFromInt(hoursInDay);
            var hoursString = hoursArray.Select(i => i.ToString()).ToArray();
            selecteHours = EditorGUILayout.IntPopup(selecteHours, hoursString, hoursArray, new GUILayoutOption[] { GUILayout.MaxWidth(40) });
            GUILayout.EndVertical();

            // draw minutes
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.MaxWidth(50) });
            EditorGUILayout.LabelField("Min", new GUILayoutOption[] { GUILayout.MaxWidth(40) });
            var minutesInHours = 60;
            var minutesArray = GetArrayFromInt(minutesInHours, -1);
            var minutesString = minutesArray.Select(i => i.ToString()).ToArray();
            selecteMinutes = EditorGUILayout.IntPopup(selecteMinutes, minutesString, minutesArray, new GUILayoutOption[] { GUILayout.MaxWidth(40) });
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            return new DateTime(selecteYear, selecteMonth, selectedDay, selecteHours, selecteMinutes, 0);
        }

        private static int[] GetArrayFromInt(int intValue, int offset = 0)
        {
            int[] daysArray = new int[intValue];
            for (int i=0;i< intValue;i++)
            {
                daysArray[i] = i + 1 + offset;
            }
            return daysArray;
        }

        private static int[] GetYearsArray(int startYear, int forwardCount)
        {
            int[] yearsArray = new int[forwardCount];

            for (int i = 0; i < forwardCount; i++)
            {
                yearsArray[i] = startYear + i - forwardCount/2;
            }
            yearsArray = yearsArray.OrderBy(x=>x).ToArray();
            return yearsArray;
        }

        public static void DrawReward(PrizeObject reward, int size, ItemDirection direction)
        {
            if (direction == ItemDirection.HORIZONTAL)
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            else if (direction == ItemDirection.VERTICAL)
                GUILayout.BeginVertical(GUILayout.ExpandHeight(false));

            var items = reward.BundledItems ?? new List<string>();
            var lootboxes = reward.Lootboxes ?? new List<string>();
            var allItems = items.Concat(lootboxes).ToArray();

            for (int j = 0; j < allItems.Length; j++)
            {
                var itemID = allItems[j];
                if (!string.IsNullOrEmpty(itemID))
                {
                    // draw icon
                    var actvieSprite = CBSScriptable.Get<ItemsIcons>().GetSprite(itemID);
                    var iconTexture = actvieSprite == null ? null : actvieSprite.texture;
                    GUILayout.Button(iconTexture, GUILayout.Width(size), GUILayout.Height(size));
                }
            }

            // draw currencies
            var curList = reward.BundledVirtualCurrencies;
            if (curList != null)
            {
                foreach (var currency in curList)
                {
                    var curSprite = CBSScriptable.Get<CurrencyIcons>().GetSprite(currency.Key);
                    var curTexture = curSprite == null ? null : curSprite.texture;
                    GUILayout.Button(curTexture, GUILayout.Width(size), GUILayout.Height(size));
                    var textDimensions = GUI.skin.label.CalcSize(new GUIContent(currency.Value.ToString()));
                    //EditorGUILayout.LabelField(currency.Key, GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField(currency.Value.ToString(), GUILayout.Width(textDimensions.x));
                }
            }

            if (direction == ItemDirection.HORIZONTAL)
                GUILayout.EndHorizontal();
            else if (direction == ItemDirection.VERTICAL)
                GUILayout.EndVertical();
        }

        public static DevelopmentState DrawDevelopmentState(DevelopmentState currentState)
        {
            EditorGUILayout.LabelField("Dev State", TitleStyle, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.BeginHorizontal();
            var state = (DevelopmentState)EditorGUILayout.EnumPopup(currentState, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.Space(5);
            var color = state == DevelopmentState.IN_DEVELOP ? Color.red : Color.green;
            EditorUtils.DrawButton(string.Empty, color, 1, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(20) });
            GUILayout.EndHorizontal();
            return state;
        }
    }

    public enum ItemDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL
    }
}
