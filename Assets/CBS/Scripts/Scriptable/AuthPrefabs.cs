using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "AuthPrefabs", menuName = "CBS/Add new Auth Prefabs")]
    public class AuthPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/AuthPrefabs";

        public GameObject LoginForm;
        public GameObject RegisterForm;
        public GameObject RecoveryForm;
        public GameObject Background;
    }

}