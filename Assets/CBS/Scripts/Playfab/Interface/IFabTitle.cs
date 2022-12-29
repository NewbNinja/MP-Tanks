using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabTitle
    {
        void GetTitleData(string[] keys, Action<GetTitleDataResult> onGet, Action<PlayFabError> onFailed);

        void GetTitleData(string key, Action<GetTitleDataResult> onGet, Action<PlayFabError> onFailed);
    }
}