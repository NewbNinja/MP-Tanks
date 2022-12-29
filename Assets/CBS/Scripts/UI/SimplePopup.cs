using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class SimplePopup : MonoBehaviour
    {
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Body;

        private Action CurrentAction { get; set; }

        // setup popup information
        public void Setup(PopupRequest request)
        {
            Clear();
            Title.text = request.Title;
            Body.text = request.Body;
            CurrentAction = request.OnOkAction;
        }

        // reset view
        private void Clear()
        {
            Title.text = string.Empty;
            Body.text = string.Empty;
            CurrentAction = null;
        }

        // button event
        public void OnOk()
        {
            CurrentAction?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public struct PopupRequest
    {
        public string Title;
        public string Body;
        public Action OnOkAction;
    }
}
