/*  This file is part of the "Simple IAP System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIS
{
    /// <summary>
    /// Preview window for displaying child rewards of a bundled product containing several others.
    /// </summary>
    public class UIWindowPreview : MonoBehaviour
    {
        /// <summary>
        /// The target container where the preview products should be instantiated in.
        /// </summary>
        public Transform container;

        /// <summary>
        /// The prefab used for displaying a product in the preview list.
        /// </summary>
        public GameObject itemPrefab;

        /// <summary>
        /// Whether or not product previews should be instantiated recursively.
        /// This could be enabled if a preview product contains other products itself.
        /// </summary>
        public bool includeChildRewards = false;


        /// <summary>
        /// Initialize window.
        /// </summary>
        public void Set(List<KeyValuePairStringInt> products)
        {
            for (int i = 0; i < products.Count; i++)
            {
                //instantiate item in the scene and attach it to the defined parent transform
                GameObject newItem = Instantiate(itemPrefab);
                newItem.transform.SetParent(container, false);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                //rename item to force ordering as set in the IAP Settings editor
                newItem.name = "BundleItem " + string.Format("{0:000}", (container.childCount - 1) + 1);

                IAPProduct product = IAPManager.GetIAPProduct(products[i].Key);
                IAPCurrency currency = IAPManager.GetInstance().asset.currencyList.Find(x => x.ID == products[i].Key);

                if (product != null && currency != null)
                {
                    //we are not sure if this is a product or a currency, do not handle it as a product
                    product = null;
                }

                Image image = newItem.GetComponentInChildren<Image>();
                if (image != null)
                {
                    if (product != null) image.sprite = product.icon;
                    else if (currency != null) image.sprite = currency.icon;
                    else image.enabled = false;
                }

                Text txt = newItem.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    if (product != null) txt.text = products[i].Value + " " + product.title + "\n" + product.description;
                    else txt.text = products[i].Value + " " + products[i].Key;
                }

                if(includeChildRewards && product != null)
                {
                    Set(IAPManager.GetProductRewards(product.ID));
                }
            }

            gameObject.SetActive(true);
        }


        //reset window to original state
        void OnDisable()
        {
            int count = container.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}
