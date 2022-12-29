using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "PopupPrefabs", menuName = "CBS/Add new Popup prefabs")]
    public class PopupPrefabs : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/PopupPrefabs";

        public GameObject SimplePopup;
        public GameObject YesNoPopup;
        public GameObject UserInfoForm;
        public GameObject ClanInfoForm;
        public GameObject LoadingPopup;
        public GameObject RewardPopup;
    }

}