using CBS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Core.Auth
{
    public class Credential
    {
        private const string CredentialKey = "Credential.json";

        public static void Save<T>(T credential) where T : BaseCredential
        {
            DeviceStorage.SaveJsonToDisk(credential, CredentialKey);
        }

        public static T Get<T>() where T : BaseCredential
        {
            return DeviceStorage.GetJsonFromDisk<T>(CredentialKey);
        }

        public static void Clear()
        {
            DeviceStorage.Delete(CredentialKey);
        }

        public static bool Exist()
        {
            return Get<BaseCredential>() != null;
        }
    }

    public enum CredentialType
    {
        NONE,
        DEVICE_ID,
        CUSTOM_ID,
        PASSWORD,
        GOOGLE,
        FACEBOOK,
        STEAM,
        APPLE
    }

    [Serializable]
    public class BaseCredential
    {
        public CredentialType Type;
    }

    [Serializable]
    public class DeviceIDCredential : BaseCredential
    {
        public DeviceIDCredential()
        {
            Type = CredentialType.DEVICE_ID;
        }

        public string DeviceID;
    }

    [Serializable]
    public class CustomIDCredential : BaseCredential
    {
        public CustomIDCredential()
        {
            Type = CredentialType.CUSTOM_ID;
        }

        public string CustomID;
    }

    [Serializable]
    public class PasswordCredential : BaseCredential
    {
        public PasswordCredential()
        {
            Type = CredentialType.PASSWORD;
        }

        public string Mail;
        public string Password;
    }

    [Serializable]
    public class GoogleCredential : BaseCredential
    {
        public GoogleCredential()
        {
            Type = CredentialType.GOOGLE;
        }

        public string AuthCode;
    }

    [Serializable]
    public class FacebookCredential : BaseCredential
    {
        public FacebookCredential()
        {
            Type = CredentialType.FACEBOOK;
        }

        public string AccessToken;
    }

    [Serializable]
    public class SteamCredential : BaseCredential
    {
        public SteamCredential()
        {
            Type = CredentialType.STEAM;
        }

        public string SteamTicket;
    }

    [Serializable]
    public class AppleCredential : BaseCredential
    {
        public AppleCredential()
        {
            Type = CredentialType.APPLE;
        }

        public string IdentityToken;
    }
}
