using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class LootBoxBadge : BaseBadge
    {
        private ICBSInventory CBSInventory { get; set; }

        private void Awake()
        {
            CBSInventory = CBSModule.Get<CBSInventory>();
            // try get from cache
            var lootBoxes = CBSInventory.GetLootboxesFromCache().Lootboxes;
            UpdateCount(lootBoxes == null ? 0 : lootBoxes.Count);
        }

        private void OnEnable()
        {
            CBSInventory.OnLootboxAdded += OnLootBoxAdded;
            CBSInventory.OnLootboxOpen += OnLootBoxOpen;
            GetLootboxes();
        }

        private void OnDisable()
        {
            CBSInventory.OnLootboxAdded -= OnLootBoxAdded;
            CBSInventory.OnLootboxOpen -= OnLootBoxOpen;
        }

        private void GetLootboxes()
        {
            CBSInventory.GetLootboxes(OnLootBoxGetted);
        }

        private void OnLootBoxGetted(GetInventoryLootboxesResult result)
        {
            var lootBoxes = result.Lootboxes;
            UpdateCount(lootBoxes == null ? 0 : lootBoxes.Count);
        }

        private void OnLootBoxAdded(InventoryLootboxGrandResult obj)
        {
            var lootBoxes = CBSInventory.GetLootboxesFromCache().Lootboxes;
            UpdateCount(lootBoxes == null ? 0 : lootBoxes.Count);
        }

        private void OnLootBoxOpen(OpenLootboxResult result)
        {
            GetLootboxes();
        }
    }
}
