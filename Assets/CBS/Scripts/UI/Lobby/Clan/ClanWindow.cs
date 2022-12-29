using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanWindow : MonoBehaviour
    {
        [SerializeField]
        private ClanTabListener TabListener;
        [SerializeField]
        private GameObject GeneralInfo;
        [SerializeField]
        private GameObject Invatations;
        [SerializeField]
        private GameObject Members;
        [SerializeField]
        private GameObject Chat;

        private string ClanID { get; set; }

        private void Awake()
        {
            TabListener.OnTabSelected += OnTabSelected;
        }

        private void OnDestroy()
        {
            TabListener.OnTabSelected -= OnTabSelected;
        }

        public void DisplayClan(string clanID)
        {
            HideAll();
            ClanID = clanID;
            ApplyClanID(ClanID);

            var activeTab = TabListener.ActiveTab;
            DisplayTab(activeTab);
        }

        private void OnTabSelected(ClanTabType type)
        {
            DisplayTab(type);
        }

        private void DisplayTab(ClanTabType type)
        {
            GeneralInfo.SetActive(type == ClanTabType.GENERAL);
            Invatations.SetActive(type == ClanTabType.INVITATIONS);
            Members.SetActive(type == ClanTabType.MEMBERS);
            Chat.SetActive(type == ClanTabType.CHAT);
        }

        private void ApplyClanID(string clanID)
        {
            GeneralInfo.GetComponent<ISetClan>().SetClanID(clanID);
            Invatations.GetComponent<ISetClan>().SetClanID(clanID);
            Chat.GetComponent<ISetClan>().SetClanID(clanID);
            Members.GetComponent<ISetClan>().SetClanID(clanID);
        }

        private void HideAll()
        {
            GeneralInfo.SetActive(false);
            Invatations.SetActive(false);
            Members.SetActive(false);
            Chat.SetActive(false);
        }
    }
}