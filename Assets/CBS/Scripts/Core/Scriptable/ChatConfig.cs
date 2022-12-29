using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ChatConfig", menuName = "CBS/Add new Chat Config")]
    public class ChatConfig : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/Core/ChatConfig";

        public int MaxMessageLength = 256;
    }
}
