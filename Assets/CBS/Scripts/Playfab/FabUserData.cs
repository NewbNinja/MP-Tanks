using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace CBS.Playfab
{
    public class FabUserData : FabExecuter
    {
        public void GetUserData(string dataKey, Action<GetUserDataResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var request = new GetUserDataRequest { Keys = new List<string>() { dataKey } };
            PlayFabClientAPI.GetUserData(request, OnGet, OnFailed);
        }

        public void UpdateUserData(string dataKey, string value, Action<UpdateUserDataResult> OnGet, Action<PlayFabError> OnFailed)
        {
            var requestData = new Dictionary<string, string>();
            requestData.Add(dataKey, value);
            var request = new UpdateUserDataRequest { Data = requestData };
            PlayFabClientAPI.UpdateUserData(request, OnGet, OnFailed);
        }
    }
}
