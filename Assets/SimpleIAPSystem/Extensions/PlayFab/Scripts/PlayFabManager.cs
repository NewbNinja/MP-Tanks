/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SIS.SimpleJSON;

using UnityEngine.Purchasing;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;

#if PLAYFAB_STEAM
using Steamworks;
#endif

#pragma warning disable 0162, 0414
namespace SIS
{
    /// <summary>
    /// Manager integrating PlayFab's ClientModels for handling all related web requests, such as
    /// logging in and syncing Simple IAP System storage (purchases, player data, currency) with PlayFab.
    /// </summary>
    [RequireComponent(typeof(PlayFabReceiptValidator))]
    public class PlayFabManager : MonoBehaviour
    {
        /// <summary>
        /// Static reference to this script.
        /// </summary>
        private static PlayFabManager instance;

        /// <summary>
        /// PlayFab account id of the user logged in.
        /// </summary>
        public static string userId;

        /// <summary>
        /// Should a user account only be created at the time of purchase?
        /// Toggle on if you otherwise do not make use of PlayFab API and user inventory.
        /// </summary>
        public bool validationOnly = false;

        #if PLAYFAB_STEAM
        protected Callback<GetAuthSessionTicketResponse_t> authTicketResponse;
        private byte[] ticket;
        private HAuthTicket authTicket;
        private uint ticketLength;
        private string hexTicket = "";
        #endif

        /// <summary>
        /// Fired when the user successfully logged in to a PlayFab account.
        /// </summary>
        public static event Action<PlayFab.ClientModels.LoginResult> loginSucceededEvent;

        /// <summary>
        /// Fired when logging in fails due to authentication or other issues.
        /// </summary>
        public static event Action<string> loginFailedEvent;

        private PlayFab.ClientModels.LoginResult loginResult;
        private Dictionary<string, string> itemInstanceIDs;
        private JSONNode serverData;
        private GetPlayerCombinedInfoRequestParams accountParams;
        private bool isRestoreActive = false;


        /// <summary>
        /// Returns a static reference to this script.
        /// </summary>
        public static PlayFabManager GetInstance()
        {
            return instance;
        }


        //setting up parameters and callbacks
        void Awake()
        {
            //make sure we keep one instance of this script in the game
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(this);

            //set static reference
            instance = this;
            
            itemInstanceIDs = new Dictionary<string, string>();
            accountParams = new GetPlayerCombinedInfoRequestParams()
            {
                GetUserInventory = !validationOnly,
                GetUserVirtualCurrency = !validationOnly,
                GetUserData = !validationOnly,
                UserDataKeys = new List<string>() { DBManager.playerKey, DBManager.selectedKey }
            };

            if (!IsValidationOnly())
            {
                IAPManager.remoteInitializeReadyEvent += ReadyToInitialize;
                IAPManager.remoteShouldAddProductFunc += ShouldAddProductFromRemote;
                IAPManager.remoteFetchProductsEvent += FetchProductsFromRemote;
                IAPManager.remotePurchaseVirtualEvent += PurchaseVirtualFromRemote;
                IAPManager.remoteConsumePurchaseEvent += ConsumePurchaseFromRemote;
            }

            IAPManager.restoreTransactionsStartedEvent += OnRestoreTransactionsStarted;
            IAPManager.restoreTransactionsFinishedEvent += OnRestoreTransactionsFinished;

            //subscribe to selected events for handling them automatically
            DBManager.itemSelectedEvent += x => SetSelected();
            DBManager.itemDeselectedEvent += x => SetSelected();
        }


        //try to do device login
        void Start()
        {
            //no auto-login when only validation is enabled
            if (IsValidationOnly()) return;

            #if PLAYFAB_STEAM
                authTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);
                ticket = new byte[1024];
                authTicket = SteamUser.GetAuthSessionTicket(ticket, 1024, out ticketLength);
            #else
                //no auto-login by default, use UserLogin scene!
                //LoginWithDevice();
            #endif
        }


