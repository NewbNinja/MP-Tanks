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
using CBS.Editor.Window;
using CBS.Utils;
using CBS.Scriptable;

namespace CBS.Editor
{
    public class LeagueConfigurator : BaseConfigurator
    {
        protected override string Title => "League Congiguration";

        protected override bool DrawScrollView => true;

        private Rect CategoriesRect = new Rect(0, 0, 150, 700);
        private Rect ItemsRect = new Rect(200, 100, 835, 700);
        private Vector2 PositionScroll { get; set; }

        private readonly int DefaultPositionCount = 10;

        private TournamentData TournamentsData { get; set; } = new TournamentData();

        private TournamentObject SelectedTournament { get; set; }

        private int TournamentIndex { get; set; }

        private int SelectedTimeOptions { get; set; }

        private int DurationInhours { get; set; }

        private List<CatalogItem> CachedItems { get; set; }
        private Categories CachedItemCategories { get; set; }
        private List<string> CacheCurrencies { get; set; }
        private Categories CachedLootBoxCategories { get; set; }

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            GetTournaments();
        }

        protected override void OnDrawInside()
        {
            DrawLeagues();
            DrawTournament();
        }

        private void DrawLeagues()
        {
            using (var areaScope = new GUILayout.AreaScope(CategoriesRect))
            {
                GUILayout.BeginVertical();

                int categoryHeight = 30;
                int categoriesCount = TournamentsData.Tournaments == null ? 0 : TournamentsData.Tournaments.Count;

                if (TournamentsData.Tournaments != null && TournamentsData.Tournaments.Count > 0)
                {
                    var categoriesMenu = TournamentsData.Tournaments.Select(x => x.Value.TournamentName).ToArray();
                    TournamentIndex = GUI.SelectionGrid(new Rect(0, 100, 150, categoryHeight * categoriesCount), TournamentIndex, categoriesMenu.ToArray(), 1);
                    string selctedCategory = categoriesMenu[TournamentIndex];

                    SelectedTournament = TournamentsData.Tournaments.ElementAt(TournamentIndex).Value;
                }

                GUILayout.Space(30);
                GUILayout.Space(30);
                var oldColor = GUI.color;
                GUI.backgroundColor = EditorData.AddColor;
                var style = new GUIStyle(GUI.skin.button);
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 12;
                if (GUI.Button(new Rect(0, 130 + categoryHeight * categoriesCount, 150, categoryHeight), "Add new Tournament", style))
                {
                    AddTournamentWindow.Show(onAdd =>
                    {
                        var newTournament = onAdd;
                        AddDefaultPosition(newTournament);
                        AddDefaultState(newTournament);
                        TournamentsData.Tournaments = TournamentsData.Tournaments ?? new Dictionary<string, TournamentObject>();
                        if (TournamentsData.Tournaments.Count == 0)
                        {
                            TournamentsData.Date = TournamentDate.Weekly;
                        }
                        TournamentsData.Tournaments.Add(newTournament.TounamentID, newTournament);
                        SaveTournaments(TournamentsData);
                    });
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = oldColor;

                GUILayout.EndVertical();
            }
        }

        private void DrawTournament()
        {
            if (SelectedTournament == null)
                return;
            using (var areaScope = new GUILayout.AreaScope(ItemsRect))
            {
                PositionScroll = GUILayout.BeginScrollView(PositionScroll);

                var titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 14;

                EditorGUILayout.LabelField("Tournament ID", titleStyle);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(SelectedTournament.TounamentID);

                GUILayout.FlexibleSpace();

                if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(30) }))
                {
                    int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove this tournament?",
                            "Yes",
                            "No",
                            string.Empty);
                    switch (option)
                    {
                        // ok.
                        case 0:
                            RemoveTournament(SelectedTournament);
                            SelectedTournament = null;
                            SaveTournaments(TournamentsData);
                            break;
                    }
                    if (SelectedTournament == null)
                    {
                        TournamentIndex = 0;
                        return;
                    }
                }

                if (EditorUtils.DrawButton("Save", EditorData.SaveColor, 12, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(30) }))
                {
                    SaveTournaments(TournamentsData);
                }

                GUILayout.EndHorizontal();

                // draw name
                EditorGUILayout.LabelField("Tournament Name", titleStyle);
                SelectedTournament.TournamentName = EditorGUILayout.TextField(SelectedTournament.TournamentName, new GUILayoutOption[] { GUILayout.Width(400) });

                GUILayout.Space(10);

                // draw Default state
                EditorGUILayout.LabelField("Is Default Tournament ?", titleStyle);
                bool state = SelectedTournament.IsDefault;
                SelectedTournament.IsDefault = EditorGUILayout.Toggle(SelectedTournament.IsDefault);
                if (state != SelectedTournament.IsDefault)
                {
                    CheckDefaultState(SelectedTournament, SelectedTournament.IsDefault);
                }
                EditorGUILayout.HelpBox("New players will start the default tournament. At least one tournament must be in default state.", MessageType.Info);

                GUILayout.Space(10);

                // draw time
                EditorGUILayout.LabelField("Duration", titleStyle);
                var allTimeOptions = Enum.GetNames(typeof(TournamentDate));
                SelectedTimeOptions = EditorGUILayout.Popup(SelectedTimeOptions, allTimeOptions.ToArray());
                EditorGUILayout.HelpBox("Determines how long the tournament will run. This option is common for all tournaments.", MessageType.Info);
                if ((TournamentDate)SelectedTimeOptions == TournamentDate.Custom)
                {
                    DurationInhours = EditorGUILayout.IntField("Duration in hours", DurationInhours);
                }
                TournamentsData.Date = (TournamentDate)SelectedTimeOptions;
                GUILayout.Space(10);

                var allTournamentNames = TournamentsData.Tournaments.Select(x => x.Value.TournamentName).ToArray();

                // draw upgrade league
                EditorGUILayout.LabelField("Next Tournament", titleStyle);
                string nextKey = string.IsNullOrEmpty(SelectedTournament.NextTournamentID) ? SelectedTournament.TounamentID : SelectedTournament.NextTournamentID;
                int selectedNext = TournamentsData.Tournaments.Keys.ToList().IndexOf(nextKey);
                selectedNext = EditorGUILayout.Popup(selectedNext, allTournamentNames.ToArray());
                SelectedTournament.NextTournamentID = TournamentsData.Tournaments.ElementAt(selectedNext).Key;
                EditorGUILayout.HelpBox("Determine which next tournament the player will get to if he takes a high place in the rating.", MessageType.Info);

                GUILayout.Space(10);

                // draw downgrade league
                EditorGUILayout.LabelField("Downgrade Tournament", titleStyle);
                string prevKey = string.IsNullOrEmpty(SelectedTournament.DowngradeTournamentID) ? SelectedTournament.TounamentID : SelectedTournament.DowngradeTournamentID;
                int selectedPrev = TournamentsData.Tournaments.Keys.ToList().IndexOf(prevKey);
                selectedPrev = EditorGUILayout.Popup(selectedPrev, allTournamentNames.ToArray());
                SelectedTournament.DowngradeTournamentID = TournamentsData.Tournaments.ElementAt(selectedPrev).Key;
                EditorGUILayout.HelpBox("Determine which next tournament the player will get to if he takes a low place in the rating.", MessageType.Info);

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Positions", titleStyle, GUILayout.Width(100));
                GUILayout.Space(50);
                EditorGUILayout.LabelField("Next Tournament Place?", titleStyle, GUILayout.Width(150));
                GUILayout.Space(50);
                EditorGUILayout.LabelField("Downgrade Place?", titleStyle, GUILayout.Width(120));
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                // draw places
                var places = SelectedTournament.Positions;
                int positionCount = places.Count;
                for (int i = 0; i < positionCount; i++)
                {
                    if (i >= places.Count)
                        return;
                    var currentPosition = places[i];
                    GUILayout.BeginHorizontal();
                    string positionNumber = (i + 1).ToString() + ". Place";
                    EditorGUILayout.LabelField(positionNumber, titleStyle);

                    var next = currentPosition.NextTournament;
                    var down = currentPosition.DowngradeTournament;

                    currentPosition.NextTournament = EditorGUILayout.Toggle(currentPosition.NextTournament);

                    if (next != currentPosition.NextTournament)
                    {
                        if (currentPosition.NextTournament == true)
                        {
                            currentPosition.DowngradeTournament = false;
                        }
                    }

                    currentPosition.DowngradeTournament = EditorGUILayout.Toggle(currentPosition.DowngradeTournament);

                    if (down != currentPosition.DowngradeTournament)
                    {
                        if (currentPosition.DowngradeTournament == true)
                        {
                            currentPosition.NextTournament = false;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (EditorUtils.DrawButton("Remove", EditorData.RemoveColor, 12))
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning",
                            "Are you sure you want to remove this position?",
                            "Yes",
                            "No",
                            string.Empty);
                        switch (option)
                        {
                            // ok.
                            case 0:
                                places.RemoveAt(i);
                                places.TrimExcess();
                                break;
                        }
                    }

                    if (EditorUtils.DrawButton("Edit Prize", EditorData.AddPrizeColor, 12))
                    {
                        ShowPrizeDialog(currentPosition.Prizes, true, result =>
                        {
                            currentPosition.Prizes = result;
                            SaveTournaments(TournamentsData);
                        });
                    }

                    GUILayout.EndHorizontal();

                    EditorUtils.DrawUILine(Color.grey, 1, 20);

                    GUILayout.Space(10);
                }


                GUILayout.Space(20);
                if (EditorUtils.DrawButton("Add new position", EditorData.AddColor, 12))
                {
                    if (places.Count > 100)
                        return;
                    places.Add(new TournamentPosition());
                }

                GUILayout.Space(110);

                GUILayout.EndScrollView();
            }
        }

        private void GetTournaments()
        {
            ShowProgress();
            var keys = new List<string>();
            keys.Add(CBSConstants.TournamentsDataKey);

            var request = new GetTitleDataRequest
            {
                Keys = keys
            };
            PlayFabAdminAPI.GetTitleInternalData(request, OnInternalDataGetted, OnGetDataFailed);
        }

        private void OnInternalDataGetted(GetTitleDataResult result)
        {
            HideProgress();
            var dictionary = result.Data;
            bool keyExist = dictionary.ContainsKey(CBSConstants.TournamentsDataKey);
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            TournamentsData = keyExist ? jsonPlugin.DeserializeObject<TournamentData>(dictionary[CBSConstants.TournamentsDataKey]) : new TournamentData();
            // parse data
            var currentDate = TournamentsData.Date;
            SelectedTimeOptions = (int)currentDate;
            if (currentDate == TournamentDate.Custom)
            {
                DurationInhours = DateUtils.TimestampToHours(TournamentsData.DateTimestamp);
            }
            else
            {
                DurationInhours = DateUtils.TournamentDateToHours(TournamentsData.Date);
            }
        }

        private void OnGetDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        private void SaveTournaments(TournamentData tournamentData)
        {
            ShowProgress();

            // init time
            var dateType = tournamentData.Date;
            if (dateType == TournamentDate.Custom)
            {
                tournamentData.DateTimestamp = DateUtils.HoursToMiliseconds(DurationInhours);
            }
            else
            {
                tournamentData.DateTimestamp = DateUtils.ToutnamentDateMiliseconds(dateType);
            }

            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            string rawData = jsonPlugin.SerializeObject(tournamentData);

            var request = new SetTitleDataRequest
            {
                Key = CBSConstants.TournamentsDataKey,
                Value = rawData
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSaveTournament, OnSaveDataFailed);
        }

        private void OnSaveTournament(SetTitleDataResult result)
        {
            HideProgress();
            GetTournaments();
        }

        private void OnSaveDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        private void AddDefaultPosition(TournamentObject tournament)
        {
            tournament.Positions = new List<TournamentPosition>();
            for (int i = 0; i < DefaultPositionCount; i++)
            {
                tournament.Positions.Add(new TournamentPosition());
            }
        }

        private void AddDefaultState(TournamentObject tournament)
        {
            bool firstTournament = TournamentsData.Tournaments == null || TournamentsData.Tournaments.Count == 0;
            tournament.IsDefault = firstTournament;
        }

        private void CheckDefaultState(TournamentObject tournament, bool active)
        {
            var otherTournaments = TournamentsData.Tournaments.Where(x => x.Value != tournament);
            if (active)
            {
                if (otherTournaments.Count() > 0)
                {
                    foreach (var t in otherTournaments)
                        t.Value.IsDefault = false;
                }
            }
            else
            {
                if (otherTournaments.Count() > 0)
                {
                    var defaultTournament = otherTournaments.FirstOrDefault().Value;
                    defaultTournament.IsDefault = true;
                }
                else
                {
                    tournament.IsDefault = true;
                }
            }
        }

        private void RemoveTournament(TournamentObject tournament)
        {
            if (TournamentsData.Tournaments.ContainsKey(tournament.TounamentID))
            {
                TournamentsData.Tournaments.Remove(tournament.TounamentID);
            }
        }

        private void ShowPrizeDialog(PrizeObject prize, bool includeCurrencies, Action<PrizeObject> modifyCallback)
        {
            if (CachedItemCategories == null || CachedItems == null || CacheCurrencies == null || CachedLootBoxCategories == null)
            {
                ShowProgress();
                var itemConfig = new ItemsConfigurator();
                itemConfig.GetCategories(categoriesResult => {
                    if (categoriesResult.Data.ContainsKey(CBSConstants.CategoriesKey))
                    {
                        var rawData = categoriesResult.Data[CBSConstants.CategoriesKey];
                        CachedItemCategories = JsonUtility.FromJson<Categories>(rawData);
                    }
                    else
                    {
                        CachedItemCategories = new Categories();
                    }

                    if (categoriesResult.Data.ContainsKey(CBSConstants.LootboxesCategoriesKey))
                    {
                        var rawData = categoriesResult.Data[CBSConstants.LootboxesCategoriesKey];
                        CachedLootBoxCategories = JsonUtility.FromJson<Categories>(rawData);
                    }
                    else
                    {
                        CachedLootBoxCategories = new Categories();
                    }

                    // get item catalog
                    itemConfig.GetItemsCatalog(itemsResult => {
                        HideProgress();
                        CachedItems = itemsResult.Catalog;
                        itemConfig.GetAllCurrencies(curResult => {
                            CacheCurrencies = curResult.VirtualCurrencies.Select(x => x.CurrencyCode).ToList();
                            // show prize windows
                            AddPrizeWindow.Show(new PrizeWindowRequest
                            {
                                currencies = CacheCurrencies,
                                includeCurencies = includeCurrencies,
                                itemCategories = CachedItemCategories,
                                lootboxCategories = CachedLootBoxCategories,
                                items = CachedItems,
                                modifyCallback = modifyCallback,
                                prize = prize
                            });
                            //GUIUtility.ExitGUI();
                        });
                    });
                });
            }
            else
            {
                // show prize windows
                AddPrizeWindow.Show(new PrizeWindowRequest
                {
                    currencies = CacheCurrencies,
                    includeCurencies = includeCurrencies,
                    itemCategories = CachedItemCategories,
                    lootboxCategories = CachedLootBoxCategories,
                    items = CachedItems,
                    modifyCallback = modifyCallback,
                    prize = prize
                });
                GUIUtility.ExitGUI();
            }
        }
    }
}
#endif
