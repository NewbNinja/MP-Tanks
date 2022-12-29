using CBS.Scriptable;
using CBS.Utils;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class PopupViewer
    {
        public void ShowSimplePopup(PopupRequest request)
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var popupPrefab = uiData.SimplePopup;
            var popupObject = UIView.ShowWindow(popupPrefab);
            popupObject.GetComponent<SimplePopup>().Setup(request);
        }

        public void ShowYesNoPopup(YesNoPopupRequest request)
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var popupPrefab = uiData.YesNoPopup;
            var popupObject = UIView.ShowWindow(popupPrefab);
            popupObject.GetComponent<YesNoPopup>().Setup(request);
        }

        public void ShowFabError(SimpleError error)
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var popupPrefab = uiData.SimplePopup;
            var popupObject = UIView.ShowWindow(popupPrefab);

            var request = new PopupRequest {
                Title = AuthTXTHandler.ErrorTitle,
                Body = error.Message
            };

            popupObject.GetComponent<SimplePopup>().Setup(request);
        }

        public void ShowStackError(SimpleError error)
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var popupPrefab = uiData.SimplePopup;
            var popupObject = UIView.ShowWindow(popupPrefab);

            var request = new PopupRequest
            {
                Title = AuthTXTHandler.ErrorTitle,
                Body = error.Stack
            };

            popupObject.GetComponent<SimplePopup>().Setup(request);

            Debug.LogError(error.Stack);
        }

        public void ShowUserInfo(string userID)
        {
            if (string.IsNullOrEmpty(userID))
                return;
            var profile = CBSModule.Get<CBSProfile>();
            string profileID = profile.PlayerID;
            if (profileID == userID)
                return;

            profile.GetPlayerProfile(new CBSGetProfileRequest {
                ProfileID = userID,
                LoadClan = true,
                LoadLevel = true,
                LoadEntityId = true
            }, onGet => {
                if (onGet.IsSuccess)
                {
                    var uiData = CBSScriptable.Get<PopupPrefabs>();
                    var formPrefab = uiData.UserInfoForm;
                    var formObject = UIView.ShowWindow(formPrefab);
                    var formUI = formObject.GetComponent<UserInfoForm>();
                    formUI.Display(onGet);
                }
                else
                {
                    Debug.Log(onGet.Error.Message);
                }
            });
        }

        public void ShowClanInfo(string clanID)
        {
            if (string.IsNullOrEmpty(clanID))
                return;
            var clan = CBSModule.Get<CBSClan>();

            clan.GetClanInfo(clanID, onGet => {
                if (onGet.IsSuccess)
                {
                    var uiData = CBSScriptable.Get<PopupPrefabs>();
                    var formPrefab = uiData.ClanInfoForm;
                    var formObject = UIView.ShowWindow(formPrefab);
                    var formUI = formObject.GetComponent<ClanInfoForm>();
                    formUI.Display(onGet);
                }
            });
        }

        public void ShowLoadingPopup()
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var loadingPrefab = uiData.LoadingPopup;
            UIView.ShowWindow(loadingPrefab);
        }

        public void HideLoadingPopup()
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var loadingPrefab = uiData.LoadingPopup;
            UIView.HideWindow(loadingPrefab);
        }

        public void ShowRewardPopup(PrizeObject prize)
        {
            var uiData = CBSScriptable.Get<PopupPrefabs>();
            var rewardPrefab = uiData.RewardPopup;
            var popupObject = UIView.ShowWindow(rewardPrefab);
            popupObject.GetComponent<PrizeDrawer>().Display(prize);
        }
    }
}
