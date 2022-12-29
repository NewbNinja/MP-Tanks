using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "CurrencyIcons", menuName = "CBS/Add new Currency Sprite pack")]
    public class CurrencyIcons : IconsData
    {
        public override string ResourcePath => "CurrencyIcons";

        public override string EditorPath => "Assets/CBS_External/Resources";

        public override string EditorAssetName => "CurrencyIcons.asset";
    }
}
