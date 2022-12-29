using CBS.Scriptable;
using CBS.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class CreateClanForm : MonoBehaviour
    {
        [SerializeField]
        private InputField NameInput;
        [SerializeField]
        private InputField DescriptionInput;
        [SerializeField]
        private InputField URLInput;

        private Action BackAction { get; set; }

        private IClan CBSClan { get; set; }
        private ClanPrefabs Prefabs { get; set; }

        private void Awake()
        {
            CBSClan = CBSModule.Get<CBSClan>();
            Prefabs = CBSScriptable.Get<ClanPrefabs>();
        }

        public void SetBackAction(Action back)
        {
            BackAction = back;
        }

        private bool ValidInputs()
        {
            bool validName = !string.IsNullOrEmpty(NameInput.text);
            bool validDescription = !string.IsNullOrEmpty(DescriptionInput.text);

            bool fieldsValid = validName & validDescription;
            if (!fieldsValid)
            {
                new PopupViewer().ShowSimplePopup(new PopupRequest
                {
                    Title = ClanTXTHandler.ErrorTitle,
                    Body = ClanTXTHandler.InvalidInput
                });
                return false;
            }
            return true;
        }

        // button click
        public void CreateClan()
        {
            if (!ValidInputs())
                return;
            string clanName = NameInput.text;
            string clanDescription = DescriptionInput.text;
            string clanImageURL = URLInput.text;

            var createResult = new CreateClanRequest { 
                ClanName = clanName,
                ClanDescription = clanDescription,
                ClanImageURL = clanImageURL
            };

            CBSClan.CreateClan(createResult, onCreate => { 
                if (onCreate.IsSuccess)
                {
                    string clanID = onCreate.ClanID;
                    new PopupViewer().ShowSimplePopup(new PopupRequest
                    {
                        Title = ClanTXTHandler.SuccessTitle,
                        Body = ClanTXTHandler.ClanCreated,
                        OnOkAction = () => {
                            OnClanCreated(clanID);
                        }
                    });
                }
                else
                {
                    if (onCreate.Error.ErrorCode == PlayFab.PlayFabErrorCode.CloudScriptAPIRequestError)
                    {
                        new PopupViewer().ShowSimplePopup(new PopupRequest
                        {
                            Title = ClanTXTHandler.ErrorTitle,
                            Body = ClanTXTHandler.InvalidInput
                        });
                    }
                    else
                    {
                        new PopupViewer().ShowSimplePopup(new PopupRequest
                        {
                            Title = ClanTXTHandler.ErrorTitle,
                            Body = onCreate.Error.Message
                        });
                    }
                }
            });
        }

        private void OnClanCreated(string clanID)
        {
            var noClanPrefab = Prefabs.NoClanWindow;
            var clanPrefab = Prefabs.ClanWindow;

            var clanWindow = UIView.ShowWindow(clanPrefab);
            clanWindow.GetComponent<ClanWindow>().DisplayClan(clanID);

            UIView.HideWindow(noClanPrefab);
        }

        public void ReturnBack()
        {
            BackAction?.Invoke();
        }
    }
}
