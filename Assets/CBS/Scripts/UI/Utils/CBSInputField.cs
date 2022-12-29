using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI.Utils
{
    [RequireComponent(typeof(InputField))]
    public class CBSInputField : MonoBehaviour
    {
        private InputField InputField { get; set; }

        private void Start()
        {
            InputField = GetComponent<InputField>();

            InputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string txt)
        {

        }
    }
}
