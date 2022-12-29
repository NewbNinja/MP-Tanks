using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ItemsIcons", menuName = "CBS/Add new Items Sprite pack")]
    public class ItemsIcons : IconsData
    {
        public override string ResourcePath => "ItemsIcons";

        public override string EditorPath => "Assets/CBS_External/Resources";

        public override string EditorAssetName => "ItemsIcons.asset";
    }
}
