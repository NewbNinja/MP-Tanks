using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class NoClanWindow : MonoBehaviour
    {
        [SerializeField]
        private ClanTabListener TabListener;
        [SerializeField]
        private GameObject GeneralInfo;
        [SerializeField]
        private GameObject CreateForm;
        [SerializeField]
        private GameObject Invatations;
        [SerializeField]
        private GameObject Search;

        private void Awake()
        {
            TabListener.OnTabSelected += OnTabSelected;

            CreateForm.GetComponent<CreateClanForm>().SetBackAction(CancelCreate);
        }

        private void OnDestroy()
        {
            TabListener.OnTabSelected -= OnTabSelected;
        }

        private void OnEnable()
        {
            var activeTab = TabListener.ActiveTab;
            DisplayTab(activeTab);
        }

        private void OnTabSelected(ClanTabType type)
        {
            DisplayTab(type);
        }

        private void DisplayTab(ClanTabType type)
        {
            CreateForm.SetActive(false);
            GeneralInfo.SetActive(type == ClanTabType.GENERAL);
            Invatations.SetActive(type == ClanTabType.INVITATIONS);
            Search.SetActive(type == ClanTabType.SEARCH);
        }

        private void CancelCreate()
        {
            DisplayTab(ClanTabType.GENERAL);
        }

        // buttons events
        public void ShowCreateClan()
        {
            GeneralInfo.SetActive(false);
            CreateForm.SetActive(true);
        }

        public void ShowFind()
        {
            DisplayTab(ClanTabType.SEARCH);
        }
    }
}