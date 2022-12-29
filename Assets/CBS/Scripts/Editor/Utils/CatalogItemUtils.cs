#if ENABLE_PLAYFABADMIN_API
using CBS.Scriptable;
using PlayFab.AdminModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Editor
{
    public static class CatalogItemUtils
    {
        public static string GetNextID(this CatalogItem item)
        {
            var currentID = item.ItemId;
            var lastIntChars = string.Empty;
            foreach (var c in currentID)
            {
                var isInt = Char.IsNumber(c);
                if (isInt)
                {
                    lastIntChars += c;
                }
                else
                {
                    lastIntChars = string.Empty;
                }
            }
            if (string.IsNullOrEmpty(lastIntChars))
            {
                return currentID + "1";
            }
            else
            {
                var findInts = int.Parse(lastIntChars);
                findInts += 1;
                var newID = currentID.Replace(lastIntChars, string.Empty);
                return newID + findInts.ToString();
            }
        }

        public static CatalogItem Duplicate(this CatalogItem item, string newItemID)
        {
            var itemIcons = CBSScriptable.Get<ItemsIcons>();
            var currentSprite = itemIcons.GetSprite(item.ItemId);
            if (currentSprite != null)
            {
                itemIcons.SaveSprite(newItemID, currentSprite);
                itemIcons.Save();
            }

            return new CatalogItem
            {
                Bundle = item.Bundle,
                CanBecomeCharacter = item.CanBecomeCharacter,
                CatalogVersion = item.CatalogVersion,
                Consumable = item.Consumable,
                Container = item.Container,
                CustomData = item.CustomData,
                Description = item.Description,
                DisplayName = item.DisplayName,
                InitialLimitedEditionCount = item.InitialLimitedEditionCount,
                IsLimitedEdition = item.IsLimitedEdition,
                IsStackable = item.IsStackable,
                IsTradable = item.IsTradable,
                ItemClass = item.ItemClass,
                ItemId = newItemID,
                ItemImageUrl = item.ItemImageUrl,
                RealCurrencyPrices = item.RealCurrencyPrices,
                Tags = item.Tags,
                VirtualCurrencyPrices = item.VirtualCurrencyPrices
            };
        }
    }
}
#endif
