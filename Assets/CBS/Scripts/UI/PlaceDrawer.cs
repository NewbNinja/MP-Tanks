using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.UI
{
    public class PlaceDrawer : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] Places;
        [SerializeField]
        private Text PlaceText;

        public void Draw(int place)
        {
            Clear();
            if (place < Places.Length)
                Places[place].SetActive(true);
            else
                PlaceText.text = (place + 1).ToString();
        }

        private void Clear()
        {
            foreach (var place in Places)
                place.SetActive(false);
            PlaceText.text = string.Empty;
        }
    }
}
