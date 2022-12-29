using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Playfab
{
    public interface IFabAuth
    {
        bool IsLoggined { get; }

        void RegisterWithMailAndPassword(CBSMailRegistrationRequest request, Action<RegisterPlayFabUserResult> onSuccess, Action<PlayFabError> onFailed);

        void LoginWithMailAndPassword(CBSMailLoginRequest request, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed);

        void LoginWithCustomID(string id, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed);

        void SendPasswordRecovery(string mail, Action<SendAccountRecoveryEmailResult> onSuccess, Action<PlayFabError> onFailed);

        void LoginWithGoogle(string serverAuthCode, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed);

        void LoginWithSteam(string steamTicket, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed);

        void LoginWithFacebook(string accessToken, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed);

        void LoginWithApple(string identityToken, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed);

        void Logout();
    }
}
