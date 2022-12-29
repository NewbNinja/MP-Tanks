using PlayFab;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public static class PlayfabUtils
    {
        public static string ToJson(this PlayFabAuthenticationContext context)
        {
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            return jsonPlugin.SerializeObject(context);
        }

        public static string ToJson(this MatchmakingQueueConfig context)
        {
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            return jsonPlugin.SerializeObject(context);
        }
    }
}
