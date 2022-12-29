using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    [RequireComponent(typeof (Toggle))]
    public class BaseTab<T> : MonoBehaviour where T : class
    {
        [SerializeField]
        private Text Title;

        private T tabObject;
        public T TabObject {
            get => tabObject;
            set {
                tabObject = value;
                Title.text = tabObject.ToString();
            }
        }

        private Action<T> SelectAction { get; set; }

        private Toggle Toggle { get; set; }

        private void Awake()
        {
            Toggle = GetComponent<Toggle>();
            Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDestroy()
        {
            Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        public void SetSelectAction(Action<T> action)
        {
            SelectAction = action;
        }

        private void OnToggleValueChanged(bool val)
        {
            if (val)
            {
                SelectAction?.Invoke(TabObject);
            }
        }
    }
}
