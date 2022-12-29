using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabTitleData : FabExecuter, IFabTitle
    {
        public void GetTitleData (string [] keys, Action<GetTitleDataResult> onGet, Action<PlayFabError> onFailed)
        {
            var request = new GetTitleDataRequest {
                Keys = keys.ToList()
            };
            PlayFabClientAPI.GetTitleData(request, onGet, onFailed);
        }

        public void GetTitleData(string key, Action<GetTitleDataResult> onGet, Action<PlayFabError> onFailed)
        {
            var request = new GetTitleDataRequest
            {
                Keys = new string[] { key }.ToList()
            };
            PlayFabClientAPI.GetTitleData(request, onGet, onFailed);
        }
    }
}
