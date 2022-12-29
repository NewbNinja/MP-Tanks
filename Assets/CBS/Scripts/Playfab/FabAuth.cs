using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using CBS.Scriptable;

namespace CBS.Playfab
{
    public class FabAuth : FabExecuter, IFabAuth
    {
        private FabAccount PlayfabAccount { get; set; }
        private FabInventory PlayfabInventory { get; set; }
        private AuthData AuthData { get; set; }
        private IFabItems FabItems { get; set; }

        public bool IsLoggined => PlayFabClientAPI.IsClientLoggedIn();

        protected override void Init()
        {
            PlayfabAccount = Get<FabAccount>();
            PlayfabInventory = Get<FabInventory>();
            FabItems = Get<FabItems>();
            AuthData = CBSScriptable.Get<AuthData>();
        }

        public void RegisterWithMailAndPassword(CBSMailRegistrationRequest request, Action<RegisterPlayFabUserResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var regRequest = new RegisterPlayFabUserRequest
            {
                Email = request.Mail,
                Password = request.Password,
                DisplayName = request.DisplayName,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(regRequest, success => {
                GrandRegistrationPrize(success.PlayFabId, grandResult => {
                    onSuccess?.Invoke(success);
                });
            }, onFailed);
        }

        public void LoginWithMailAndPassword(CBSMailLoginRequest request, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var loginRequest = new LoginWithEmailAddressRequest
            {
                Email = request.Mail,
                Password = request.Password,
                InfoRequestParameters = GetLoginRequestParams()
            };

            PlayFabClientAPI.LoginWithEmailAddress(loginRequest, onSuccess, onFailed);
        }

        public void LoginWithCustomID(string id, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var request = new LoginWithCustomIDRequest { CustomId = id,
                CreateAccount = true,
                InfoRequestParameters = GetLoginRequestParams()
            };

            PlayFabClientAPI.LoginWithCustomID(request, result => {
                if (result.NewlyCreated)
                {
                    GrandRegistrationPrize(result.PlayFabId, grandResult => {
                        PostRegistrationProccess(result, onSuccess, onFailed);
                    });
                }
                else
                {
                    onSuccess.Invoke(result);
                }
            }, onFailed);
        }

        public void LoginWithGoogle(string serverAuthCode, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var request = new LoginWithGoogleAccountRequest {
                CreateAccount = true,
                InfoRequestParameters = GetLoginRequestParams(),
                ServerAuthCode = serverAuthCode
            };
            PlayFabClientAPI.LoginWithGoogleAccount(request, result=> {
                if (result.NewlyCreated)
                {
                    GrandRegistrationPrize(result.PlayFabId, grandResult => {
                        PostRegistrationProccess(result, onSuccess, onFailed);
                    });
                }
                else
                {
                    onSuccess.Invoke(result);
                }
            }, onFailed);
        }

        public void LoginWithSteam(string steamTicket, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var request = new LoginWithSteamRequest {
                CreateAccount = true,
                InfoRequestParameters = GetLoginRequestParams(),
                SteamTicket = steamTicket
            };
            PlayFabClientAPI.LoginWithSteam(request, result => {
                if (result.NewlyCreated)
                {
                    GrandRegistrationPrize(result.PlayFabId, grandResult => {
                        PostRegistrationProccess(result, onSuccess, onFailed);
                    });
                }
                else
                {
                    onSuccess.Invoke(result);
                }
            }, onFailed);
        }

        public void LoginWithFacebook(string accessToken, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var request = new LoginWithFacebookRequest
            {
                CreateAccount = true,
                InfoRequestParameters = GetLoginRequestParams(),
                AccessToken = accessToken
            };
            PlayFabClientAPI.LoginWithFacebook(request, result => {
                if (result.NewlyCreated)
                {
                    GrandRegistrationPrize(result.PlayFabId, grandResult => {
                        PostRegistrationProccess(result, onSuccess, onFailed);
                    });
                }
                else
                {
                    onSuccess.Invoke(result);
                }
            }, onFailed);
        }

        public void LoginWithApple(string identityToken, Action<LoginResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var request = new LoginWithAppleRequest
            {
                CreateAccount = true,
                InfoRequestParameters = GetLoginRequestParams(),
                IdentityToken = identityToken
            };
            PlayFabClientAPI.LoginWithApple(request, result => {
                if (result.NewlyCreated)
                {
                    GrandRegistrationPrize(result.PlayFabId, grandResult => {
                        PostRegistrationProccess(result, onSuccess, onFailed);
                    });
                }
                else
                {
                    onSuccess.Invoke(result);
                }
            }, onFailed);
        }

        public void SendPasswordRecovery(string mail, Action<SendAccountRecoveryEmailResult> onSuccess, Action<PlayFabError> onFailed)
        {
            var request = new SendAccountRecoveryEmailRequest {
                Email = mail,
                TitleId = PlayFabSettings.TitleId
            };
            PlayFabClientAPI.SendAccountRecoveryEmail(request, onSuccess, onFailed);
        }

        public void Logout()
        {
            PlayFabClientAPI.ForgetAllCredentials();
        }

        private void PostRegistrationProccess(LoginResult result, Action<LoginResult> onLogin, Action<PlayFabError> onFailed)
        {
            bool autoGenerateRandomName = AuthData.AutoGenerateRandomNickname;
            string _playerId = result.PlayFabId;
            string _newName = autoGenerateRandomName ? AuthData.RandomNamePrefix + _playerId : string.Empty;
            Debug.Log("Registration name = "+_newName);
            // set default name on player registration
            if (autoGenerateRandomName)
            {
                PlayfabAccount.UpdateUserDisplayName(_newName, OnSuccess => {
                    result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName = OnSuccess.DisplayName;

                    bool preloadInvertory = AuthData.PreloadInventory;
                    if (preloadInvertory)
                    {
                        // get invertory
                        PlayfabInventory.GetInventory(OnGetInvertory =>
                        {
                            result.InfoResultPayload.UserInventory = OnGetInvertory.Inventory;
                            onLogin.Invoke(result);
                        }, OnFailedInvertory =>
                        {
                            onFailed.Invoke(OnFailedInvertory);
                        });
                    }
                    else
                    {
                        onLogin.Invoke(result);
                    }
                }, OnError => {
                    onFailed.Invoke(OnError);
                });
            }
            else
            {
                bool preloadInvertory = AuthData.PreloadInventory;
                if (preloadInvertory)
                {
                    // get invertory
                    PlayfabInventory.GetInventory(OnGetInvertory =>
                    {
                        result.InfoResultPayload.UserInventory = OnGetInvertory.Inventory;
                        onLogin.Invoke(result);
                    }, OnFailedInvertory =>
                    {
                        onFailed.Invoke(OnFailedInvertory);
                    });
                }
                else
                {
                    onLogin.Invoke(result);
                }
            }
            
        }

        private void GrandRegistrationPrize(string playerID, Action<CBSRegistrationPrizeResult> result)
        {
            FabItems.GrandRegistrationPrize(playerID, grandResult => {
                result?.Invoke(new CBSRegistrationPrizeResult
                {
                    IsSuccess = true
                });
            }, onError => {
                result?.Invoke(new CBSRegistrationPrizeResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                });
            });
        }

        private GetPlayerCombinedInfoRequestParams GetLoginRequestParams()
        {
            return new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = AuthData.PreloadAccountInfo,
                GetPlayerProfile = AuthData.PreloadAccountInfo,
                GetUserVirtualCurrency = AuthData.PreloadCurrency,
                GetPlayerStatistics = AuthData.PreloadStatistics,
                GetUserInventory = AuthData.PreloadInventory,
                GetUserData = AuthData.PreloadUserData
            };
        }
    }
}
