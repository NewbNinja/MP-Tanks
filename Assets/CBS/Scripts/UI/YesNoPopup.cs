using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class YesNoPopup : MonoBehaviour
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Body;

        private Action YesAction { get; set; }
        private Action NoAction { get; set; }

        // setup popup information
        public void Setup(YesNoPopupRequest request)
        {
            Clear();
            Title.text = request.Title;
            Body.text = request.Body;
            YesAction = request.OnYesAction;
            NoAction = request.OnNoAction;
        }

        // reset view
        private void Clear()
        {
            Title.text = string.Empty;
            Body.text = string.Empty;
            YesAction = null;
        }

        // button event
        public void OnYes()
        {
            YesAction?.Invoke();
            gameObject.SetActive(false);
        }

        public void OnNo()
        {
            NoAction?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public struct YesNoPopupRequest
    {
        public string Title;
        public string Body;
        public Action OnYesAction;
        public Action OnNoAction;
    }
}
