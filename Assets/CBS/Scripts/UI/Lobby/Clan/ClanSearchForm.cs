using CBS.Core;
using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CBS.UI
{
    public class ClanSearchForm : MonoBehaviour
    {
        [SerializeField]
        private InputField SearchInput;
        [SerializeField]
        private BaseScroller Scroller;

        private IClan CBSClan { get; set; }
        private ClanPrefabs Prefabs { get; set; }

        private Dictionary<string, string> CurrentList = new Dictionary<string, string>();

        private void Awake()
        {
            CBSClan = CBSModule.Get<CBSClan>();
            Prefabs = CBSScriptable.Get<ClanPrefabs>();
            Scroller.OnSpawn += OnClanSpawn;
        }

        private void OnDestroy()
        {
            Scroller.OnSpawn -= OnClanSpawn;
        }

        private void OnEnable()
        {
            Scroller.Clear();
            CBSClan.GetClanList(OnClanListGetted);
        }

        private void OnDisable()
        {
            SearchInput.text = string.Empty;
        }

        private void OnClanListGetted(GetClanListResult result)
        {
            if (result.IsSuccess)
            {
                CurrentList = result.Clans;
                int listCount = CurrentList.Count;
                var clanPrefab = Prefabs.ClanSearchResult;
                Scroller.SpawnItems(clanPrefab, listCount);
            }
        }

        private void OnClanSpawn(GameObject uiItem, int index)
        {
            var clan = CurrentList.ElementAt(index);
            var resultObject = uiItem.GetComponent<ClanSearchResult>();
            resultObject.Display(clan.Key, clan.Value);
        }

        // button events
        public void OnSearch()
        {
            Scroller.Clear();
            string searchValue = SearchInput.text;
            if (string.IsNullOrEmpty(searchValue))
            {
                CBSClan.GetClanList(OnClanListGetted);
            }
            else
            {
                CBSClan.SearchClanByName(searchValue, OnClanListGetted);
            }
        }
    }
}
