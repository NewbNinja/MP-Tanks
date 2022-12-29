using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "AuthData", menuName = "CBS/Add new Auth Data")]
    public class AuthData : CBSScriptable
    {
        public override string ResourcePath => "Scriptable/Core/AuthData";

        [Header("Preload data after login")]
        public bool PreloadAccountInfo;
        public bool PreloadLevelData;
        public bool PreloadCurrency;
        public bool PreloadStatistics;
        public bool PreloadInventory;
        public bool PreloadUserData;
        [Header("Display name")]
        public bool AutoGenerateRandomNickname;
        public string RandomNamePrefix;
        public bool AutoLogin;
        public DeviceIdDataProvider DeviceIdProvider;
    }
}
