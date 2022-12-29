using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.DataModels;
using System;
using PlayFab;

namespace CBS.Playfab
{
    public interface IFabEntity
    {
        void SetObject(EntityKey entity, string key, string value, Action<SetObjectsResponse> onUpdate, Action<PlayFabError> onFailed);
        void GetObjects(EntityKey entity, Action<GetObjectsResponse> onGet, Action<PlayFabError> onFailed);
    }
}
