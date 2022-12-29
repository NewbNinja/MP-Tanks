using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "EditorData", menuName = "CBS/Add new Editor Data")]
    public class EditorData : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/Core/EditorData";

        public Color AddColor;
        public Color AddPrizeColor;
        public Color RemoveColor;
        public Color SaveColor;
        public Color EditColor;
        public Color DuplicateColor;
    }
}
