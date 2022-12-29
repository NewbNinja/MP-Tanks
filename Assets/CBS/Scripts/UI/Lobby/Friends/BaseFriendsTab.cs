using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class BaseFriendsTab : BaseTab<string>
    {
        [SerializeField]
        private FriendsTabTitle TabTitle;

        private void Start()
        {
            TabObject = TabTitle.ToString();
        }
    }
}
