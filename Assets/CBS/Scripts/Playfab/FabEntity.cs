using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.DataModels;
using System;

namespace CBS.Playfab
{
    public class FabEntity : FabExecuter, IFabEntity
    {
        public void SetObject(EntityKey entity, string key, string value, Action<SetObjectsResponse> onUpdate, Action<PlayFabError> onFailed)
        {
            var objectToUpdate = new List<SetObject>();
            objectToUpdate.Add(new SetObject { 
                ObjectName = key,
                DataObject = value
            });

            var request = new SetObjectsRequest {
                Entity = entity,
                Objects = objectToUpdate
            };
            PlayFabDataAPI.SetObjects(request, onUpdate, onFailed);
        }

        public void GetObjects(EntityKey entity, Action<GetObjectsResponse> onGet, Action<PlayFabError> onFailed)
        {
            var request = new GetObjectsRequest { 
                Entity = entity
            };
            PlayFabDataAPI.GetObjects(request, onGet, onFailed);
        }
    }
}
