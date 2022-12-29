using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class ConfiguratorWindow : EditorWindow
    {
        private Rect SideMenuRect = new Rect(2, 15, 160, 700);
        public static Rect ContentRect = new Rect(180, 15, 1085, 700);

        private GUILayoutOption[] SideButtonOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(46f), GUILayout.Width(146.88f) };
            }
        }

        private List<MenuTitles> SideMenuItems
        {
            get
            {
                return Enum.GetValues(typeof(MenuTitles)).Cast<MenuTitles>().ToList();
            }
        }

        private Vector2 ScrollPos { get; set; }

        private MenuTitles ActiveMenu { get; set; }

        private BaseConfigurator CurrentConfigurator { get; set; }

        private void Awake()
        {
            // load default configurator
            OnMenuSelected(MenuTitles.Auth);
        }

        private void OnGUI()
        {
            // draw background
            var tex = ResourcesUtils.GetBackgroundImage();
            GUI.DrawTexture(new Rect(0, 0, 1280, 720), tex);
            // draw menu
            DrawMenuToolBar();
            // draw content
            CurrentConfigurator?.Draw(ContentRect);
            
        }

        private void DrawMenuToolBar()
        {
            using (var areaScope = new GUILayout.AreaScope(SideMenuRect))
            {
                GUIStyle verticalStyle = new GUIStyle("verticalScrollbar");
                GUIStyle horisontalStyle = new GUIStyle("horizontalScrollbar");
                verticalStyle.fixedWidth = 10;

                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, horisontalStyle, verticalStyle);

                var levelTitleStyle = new GUIStyle(GUI.skin.label);
                levelTitleStyle.fontStyle = FontStyle.Bold;
                levelTitleStyle.fontSize = 14;

                EditorGUILayout.LabelField("Modules", levelTitleStyle, new GUILayoutOption[] { GUILayout.MaxWidth(150) });

                GUIStyle style = new GUIStyle("Label");

                for (int i = 0; i < SideMenuItems.Count; i++)
                {
                    var title = SideMenuItems[i];

                    if (title == MenuTitles.Azure)
                    {
                        GUILayout.Space(50);
                        EditorGUILayout.LabelField("Settings", levelTitleStyle);
                    }

                    string buttonTitle = title.ToString();
                    bool active = title == ActiveMenu;
                    var tex = active ? ResourcesUtils.GetMenuTexture(title, ButtonState.Active) : ResourcesUtils.GetMenuTexture(title, ButtonState.Default);

                    if (GUILayout.Button(tex, style, SideButtonOptions))
                    {
                        OnMenuSelected(SideMenuItems[i]);
                    }

                    GUILayout.Space(-8);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void OnMenuSelected(MenuTitles title)
        {
            ActiveMenu = title;
#if ENABLE_PLAYFABADMIN_API
            switch (title)
            {
                case MenuTitles.Auth:
                    CurrentConfigurator = new AuthConfigurator();
                    break;
                case MenuTitles.Profile:
                    CurrentConfigurator = new ProfileConfigurator();
                    break;
                case MenuTitles.Currency:
                    CurrentConfigurator = new CurrencyConfigurator();
                    break;
                case MenuTitles.Items:
                    CurrentConfigurator = new ItemsConfigurator();
                    break;
                case MenuTitles.Azure:
                    CurrentConfigurator = new AzureConfigurator();
                    break;
                case MenuTitles.Chat:
                    CurrentConfigurator = new ChatConfigurator();
                    break;
                case MenuTitles.Clans:
                    CurrentConfigurator = new ClanConfigurator();
                    break;
                case MenuTitles.Tournaments:
                    CurrentConfigurator = new LeagueConfigurator();
                    break;
                case MenuTitles.DailyBonus:
                    CurrentConfigurator = new DailyBonusConfigurator();
                    break;
                case MenuTitles.Roulette:
                    CurrentConfigurator = new RouletteConfigurator();
                    break;
                case MenuTitles.PlayFab:
                    CurrentConfigurator = new PlayfabConfigurator();
                    break;
                case MenuTitles.Examples:
                    CurrentConfigurator = new ExampleConfigurator();
                    break;
                case MenuTitles.Matchmaking:
                    CurrentConfigurator = new MatchmakingConfigurator();
                    break;
                case MenuTitles.Achievements:
                    CurrentConfigurator = new AchievementsConfigurator();
                    break;
                case MenuTitles.DailyTasks:
                    CurrentConfigurator = new DailyTasksConfigurator();
                    break;
                case MenuTitles.Leaderboards:
                    CurrentConfigurator = new LeaderboardsConfigurator();
                    break;
                case MenuTitles.BattlePass:
                    CurrentConfigurator = new BattlePassConfigurator();
                    break;
                default:
                    Debug.Log("NOTHING");
                    CurrentConfigurator = null;
                    break;
            }
#endif
            CurrentConfigurator?.Init(title);
        }

        public void OnInspectorUpdate()
        {
            // This will only get called 10 times per second.
            Repaint();
        }
    }

    public enum MenuTitles
    {
        Auth,
        Profile,
        Currency,
        Items,
        Chat,
        Clans,
        Tournaments,
        DailyBonus,
        Roulette,
        Matchmaking,
        Achievements,
        DailyTasks,
        Leaderboards,
        BattlePass,
        Azure,
        PlayFab,
        Examples
    }
}