        void ReadyToInitialize()
        {
            IAPManager.GetInstance().GetComponent<DBManager>().memoryOnly = true;
            if (IAPManager.isDebug)
                Debug.Log("PlayFab (online mode) is enabled: IAP data will not be saved on devices.");
        }


        bool ShouldAddProductFromRemote(IAPProduct product)
        {
            //if it's an IAP for real money, add it to the id list
            //on PlayFab we also add virtual products for cloud save, except in validation mode
            if (!product.IsVirtual()) return true;

            //removing free virtual products because PlayFab does not keep them in their store
            if (!product.priceList.Exists(x => x.amount > 0))
                return false;

            return true;
        }


        void FetchProductsFromRemote()
        {
            //manually retrieve catalog items for remote overwrites,
            //when using PlayFab (full) on a platform that does not utilize the PlayFabStore class
            #if !PLAYFAB_PAYPAL
            PlayFabStore.instance.RetrieveProducts(null);
            #endif
        }


        void PurchaseVirtualFromRemote(IAPProduct product)
        {
            PlayFabStore.instance.Purchase(product);
        }


        void ConsumePurchaseFromRemote(IAPProduct product, int amount)
        {
            if(!itemInstanceIDs.ContainsKey(product.ID))
            {
                IAPManager.OnConsumeFailed("Product not in inventory.");
                return;
            }

            PlayFabStore.instance.Consume(product, itemInstanceIDs[product.ID], amount);
        }


        void OnRestoreTransactionsStarted()
        {
            isRestoreActive = true;
        }


        void OnRestoreTransactionsFinished(bool success)
        {
            isRestoreActive = false;
            if (!success || IsValidationOnly()) return;

            List<string> restoreProducts = DBManager.GetAllPurchased(true);
            restoreProducts.RemoveAll(p => IAPManager.controller.products.WithID(p) == null);
            GetInstance().StartCoroutine(SetPurchase(restoreProducts));
        }


        /// <summary>
        /// Return whether a restore workflow is currently active. This is used on receipt validation
        /// as there is currently a Unity IAP bug that causes PlayFab to decline any restore transactions.
        /// So if a restore workflow is active, we tell the validation to not validate the receipts again.
        /// </summary>
        public bool IsRestoreActive()
        {
            return this.isRestoreActive;
        }


        /// <summary>
        /// Create a request on the underlying store instance to redeem a coupon code.
        /// </summary>
        public static void RedeemCoupon(string code)
        {
            PlayFabStore.instance.RedeemCoupon(code);
        }


        /// <summary>
        /// Return raw LoginResult data from PlayFab.
        /// Can be null if not logged in yet.
        /// </summary>
        /// <returns></returns>
        public PlayFab.ClientModels.LoginResult GetLoginResult()
        {
            return this.loginResult;
        }


        /// <summary>
        /// Grant non-consumables to the user's inventory on PlayFab, for free.
        /// As this is a security risk you should have some server validations in place.
        /// Consumables will be granted only locally to minimize requests - please call SetCurrency or SetPlayerData afterwards if needed.
        /// </summary>
        public static IEnumerator SetPurchase(List<string> productIDs)
        {
            //create a separate list for non-consumables to request
            List<string> cloudProducts = new List<string>();
            for(int i = 0; i < productIDs.Count; i++)
            {
                if((int)IAPManager.GetIAPProduct(productIDs[i]).type > 0)
                    cloudProducts.Add(productIDs[i]);
            }            

            bool commit = true;
            if(cloudProducts.Count > 0)
            {
                ExecuteFunctionRequest cloudRequest = new ExecuteFunctionRequest()
                {
                    FunctionName = "grantItems",
                    FunctionParameter = cloudProducts,
                     GeneratePlayStreamEvent = true
                };

                bool result = false;
                PlayFabCloudScriptAPI.ExecuteFunction(cloudRequest, (cloudResult) =>
                {
                    result = true; 
                }, (error) =>
                {
                        OnPlayFabError(error);
                        commit = false;
                        result = true;
                });
    
                while(!result)
                {
                    yield return null;
                }
            }

            //only grant products if the cloud request was successful
            if(commit == true)
            {
                for(int i = 0; i < productIDs.Count; i++)
                {
                    if(DBManager.GetPurchase(productIDs[i]) == 0)
					    IAPManager.GetInstance().CompletePurchase(productIDs[i]);
                }
            }

            yield return null;
        }


