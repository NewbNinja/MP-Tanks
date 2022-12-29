using CBS.Core;
using CBS.Core.Auth;
using CBS.Other;
using CBS.Playfab;
using CBS.Scriptable;
using PlayFab.ClientModels;
using System;
using System.Collections;
using UnityEngine;

namespace CBS
{
    public class CBSAuth : CBSModule, IAuth
    {
        /// <summary>
        /// An event that reports a successful user login
        /// </summary>
        public event Action<CBSLoginResult> OnLoginEvent;

        /// <summary>
        /// An event that reports when the user logged out
        /// </summary>
        public event Action<CBSLogoutResult> OnLogoutEvent;

        private IFabAuth FabAuth { get; set; }
        private AuthData AuthData { get; set; }

        protected override void Init()
        {
            FabAuth = FabExecuter.Get<FabAuth>();
            AuthData = CBSScriptable.Get<AuthData>();
        }

        /// <summary>
        /// Authorization using login and password. No automatic registration. Before login, you need to register
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void LoginWithMailAndPassword(CBSMailLoginRequest request, Action<CBSLoginResult> result)
        {
            FabAuth.LoginWithMailAndPassword(request, onSuccess => 
            {
                var successResult = new CBSLoginResult {
                    IsSuccess = true,
                    IsNew = false,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new PasswordCredential
                    {
                        Mail = request.Mail,
                        Password = request.Password,
                        Type = CredentialType.PASSWORD
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError => 
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization that binds the account with the current ID of the device on which the application was launched. Auto-register user if there is no such user in the database.
        /// </summary>
        /// <param name="result"></param>
        public void LoginWithCustomID(string customID, Action<CBSLoginResult> result)
        {
            FabAuth.LoginWithCustomID(customID, onSuccess =>
            {
                var successResult = new CBSLoginResult
                {
                    IsSuccess = true,
                    IsNew = onSuccess.NewlyCreated,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new CustomIDCredential { 
                        CustomID = customID,
                        Type = CredentialType.CUSTOM_ID
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError =>
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization using your own custom identifier. Auto-register user if there is no such user in the database.
        /// </summary>
        /// <param name="customID"></param>
        /// <param name="result"></param>
        public void LoginWithDeviceID(Action<CBSLoginResult> result)
        {
            string deviceID = Device.DEVICE_ID;

            FabAuth.LoginWithCustomID(deviceID, onSuccess => 
            {
                var successResult = new CBSLoginResult
                {
                    IsSuccess = true,
                    IsNew = onSuccess.NewlyCreated,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new DeviceIDCredential
                    {
                        DeviceID = deviceID,
                        Type = CredentialType.DEVICE_ID
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError => 
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization with google account. Required server auth code.
        /// </summary>
        /// <param name="serverAuthCode"></param>
        /// <param name="result"></param>
        public void LoginWithGoolge(string serverAuthCode, Action<CBSLoginResult> result)
        {
            FabAuth.LoginWithGoogle(serverAuthCode, onSuccess =>
            {
                var successResult = new CBSLoginResult
                {
                    IsSuccess = true,
                    IsNew = onSuccess.NewlyCreated,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new GoogleCredential
                    {
                        AuthCode = serverAuthCode,
                        Type = CredentialType.GOOGLE
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError =>
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization with Steam account. Required steam ticket.
        /// </summary>
        /// <param name="steamTicket"></param>
        /// <param name="result"></param>
        public void LoginWithSteam(string steamTicket, Action<CBSLoginResult> result)
        {
            FabAuth.LoginWithSteam(steamTicket, onSuccess =>
            {
                var successResult = new CBSLoginResult
                {
                    IsSuccess = true,
                    IsNew = onSuccess.NewlyCreated,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new SteamCredential
                    {
                        SteamTicket = steamTicket,
                        Type = CredentialType.STEAM
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError =>
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization with Facebook account. Required access token.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="result"></param>
        public void LoginWithFacebook(string accessToken, Action<CBSLoginResult> result)
        {
            FabAuth.LoginWithFacebook(accessToken, onSuccess =>
            {
                var successResult = new CBSLoginResult
                {
                    IsSuccess = true,
                    IsNew = onSuccess.NewlyCreated,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new FacebookCredential
                    {
                        AccessToken = accessToken,
                        Type = CredentialType.FACEBOOK
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError =>
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization with Apple account. Required apple identity Token.
        /// </summary>
        /// <param name="identityToken"></param>
        /// <param name="result"></param>
        public void LoginWithApple(string identityToken, Action<CBSLoginResult> result)
        {
            FabAuth.LoginWithApple(identityToken, onSuccess =>
            {
                var successResult = new CBSLoginResult
                {
                    IsSuccess = true,
                    IsNew = onSuccess.NewlyCreated,
                    PlayerId = onSuccess.PlayFabId,
                    Result = onSuccess
                };

                // generate creds
                var autoLogin = AuthData.AutoLogin;
                if (autoLogin)
                {
                    var credential = new AppleCredential
                    {
                        IdentityToken = identityToken,
                        Type = CredentialType.FACEBOOK
                    };
                    successResult.Credential = credential;
                }

                LoginPostProcess(result, successResult);
            }, onError =>
            {
                var failedResult = new CBSLoginResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Authorization based on the last successful login. Enable this setting in CBS Configurator-> Configurator-> Auth-> Enable AutoLogin
        /// </summary>
        /// <param name="result"></param>
        public void AutoLogin(Action<CBSLoginResult> result)
        {
            if (Credential.Exist())
            {
                var baseCredential = Credential.Get<BaseCredential>();
                var type = baseCredential.Type;
                if (type == CredentialType.CUSTOM_ID)
                {
                    var credential = Credential.Get<CustomIDCredential>();
                    var customID = credential.CustomID;
                    LoginWithCustomID(customID, result);
                }
                else if (type == CredentialType.DEVICE_ID)
                {
                    LoginWithDeviceID(result);
                }
                else if (type == CredentialType.FACEBOOK)
                {
                    var credential = Credential.Get<FacebookCredential>();
                    var accessToken = credential.AccessToken;
                    LoginWithFacebook(accessToken, result);
                }
                else if (type == CredentialType.GOOGLE)
                {
                    var credential = Credential.Get<GoogleCredential>();
                    var authCode = credential.AuthCode;
                    LoginWithGoolge(authCode, result);
                }
                else if (type == CredentialType.STEAM)
                {
                    var credential = Credential.Get<SteamCredential>();
                    var steamTicket = credential.SteamTicket;
                    LoginWithSteam(steamTicket, result);
                }
                else if (type == CredentialType.PASSWORD)
                {
                    var credential = Credential.Get<PasswordCredential>();
                    var mail = credential.Mail;
                    var password = credential.Password;
                    LoginWithMailAndPassword(new CBSMailLoginRequest {
                        Mail = mail,
                        Password = password
                    }, result);
                }
                else if (type == CredentialType.APPLE)
                {
                    var credential = Credential.Get<AppleCredential>();
                    var identityToken = credential.IdentityToken;
                    LoginWithApple(identityToken, result);
                }
                else
                {
                    result?.Invoke(new CBSLoginResult
                    {
                        IsSuccess = false,
                        Error = SimpleError.CredentialNotFound()
                    });
                }
            }
            else
            {
                result?.Invoke(new CBSLoginResult { 
                    IsSuccess = false,
                    Error = SimpleError.CredentialNotFound()
                });
            }
        }

        /// <summary>
        /// User registration using mail and password. Auto generation of the name is not applied. The name must be specified in the request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public void RegisterWithMailAndPassword(CBSMailRegistrationRequest request, Action<CBSMailRegistrationResult> result)
        {
            FabAuth.RegisterWithMailAndPassword(request, onSuccess => 
            {
                var successResult = new CBSMailRegistrationResult
                {
                    IsSuccess = true,
                    PlayerId = onSuccess.PlayFabId
                };
                    result?.Invoke(successResult);
            }, onError => {
                var failedResult = new CBSMailRegistrationResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Recovering a player's password using mail. Works only for users who have registered using mail and password.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="result"></param>
        public void SendPasswordRecovery(string mail, Action<CBSSendRecoveryResult> result)
        {
            FabAuth.SendPasswordRecovery(mail, onSuccess => 
            {
                var successResult = new CBSSendRecoveryResult
                {
                    IsSuccess = true,
                };
                result?.Invoke(successResult);
            }, onError => 
            {
                var failedResult = new CBSSendRecoveryResult
                {
                    IsSuccess = false,
                    Error = SimpleError.FromTemplate(onError)
                };
                result?.Invoke(failedResult);
            });
        }

        /// <summary>
        /// Sign Out. Stops running and executing CBS scripts. Clears all cached information.
        /// </summary>
        /// <param name="result"></param>
        public void Logout(Action<CBSLogoutResult> result = null)
        {
            FabAuth.Logout();
            LogoutPrecces();
            var logoutResult = new CBSLogoutResult
            {
                IsSuccess = true
            };
            Credential.Clear();
            OnLogoutEvent?.Invoke(logoutResult);
            result?.Invoke(logoutResult);
        }

        // other tools
        private void LoginPostProcess(Action<CBSLoginResult> successAction, CBSLoginResult result)
        {
            // bind modules
            Get<CBSProfile>().Bind();
            Get<CBSCurrency>().Bind();
            Get<CBSItems>().Bind();
            Get<CBSInventory>().Bind();

            bool preloadLevelData = AuthData.PreloadLevelData;
            if (preloadLevelData)
            {
                Get<CBSProfile>().GetPlayerLevelData(levelResult => {
                    if (levelResult.IsSuccess)
                    {
                        PreloadItems(successAction, result);
                    }
                    else
                    {
                        var failedResult = new CBSLoginResult
                        {
                            IsSuccess = false,
                            Error = levelResult.Error
                        };
                        successAction?.Invoke(failedResult);
                    }
                });
            }
            else
            {
                
                PreloadItems(successAction, result);
            }
        }

        private void PreloadItems(Action<CBSLoginResult> successAction, CBSLoginResult result)
        {
            Get<CBSItems>().FetchAll(itemResult => {
                if (itemResult.IsSuccess)
                {
                    // save credential
                    var autoLogin = AuthData.AutoLogin;
                    if (autoLogin)
                    {
                        var credential = result.Credential;
                        Credential.Save(credential);
                    }
                    OnLoginEvent?.Invoke(result);
                    successAction?.Invoke(result);
                }
                else
                {
                    var failedResult = new CBSLoginResult
                    {
                        IsSuccess = false,
                        Error = itemResult.Error
                    };
                    successAction?.Invoke(failedResult);
                }
            });
        }
    }

    // login
    public struct CBSLoginResult
    {
        public bool IsSuccess;
        public bool IsNew;
        public string PlayerId;
        public LoginResult Result;
        public SimpleError Error;
        public BaseCredential Credential;
    }

    public struct CBSLogoutResult
    {
        public bool IsSuccess;
        public string PlayerId;
        public SimpleError Error;
    }

    // mail registration
    public struct CBSMailRegistrationResult
    {
        public bool IsSuccess;
        public string PlayerId;
        public SimpleError Error;
    }

    public struct CBSMailRegistrationRequest
    {
        public string Mail;
        public string Password;
        public string DisplayName;
    }

    // mail login 
    public struct CBSMailLoginRequest
    {
        public string Mail;
        public string Password;
    }

    // password recovery
    public struct CBSSendRecoveryResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }

    // registration prize
    public struct CBSRegistrationPrizeResult
    {
        public bool IsSuccess;
        public SimpleError Error;
    }


    public enum AuthType
    {
        NONE,
        CUSTOM_ID,
        FACEBOOK,
        GOOGLE,
        STEAM,
        XBOX,
        PS
    }

    public enum DeviceIdDataProvider
    {
        SYSTEM_UNIQUE_ID,
        PLAYFAB_DEVICE_ID
    }
}
