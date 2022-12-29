using CBS.Scriptable;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Other
{
    public class Device
    {
#if UNITY_WEBGL
        private static readonly string DeviceIDKey = "CBSDeviceID";
#endif

        public static string DEVICE_ID
        {
            get
            {
#if UNITY_WEBGL
                if (PlayerPrefs.HasKey(DeviceIDKey))
                {
                    var deviceID = PlayerPrefs.GetString(DeviceIDKey);
                    return deviceID;
                }
                else
                {
                    var deviceID = System.Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(DeviceIDKey, deviceID);
                    PlayerPrefs.Save();
                    return deviceID;
                }
#endif
                var authData = CBSScriptable.Get<AuthData>();
                if (authData.DeviceIdProvider == DeviceIdDataProvider.PLAYFAB_DEVICE_ID)
                {
                    return PlayFabSettings.DeviceUniqueIdentifier;
                }
                else if (authData.DeviceIdProvider == DeviceIdDataProvider.SYSTEM_UNIQUE_ID)
                {
                    return SystemInfo.deviceUniqueIdentifier;
                }
                return SystemInfo.deviceUniqueIdentifier;
            }
        }
    }
}