        /// <summary>
        /// Uploads local player data to PlayFab. Call this after manipulating player data manually, e.g. via DBManager.IncreasePlayerData.
        /// Note that this method is called automatically for syncing consumable usage counts on product purchases.
        /// </summary>
        public static void SetPlayerData()
        {
            if (IsValidationOnly()) return;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(DBManager.playerKey, DBManager.GetJSON(DBManager.playerKey));

            UpdateUserDataRequest request = new UpdateUserDataRequest()
            {
                Data = dic
            };

            PlayFabClientAPI.UpdateUserData(request, null, OnPlayFabError);
        }


        /// <summary>
        /// Uploads local currency balance to PlayFab. Call this after giving currency manually, e.g. via DBManager.IncreaseFunds.
        /// Note that the virtual currency balance is synced automatically with PlayFab on product purchases.
        /// </summary>
        public static void SetFunds()
        {
            if (IsValidationOnly()) return;

            Dictionary<string, int> dic = DBManager.GetCurrencies();

            PlayFabCloudScriptAPI.ExecuteFunction(new ExecuteFunctionRequest()
            {
                FunctionName = "addCurrency",
                FunctionParameter = dic
            }, null, OnPlayFabError);
        }      


        /// <summary>
        /// Uploads local item selection states to PlayFab. Call this after manual selections, e.g. via DBManager.SetSelected.
        /// Note that selection states are synced automatically when selecting or deseleting items in the shop.
        /// </summary>
        public static void SetSelected()
        {
            if (IsValidationOnly()) return;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(DBManager.selectedKey, DBManager.GetJSON(DBManager.selectedKey));

            UpdateUserDataRequest request = new UpdateUserDataRequest()
            {
                Data = dic
            };

            PlayFabClientAPI.UpdateUserData(request, null, null);
        }


        /// <summary>
        /// Logs in with the user device id, using the correct PlayFab method per platform.
        /// A new account will be created when no account is associated with the device id.
        /// </summary>
        public void LoginWithDevice(Action<bool> resultCallback = null)
        {
            #if UNITY_ANDROID
            LoginWithAndroidDeviceIDRequest request = new LoginWithAndroidDeviceIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                AndroidDeviceId = DBManager.GetDeviceId(),
                CreateAccount = true,
                OS = SystemInfo.operatingSystem,
                AndroidDevice = SystemInfo.deviceModel,
                InfoRequestParameters = accountParams
            };

            PlayFabClientAPI.LoginWithAndroidDeviceID(request, (result) =>
            {
                OnLoggedIn(result);
                if(resultCallback != null) resultCallback(true);
            }, (error) =>
            {
                OnPlayFabError(error);
                if(resultCallback != null) resultCallback(false);
            });
            
