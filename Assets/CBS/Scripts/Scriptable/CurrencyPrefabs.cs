using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "CurrencyPrefabs", menuName = "CBS/Add new Currency Prefabs")]
    public class CurrencyPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/CurrencyPrefabs";

        public GameObject CurrencyPanel;
        public GameObject CurrencyItem;
        public GameObject CurrenciesPacks;
        public GameObject CurrencyPackItem;
        public GameObject CurrencySlot;
    }
}
