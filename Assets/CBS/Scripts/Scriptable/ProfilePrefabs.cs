using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "ProfilePrefabs", menuName = "CBS/Add new Profile Prefabs")]
    public class ProfilePrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/ProfilePrefabs";

        public GameObject ProfileIcon;
        public GameObject AccountForm;
    }
}
