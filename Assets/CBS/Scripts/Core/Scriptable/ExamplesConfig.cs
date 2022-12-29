using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ExamplesConfig", menuName = "CBS/Add new ExamplesConfig")]
    public class ExamplesConfig : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/Core/ExamplesConfig";

        public AvatarDisplayOptions AvatarDisplay;

        public bool UseCacheForAvatars;

        public Sprite DefaultAvatar;

        public Sprite DefaultClanAvatar;

        public bool UseImageCache
        {
            get
            {
#if UNITY_WEBGL
                return false;
#endif
                return UseCacheForAvatars;
            }
        }
    }
}
