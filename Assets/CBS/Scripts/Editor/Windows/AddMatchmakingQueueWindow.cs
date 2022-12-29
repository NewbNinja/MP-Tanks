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
    public class AddMatchmakingQueueWindow : EditorWindow
    {
        private static Action<MatchmakingQueueConfig> ModifyCallback { get; set; }
        private static MatchmakingQueueConfig Queue { get; set; }
        private static ItemAction Action { get; set; }

        private bool IsInited { get; set; }
        private Vector2 ScrollView { get; set; }

        private string QueueName { get; set; }
        private int PlayersCount { get; set; }
        private MatchmakingMode Mode { get; set; }

        private bool IsLevelEquality { get; set; }
        private bool IsStringEquality { get; set; }
        private bool IsLevelDifference { get; set; }
        private bool IsValueDifference { get; set; }

        private int LevelDifferenceValue { get; set; }
        private double DifferenceValue { get; set; }


        private Sprite IconSprite { get; set; }

        public static void Show(Action<MatchmakingQueueConfig> modifyCallback, MatchmakingQueueConfig queue, ItemAction action)
        {
            ModifyCallback = modifyCallback;
            Queue = queue;
            Action = action;

            AddMatchmakingQueueWindow window = ScriptableObject.CreateInstance<AddMatchmakingQueueWindow>();
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
            if (Queue == null)
            {
                Queue = new MatchmakingQueueConfig();
                Queue.MinMatchSize = 2;
                Queue.MaxMatchSize = 2;
            }
            QueueName = Queue.Name;
            PlayersCount = (int)Queue.MaxMatchSize;
            Mode = Queue.Teams == null || Queue.Teams.Count == 0 ? MatchmakingMode.Single : MatchmakingMode.Team;

            Queue.StringEqualityRules = Queue.StringEqualityRules ?? new List<StringEqualityRule>();
            Queue.DifferenceRules = Queue.DifferenceRules ?? new List<DifferenceRule>();

            IsLevelEquality = Queue.StringEqualityRules.Any(x => x.Name == CBSConstants.LevelEqualityRuleName);
            IsStringEquality = Queue.StringEqualityRules.Any(x => x.Name == CBSConstants.StringEqualityRuleName);
            IsLevelDifference = Queue.DifferenceRules.Any(x => x.Name == CBSConstants.LevelDifferenceRuleName);
            IsValueDifference = Queue.DifferenceRules.Any(x => x.Name == CBSConstants.ValueDifferenceRuleName);

            LevelDifferenceValue = IsLevelDifference ? (int)Queue.DifferenceRules.FirstOrDefault(x => x.Name == CBSConstants.LevelDifferenceRuleName).Difference : 1;
            DifferenceValue = IsValueDifference ? Queue.DifferenceRules.FirstOrDefault(x => x.Name == CBSConstants.ValueDifferenceRuleName).Difference : 1f;

            IsInited = true;
        }

        private void OnApply()
        {
            Queue.Name = QueueName;
            Queue.MinMatchSize = (uint)PlayersCount;
            Queue.MaxMatchSize = (uint)PlayersCount;

            if (Mode == MatchmakingMode.Single)
            {
                Queue.Teams = null;
            }
        }

        private bool IsValidInputs()
        {
            if (Mode == MatchmakingMode.Single)
            {
                return IsNameValid() && IsLevelRuleValid();
            }
            else if (Mode == MatchmakingMode.Team)
            {
                return IsNameValid() && IsTeamNameValid() && IsTeamValid() && IsTeamPlayersCountValid() && IsLevelRuleValid();
            }
            return false;
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
                if (Queue == null)
                    return;

                ScrollView = GUILayout.BeginScrollView(ScrollView);

                GUILayout.Space(10);
                GUILayout.BeginVertical();

                GUILayout.Label("Queue Name", titleStyle, GUILayout.Width(80));
                if (Action == ItemAction.ADD)
                {
                    QueueName = GUILayout.TextField(QueueName);
                }
                else
                {
                    EditorGUILayout.LabelField(QueueName);
                }
                if (Action == ItemAction.ADD)
                {
                    EditorGUILayout.HelpBox("The name for a specific queue. It is between 1 and 64 characters long (inclusive) and is case-sensitive. It is alpha-numeric, plus underscores and hyphens, and starts with a letter or number. Generally a queue name represents a way to play a game, such as 4v4CaptureTheFlag or UnrankedRace. When creating a matchmaking ticket, the queue name must be specified to identify which queue it should enter.", MessageType.Info);
                    if (!IsNameValid())
                    {
                        EditorGUILayout.HelpBox("Queue name cannot be empty", MessageType.Error);
                    }
                }

                GUILayout.Space(15);
                EditorGUILayout.LabelField("Players Count (Total)", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                PlayersCount = EditorGUILayout.IntField(PlayersCount);
                if (Mode == MatchmakingMode.Single)
                {
                    PlayersCount = Mathf.Clamp(PlayersCount, 2, 100);
                    EditorGUILayout.HelpBox("Players count for this queue. Min value is 2. Max for single mode is 100", MessageType.Info);
                }
                else
                {
                    PlayersCount = Mathf.Clamp(PlayersCount, 2, 32);
                    EditorGUILayout.HelpBox("Players count for this queue. Min value is 2. Max for team mode is 32", MessageType.Info);
                }
                
                GUILayout.Space(15);

                GUILayout.Space(15);
                EditorGUILayout.LabelField("Queue Mode", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                Mode = (MatchmakingMode)EditorGUILayout.EnumPopup(Mode);
                EditorGUILayout.HelpBox("Determines the mode for the queue. Team or not", MessageType.Info);
                GUILayout.Space(15);

                // draw team menu 
                if (Mode == MatchmakingMode.Team)
                {
                    GUILayout.Space(15);
                    EditorGUILayout.LabelField("Team configurations", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                    if (!IsTeamValid())
                    {
                        EditorGUILayout.HelpBox("The minimum number of teams must be 2", MessageType.Error);
                    }
                    else if (!IsTeamPlayersCountValid())
                    {
                        EditorGUILayout.HelpBox("The sum of the players count of all teams must match the number of total players count", MessageType.Error);
                    }
                    else if (!IsTeamNameValid())
                    {
                        EditorGUILayout.HelpBox("Team name cannot be empty", MessageType.Error);
                    }

                    // draw each teams
                    var queueList = Queue.Teams;
                    if (queueList != null)
                    {
                        GUILayout.Space(5);
                        for (int i=0;i< queueList.Count;i++)
                        {
                            EditorUtils.DrawUILine(Color.grey, 2, 20);

                            float lastY = GUILayoutUtility.GetLastRect().y;
                            float lastX = GUILayoutUtility.GetLastRect().x;

                            var boxHeight = 187;
                            var boxColor = ColorUtils.GetTeamColor(i);
                            boxColor.a = 0.1f;
                            var boxTexture = MakeTex(600, boxHeight, boxColor);
                            GUI.Box(new Rect(new Vector2(lastX - 20, lastY + 10), new Vector2(600, boxHeight)), boxTexture);

                            var team = queueList[i];
                            EditorGUILayout.LabelField("Team Name", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                            team.Name = GUILayout.TextField(team.Name);
                            EditorGUILayout.HelpBox("For example blue team or red team", MessageType.Info);

                            var teamPlayerCount = int.Parse(team.MaxTeamSize.ToString());
                            EditorGUILayout.LabelField("Players Count", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                            teamPlayerCount = EditorGUILayout.IntField(teamPlayerCount);
                            if (teamPlayerCount < 1)
                                teamPlayerCount = 1;
                            team.MaxTeamSize = (uint)teamPlayerCount;
                            team.MinTeamSize = (uint)teamPlayerCount;
                            EditorGUILayout.HelpBox("Players count for this team. Min value is 1", MessageType.Info);
                            EditorUtils.DrawUILine(Color.grey, 2, 20);
                            if (GUILayout.Button("Remove team"))
                            {
                                queueList.Remove(team);
                                queueList.TrimExcess();
                            }
                            GUILayout.Space(5);
                        }
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("Add new team"))
                    {
                        Queue.Teams = Queue.Teams ?? new List<MatchmakingQueueTeam>();
                        Queue.Teams.Add(new MatchmakingQueueTeam());
                    }
                }

                // draw rules menu
                GUILayout.Space(15);
                EditorGUILayout.LabelField("Rules configurations", titleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
                if (!IsLevelRuleValid())
                {
                    EditorGUILayout.HelpBox("Level equality rule and Level Difference rule can not be used at the same time", MessageType.Error);
                }

                // draw level equality
                var levelEquality = EditorGUILayout.Toggle("Level Equality Rule", IsLevelEquality);
                EditorGUILayout.HelpBox("Make sure players only find each other with the same levels", MessageType.Info);
                if (levelEquality != IsLevelEquality)
                {
                    if (levelEquality)
                    {
                        // add rule
                        Queue.StringEqualityRules.Add(new StringEqualityRule {
                            Name = CBSConstants.LevelEqualityRuleName,
                            Attribute = new QueueRuleAttribute { 
                                Source = AttributeSource.User,
                                Path = CBSConstants.MatchmakingLevelEqualityAttribute,
                            },
                            Weight = 1,
                            AttributeNotSpecifiedBehavior = AttributeNotSpecifiedBehavior.MatchAny
                        });
                    }
                    else
                    {
                        // remove rule
                        var levelRule = Queue.StringEqualityRules.FirstOrDefault(x => x.Name == CBSConstants.LevelEqualityRuleName);
                        if (levelRule != null)
                        {
                            Queue.StringEqualityRules.Remove(levelRule);
                            Queue.StringEqualityRules.TrimExcess();
                        }
                    }
                }
                IsLevelEquality = levelEquality;

                // draw string equality
                GUILayout.Space(5);
                var stringEquality = EditorGUILayout.Toggle("String Equality Rule", IsStringEquality);
                EditorGUILayout.HelpBox("Make sure players only find each other with the same string value. You can set string value in ticket request. Use this, for example, when you want the participants of the same tournament to find each other, specify the tournament ID as a value.", MessageType.Info);
                if (stringEquality != IsStringEquality)
                {
                    if (stringEquality)
                    {
                        // add rule
                        Queue.StringEqualityRules.Add(new StringEqualityRule
                        {
                            Name = CBSConstants.StringEqualityRuleName,
                            Attribute = new QueueRuleAttribute
                            {
                                Source = AttributeSource.User,
                                Path = CBSConstants.MatchmakingStringEqualityAttribute,
                            },
                            Weight = 1,
                            AttributeNotSpecifiedBehavior = AttributeNotSpecifiedBehavior.MatchAny
                        });
                    }
                    else
                    {
                        // remove rule
                        var stringRule = Queue.StringEqualityRules.FirstOrDefault(x => x.Name == CBSConstants.StringEqualityRuleName);
                        if (stringRule != null)
                        {
                            Queue.StringEqualityRules.Remove(stringRule);
                            Queue.StringEqualityRules.TrimExcess();
                        }
                    }
                }
                IsStringEquality = stringEquality;

                // draw level difference
                GUILayout.Space(5);
                var levelDifference = EditorGUILayout.Toggle("Level Difference Rule", IsLevelDifference);
                if (IsLevelDifference)
                {
                    LevelDifferenceValue = EditorGUILayout.IntField("Difference +-", LevelDifferenceValue);
                }
                EditorGUILayout.HelpBox("Players will be able to find an opponent within the range of the level. For example, if you specify the difference + -1 - then a player of level 7 will be able to find an opponent of levels 6,7,8", MessageType.Info);
                if (levelDifference != IsLevelDifference)
                {
                    if (levelDifference)
                    {
                        // add rule
                        Queue.DifferenceRules.Add(new DifferenceRule
                        {
                            Name = CBSConstants.LevelDifferenceRuleName,
                            Attribute = new QueueRuleAttribute
                            {
                                Source = AttributeSource.User,
                                Path = CBSConstants.MatchmakingLevelDifferenceAttribute,
                            },
                            Weight = 1,
                            AttributeNotSpecifiedBehavior = AttributeNotSpecifiedBehavior.MatchAny,
                            MergeFunction = AttributeMergeFunction.Average
                        });
                    }
                    else
                    {
                        // remove rule
                        var levelRule = Queue.DifferenceRules.FirstOrDefault(x => x.Name == CBSConstants.LevelDifferenceRuleName);
                        if (levelRule != null)
                        {
                            Queue.DifferenceRules.Remove(levelRule);
                            Queue.DifferenceRules.TrimExcess();
                        }
                    }
                }
                IsLevelDifference = levelDifference;

                if (levelDifference)
                {
                    var difLevelRule = Queue.DifferenceRules.FirstOrDefault(x => x.Name == CBSConstants.LevelDifferenceRuleName);
                    if (difLevelRule != null)
                    {
                        difLevelRule.Difference = LevelDifferenceValue;
                    }
                }

                // draw level difference
                GUILayout.Space(5);
                var isValueDifference = EditorGUILayout.Toggle("Value Difference Rule", IsValueDifference);
                if (IsValueDifference)
                {
                    DifferenceValue = EditorGUILayout.DoubleField("Difference +-", DifferenceValue);
                }
                EditorGUILayout.HelpBox("Players will be able to find an opponent within the range of the custom value. For example, if you specify the difference + -15 - then a player has value 40 will be able to find an opponent of value from range 25-55. You can set value in ticket request.", MessageType.Info);
                if (isValueDifference != IsValueDifference)
                {
                    if (isValueDifference)
                    {
                        // add rule
                        Queue.DifferenceRules.Add(new DifferenceRule
                        {
                            Name = CBSConstants.ValueDifferenceRuleName,
                            Attribute = new QueueRuleAttribute
                            {
                                Source = AttributeSource.User,
                                Path = CBSConstants.MatchmakingValueDifferenceAttribute,
                            },
                            Weight = 1,
                            AttributeNotSpecifiedBehavior = AttributeNotSpecifiedBehavior.MatchAny,
                            MergeFunction = AttributeMergeFunction.Average
                        });
                    }
                    else
                    {
                        // remove rule
                        var levelRule = Queue.DifferenceRules.FirstOrDefault(x => x.Name == CBSConstants.ValueDifferenceRuleName);
                        if (levelRule != null)
                        {
                            Queue.DifferenceRules.Remove(levelRule);
                            Queue.DifferenceRules.TrimExcess();
                        }
                    }
                }
                IsValueDifference = isValueDifference;

                if (isValueDifference)
                {
                    var difLevelRule = Queue.DifferenceRules.FirstOrDefault(x => x.Name == CBSConstants.ValueDifferenceRuleName);
                    if (difLevelRule != null)
                    {
                        difLevelRule.Difference = DifferenceValue;
                    }
                }

                GUILayout.Space(30);

                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    if (IsValidInputs())
                    {
                        OnApply();
                        ModifyCallback?.Invoke(Queue);
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

        private bool IsTeamValid()
        {
            return Queue.Teams != null && Queue.Teams.Count >= 2;
        }

        private bool IsTeamPlayersCountValid()
        {
            if (Queue == null || Queue.Teams == null)
                return true;
            var allTeamPlayers = Queue.Teams.Sum(x => x.MaxTeamSize);
            return allTeamPlayers == PlayersCount;
        }

        private bool IsNameValid()
        {
            return !string.IsNullOrEmpty(QueueName);
        }

        private bool IsTeamNameValid()
        {
            if (Queue.Teams == null)
                return false;
            return !Queue.Teams.Any(x => string.IsNullOrEmpty(x.Name));
        }

        private bool IsLevelRuleValid()
        {
            return !(IsLevelEquality && IsLevelDifference);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
#endif