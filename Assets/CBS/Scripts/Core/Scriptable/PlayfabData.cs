using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "PlayfabData", menuName = "CBS/Add new Playfab Data")]
    public class PlayfabData : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/Core/PlayfabData";

        public bool EnableCustomCloudScript;
    }
}