            #elif UNITY_IOS
            LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                DeviceId = DBManager.GetDeviceId(),
                CreateAccount = true,
                OS = SystemInfo.operatingSystem,
                DeviceModel = SystemInfo.deviceModel,
                InfoRequestParameters = accountParams
            };

            PlayFabClientAPI.LoginWithIOSDeviceID(request, (result) =>
            {
                OnLoggedIn(result);
                if(resultCallback != null) resultCallback(true);
            }, (error) =>
            {
                OnPlayFabError(error);
                if(resultCallback != null) resultCallback(false);
            });

            #else
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CreateAccount = true,
                CustomId = DBManager.GetDeviceId(),
                InfoRequestParameters = accountParams
            };

            PlayFabClientAPI.LoginWithCustomID(request, (result) =>
            {
                OnLoggedIn(result);
                if(resultCallback != null) resultCallback(true);
            }, (error) =>
            {
                OnPlayFabError(error);
                if(resultCallback != null) resultCallback(false);
            });
            #endif
        }


        #if PLAYFAB_STEAM
        private void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
        {
            byte[] tempTicket = new byte[ticketLength];
            for (int i = 0; i < tempTicket.Length; i++)
                tempTicket[i] = ticket[i];

            string hexEncodedTicket = "";
            hexEncodedTicket = System.BitConverter.ToString(tempTicket);
            hexEncodedTicket = hexEncodedTicket.Replace("-", "");
            hexTicket = hexEncodedTicket;

            LoginWithSteam();
        }
        #endif


        /// <summary>
        /// Register a new account by using the email address and password provided.
        /// </summary>
        public static void RegisterAccount(string emailAddress, string password)
        {
            RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                Email = emailAddress,
                Password = password,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisteredResult, OnLoginError);
        }


        /// <summary>
        /// Login via email by using the email address and password provided.
        /// </summary>
        public static void LoginWithEmail(string emailAddress, string password)
        {
            LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                Email = emailAddress,
                Password = password,
                InfoRequestParameters = instance.accountParams
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoggedIn, OnLoginError);
        }


        /// <summary>
        /// Request an account recovery email (new password) to the email passed in.
        /// </summary>
        public static void ForgotPassword(string emailAddress)
        {
            SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                Email = emailAddress
            };

            PlayFabClientAPI.SendAccountRecoveryEmail(request, OnEmailRecovery, OnLoginError);
        }


        private static void OnEmailRecovery(SendAccountRecoveryEmailResult result)
        {
            if (loginFailedEvent != null)
                loginFailedEvent("Recovery Email sent. Please check your inbox.");
        }


        #if PLAYFAB_STEAM
        /// <summary>
        /// Login via email by using the Steam authentication token received by Steamworks.NET
        /// </summary>
        public void LoginWithSteam()
        {
            LoginWithSteamRequest request = new LoginWithSteamRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CreateAccount = true,
                SteamTicket = hexTicket,
                InfoRequestParameters = accountParams
            };

            PlayFabClientAPI.LoginWithSteam(request, OnLoggedIn, OnLoginError);
        }
        #endif

        
        /// <summary>
        /// Process the login result and all user data retrieved from PlayFab, such as
        /// user inventory with purchased products, virtual currency balances and title data
        /// </summary>
        public static void OnLoggedIn(PlayFab.ClientModels.LoginResult result)
        {
            instance.loginResult = result;

            userId = result.PlayFabId;
            bool skipPayload = result.InfoResultPayload == null ? true : false;

            if(IAPManager.isDebug)
                Debug.Log("Got PlayFabID: " + userId + (result.NewlyCreated ? " (new account)" : " (existing account)"));

            if (!IsValidationOnly() && !skipPayload)
            {
                ConvertInventory(result.InfoResultPayload);
                ConvertSubscriptions(result.InfoResultPayload.UserInventory);
            }

            IAPManager.GetInstance().Initialize();

            if (loginSucceededEvent != null)
                loginSucceededEvent(result);
        }


        private static void ConvertInventory(GetPlayerCombinedInfoResultPayload result)
        {
            instance.serverData = new JSONClass();

            List<ItemInstance> inventory = result.UserInventory;
            if (inventory != null && inventory.Count > 0)
            {
                string itemId = null;
                for (int i = 0; i < inventory.Count; i++)
                {
                    itemId = inventory[i].ItemId;
                    IAPProduct product = IAPManager.GetIAPProduct(itemId);

                    if (product != null)
                    {
                        instance.AddItemInstanceID(product.ID, inventory[i].ItemInstanceId);
                        instance.serverData[DBManager.contentKey][product.ID] = new JSONData(inventory[i].RemainingUses != null ? (int)inventory[i].RemainingUses : 1);
                    }
                }
            }

            Dictionary<string, int> virtualCurrency = result.UserVirtualCurrency;
            if (virtualCurrency != null && virtualCurrency.Count > 0)
            {
                Dictionary<string, int> currency = DBManager.GetCurrencies();
                foreach (KeyValuePair<string, int> pair in virtualCurrency)
                {
                    //update local data in memory
                    foreach (KeyValuePair<string, int> cur in currency)
                    {
                        if (cur.Key.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            instance.serverData[DBManager.currencyKey][cur.Key].AsInt = pair.Value;
                            break;
                        }
                    }
                }
            }

            Dictionary<string, UserDataRecord> userData = result.UserData;
            if (userData != null && userData.Count > 0)
            {
                string[] userKey = instance.accountParams.UserDataKeys.ToArray();
                for (int i = 0; i < userKey.Length; i++)
                {
                    if (userData.ContainsKey(userKey[i]) && !string.IsNullOrEmpty(userData[userKey[i]].Value))
                    {
                        JSONNode node = JSON.Parse(userData[userKey[i]].Value);
                        foreach (string groupKey in node.AsObject.Keys)
                        {
                            JSONArray array = node[groupKey].AsArray;

                            //it's a simple value, not an array (usually Player data)
                            if (array == null)
                            {
                                instance.serverData[userKey[i]][groupKey] = node[groupKey].Value;
                                continue;
                            }

                            //it's an array of values (usually Selected group data)
                            for (int j = 0; j < array.Count; j++)
                            {
                                instance.serverData[userKey[i]][groupKey][j] = array[j].Value;
                            }
                        }
                    }
                }
            }

            DBManager.Overwrite(instance.serverData.ToString());
        }


        private static void ConvertSubscriptions(List<ItemInstance> items)
        {
            if (items == null || items.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if(items[i].Expiration == null)
                    continue;

                IAPProduct product = IAPManager.GetIAPProduct(items[i].ItemId);

                if (product != null)
                {
                    DateTime date = items[i].Expiration.Value;
                    string dateText = date.ToString("u");
                    product.customData.Remove("expiration");
                    product.customData.Add("expiration", dateText);
                }
            }
        }


        /// <summary>
        /// Theoretically there should only be one ItemInstanceID per item at any time.
        /// </summary>
        public void AddItemInstanceID(string productID, string instanceID)
        {
            if (itemInstanceIDs.ContainsKey(productID)) itemInstanceIDs[productID] = instanceID;
            else itemInstanceIDs.Add(productID, instanceID);
        }


        //called on successful registration of a new account, directly log in with it
        private static void OnRegisteredResult(RegisterPlayFabUserResult result)
        {
            PlayFab.ClientModels.LoginResult loginResult = new PlayFab.ClientModels.LoginResult();
            loginResult.PlayFabId = result.PlayFabId;
            loginResult.NewlyCreated = true;
            OnLoggedIn(loginResult);
        }


        //called after receiving an error when trying to log in
        private static void OnLoginError(PlayFabError error)
        {
            string errorText = error.ErrorMessage;

            if (error.ErrorDetails != null && error.ErrorDetails.Count > 0)
            {
                foreach (string key in error.ErrorDetails.Keys)
                {
                    errorText += "\n" + error.ErrorDetails[key][0];
                }
            }

            if (loginFailedEvent != null)
                loginFailedEvent(errorText);
        }


        //called after receiving an error for any request
        //(except login methods since they have their own error callback)
        private static void OnPlayFabError(PlayFabError error)
        {
            if (!IAPManager.isDebug) return;

            Debug.Log("Error: " + (int)error.Error + ", " + error.ErrorMessage);
            if (error.ErrorDetails == null || error.ErrorDetails.Count == 0) return;

            foreach (string key in error.ErrorDetails.Keys)
            {
                Debug.Log(key + ": " + error.ErrorDetails[key][0]);
            }
        }


        /// <summary>
        /// Convenience method to return validationOnly flag.
        /// </summary>
        public static bool IsValidationOnly()
        {
            return instance.validationOnly;
        }


        /// <summary>
        /// Logs out the current user by clearing userId and owned inventory.
        /// </summary>
        public static void Logout()
        {
            userId = string.Empty;
            GetInstance().itemInstanceIDs.Clear();
        }
    }
}