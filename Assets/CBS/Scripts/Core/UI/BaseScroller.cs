using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Core
{
    public class BaseScroller : MonoBehaviour
    {
        [SerializeField]
        private Transform Root;

        public event Action<GameObject, int> OnSpawn;

        private List<GameObject> AllItems = new List<GameObject>();

        protected GameObject ItemPrefab { private get; set; }

        public void SpawnItems(GameObject prefab, int count)
        {
            ItemPrefab = prefab;
            Clear();
            Resize(count);
        }

        private void Resize(int newCount)
        {
            int currentCount = AllItems.Count;
            bool needResize = newCount > currentCount;
            if (needResize)
            {
                for (int j = 0; j < currentCount; j++)
                {
                    var item = AllItems[j];
                    item.SetActive(true);
                    OnItemSpawn(item, j);
                }
                int resizeCount = needResize ? newCount - currentCount : 0;
                for (int i = 0; i < resizeCount; i++)
                {
                    GameObject item = Instantiate(ItemPrefab, Root);
                    AllItems.Add(item);
                    OnItemSpawn(item, currentCount + i);
                }
            }
            else
            {
                for (int j = 0; j < newCount; j++)
                {
                    var item = AllItems[j];
                    item.SetActive(true);
                    OnItemSpawn(item, j);
                }
            }
        }

        public void Clear()
        {
            foreach (var item in AllItems)
                item.SetActive(false);
        }

        protected virtual void OnItemSpawn(GameObject item, int index) 
        {
            OnSpawn?.Invoke(item, index);
        }
    }
}
