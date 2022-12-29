using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CBS.UI;

namespace CBS.Example
{
    public class ExampleExecuter : MonoBehaviour
    {
        [SerializeField]
        private string LobbyScene;
        [SerializeField]
        private InputField ExpInput;
        [SerializeField]
        private InputField LeaderboardInput;
        [SerializeField]
        private InputField TournamentsInput;

        private CBSProfile Profile { get; set; }
        private CBSClan Clan { get; set; }
        private CBSLeaderboard Leaderboard { get; set; }
        private CBSTournament Tournament { get; set; }

        private string PlayerID { get; set; }

        private void Start()
        {
            Profile = CBSModule.Get<CBSProfile>();
            Leaderboard = CBSModule.Get<CBSLeaderboard>();
            Clan = CBSModule.Get<CBSClan>();
            Tournament = CBSModule.Get<CBSTournament>();
            PlayerID = Profile.PlayerID;
        }

        public void BackToLobby()
        {
            SceneManager.LoadScene(LobbyScene);
        }

        public void AddExpPoints()
        {
            int val = 0;
            string input = ExpInput.text;
            var result = int.TryParse(input, out val);
            if (result)
            {
                Profile.AddPlayerExp(val, onAdd => {
                    if (onAdd.IsSuccess)
                    {
                        new PopupViewer().ShowSimplePopup(new PopupRequest {
                            Title = "Success",
                            Body = "You are successful add exp points"
                        });
                    }
                    else
                    {
                        new PopupViewer().ShowFabError(onAdd.Error);
                    }
                });
            }
        }

        public void AddClanLeaderboardPoint()
        {
            int val = 0;
            string input = LeaderboardInput.text;
            var result = int.TryParse(input, out val);
            if (result)
            {
                Clan.ExistInClan(onCheck => {
                    if (onCheck.IsSuccess)
                    {
                        if (onCheck.ExistInClan)
                        {
                            string id = onCheck.ClanID;

                            Leaderboard.AddClanStatisticPoint(id, val, onAdd => {
                                if (onAdd.IsSuccess)
                                {
                                    new PopupViewer().ShowSimplePopup(new PopupRequest
                                    {
                                        Title = "Success",
                                        Body = "You are successful add clan points"
                                    });
                                }
                                else
                                {
                                    new PopupViewer().ShowFabError(onAdd.Error);
                                }
                            });
                        }
                        else
                        {
                            new PopupViewer().ShowSimplePopup(new PopupRequest
                            {
                                Title = "Failed",
                                Body = "You are not exist in any clan"
                            });
                        }
                    }
                });
            }
        }

        public void AddTournamentPoints()
        {
            int val = 0;
            string input = TournamentsInput.text;
            var result = int.TryParse(input, out val);
            if (result)
            {
                Tournament.AddTournamentPoint(val, onAdd => {
                    if (onAdd.IsSuccess)
                    {
                        new PopupViewer().ShowSimplePopup(new PopupRequest
                        {
                            Title = "Success",
                            Body = "You are successful add tournament points"
                        });
                    }
                    else
                    {
                        new PopupViewer().ShowFabError(onAdd.Error);
                    }
                });
            }
        }
    }
}
