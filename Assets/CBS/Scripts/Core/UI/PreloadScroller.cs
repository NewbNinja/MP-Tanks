using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CBS.Core
{
    [RequireComponent(typeof (ScrollRect))]
    public abstract class PreloadScroller<T> : MonoBehaviour, IBeginDragHandler, IEndDragHandler where T : class
    {
        [SerializeField]
        private Transform Root;

        [SerializeField]
        protected int PoolCount;

        [SerializeField]
        protected ScrollerSide Direction;

        protected abstract float DeltaToPreload { get; }

        protected List<GameObject> PoolItems { get; set; } = new List<GameObject>();

        protected List<T> List { get; set; }

        private GameObject UIPrefab { get; set; }

        private bool NeedToExtend => List.Count > PoolCount;

        private bool IsInited { get; set; }

        private bool IsDragging { get; set; }

        protected ScrollRect Scroll { get; set; }

        protected HorizontalOrVerticalLayoutGroup Layout { get; set; }

        protected int PoolIndex { get; set; }

        private void OnDestroy()
        {
            if (Scroll != null)
                Scroll.onValueChanged.RemoveAllListeners();
        }

        public List<GameObject> Spawn(GameObject prefab, List<T> list)
        {
            List = list.ToList();
            UIPrefab = prefab;
            PoolIndex = 0;

            Init();
            ResetSroll();

            int spawnCount = NeedToExtend ? PoolCount : List.Count;

            for (int i = 0; i < spawnCount; i++)
            {
                PoolItems[i].SetActive(true);
                var scrollable = PoolItems[i].GetComponent<IScrollableItem<T>>();
                if (scrollable != null)
                    scrollable.Display(List[i]);
            }

            return PoolItems;
        }

        public ScrollRect GetScroll()
        {
            return Scroll;
        }

        protected void PushNew(T item)
        {
            List.Add(item);
            if (NeedToExtend)
            {
                List.RemoveAt(0);
                List.TrimExcess();

                var topItem = PoolItems.First();
                var rectItem = topItem.GetComponent<RectTransform>();
                rectItem.SetAsLastSibling();
                PoolItems.Remove(topItem);
                PoolItems.TrimExcess();
                PoolItems.Add(topItem);

                var scrollable = topItem.GetComponent<IScrollableItem<T>>();
                if (scrollable != null)
                    scrollable.Display(item);
            }
            else
            {
                int pushIndex = List.Count - 1;
                PoolItems[pushIndex].SetActive(true);
                var scrollable = PoolItems[pushIndex].GetComponent<IScrollableItem<T>>();
                if (scrollable != null)
                    scrollable.Display(List[pushIndex]);

            }
        }

        private void Init()
        {
            if (!IsInited)
            {
                Scroll = GetComponent<ScrollRect>();
                Scroll.onValueChanged.AddListener(OnScrollMove);
                Layout = Scroll.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                IsInited = true;
                for (int i = 0;i < PoolCount; i++)
                {
                    var item = Instantiate(UIPrefab, Root);
                    PoolItems.Add(item);
                    item.SetActive(false);
                }
            }
            else
            {
                foreach (var item in PoolItems)
                    item.SetActive(false);
            }
        }

        private void OnScrollMove(Vector2 position)
        {
            if (NeedToExtend && !IsDragging && Direction != ScrollerSide.NONE)
            {
                float hPos = Direction == ScrollerSide.VERTICAL ? Scroll.verticalNormalizedPosition : Scroll.horizontalNormalizedPosition;
                if (Direction == ScrollerSide.VERTICAL)
                {
                    if (hPos <= DeltaToPreload)
                        MoveNext();
                    else if (hPos >= 1 - DeltaToPreload)
                        MovePrev();
                }
                else
                {
                    if (hPos >= DeltaToPreload)
                        MoveNext();
                    else if (hPos <= 1 - DeltaToPreload)
                        MovePrev();
                }
                
            }
        }

        protected void MovePrev()
        {
            int prevPool = PoolIndex;

            if (prevPool > 0 && NeedToExtend)
            {
                PoolIndex--;
                var bottomItem = PoolItems.Last();
                var rectItem = bottomItem.GetComponent<RectTransform>();
                rectItem.SetAsFirstSibling();
                PoolItems.Remove(bottomItem);
                PoolItems.TrimExcess();
                PoolItems.Insert(0, bottomItem);

                float moveRectAmount = rectItem.sizeDelta.y + Layout.spacing;
                var contentRect = Scroll.content.anchoredPosition;
                if (Direction == ScrollerSide.VERTICAL)
                    contentRect.y += (moveRectAmount);
                else if(Direction == ScrollerSide.HORISONTAL)
                    contentRect.x -= (moveRectAmount);
                Scroll.content.anchoredPosition = contentRect;

                var listObject = List[PoolIndex];
                var scrollable = bottomItem.GetComponent<IScrollableItem<T>>();
                if (scrollable != null)
                    scrollable.Display(listObject);
            }
        }

        protected void MoveNext()
        {
            int nextPool = PoolCount + PoolIndex;
            if (nextPool <= List.Count && NeedToExtend)
            {
                PoolIndex++;
                var topItem = PoolItems.First();
                var rectItem = topItem.GetComponent<RectTransform>();
                rectItem.SetAsLastSibling();
                PoolItems.Remove(topItem);
                PoolItems.TrimExcess();
                PoolItems.Add(topItem);

                float moveRectAmount = Direction == ScrollerSide.VERTICAL ? rectItem.sizeDelta.y + Layout.spacing : rectItem.sizeDelta.x + Layout.spacing;
                var contentRect = Scroll.content.anchoredPosition;
                if (Direction == ScrollerSide.VERTICAL)
                    contentRect.y -= (moveRectAmount);
                else if (Direction == ScrollerSide.HORISONTAL)
                    contentRect.x += (moveRectAmount);

                Scroll.content.anchoredPosition = contentRect;

                var listObject = List[PoolIndex];
                var scrollable = topItem.GetComponent<IScrollableItem<T>>();
                if (scrollable != null)
                    scrollable.Display(listObject);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
        }

        protected void ResetSroll()
        {
            PoolIndex = 0;
            IsDragging = false;
            Scroll.content.anchoredPosition = Vector2.zero;
            Scroll.velocity = Vector2.zero;
        }

        public void HideAll()
        {
            foreach (var obj in PoolItems)
                obj.SetActive(false);
        }
    }

    public enum ScrollerSide
    {
        VERTICAL,
        HORISONTAL,
        NONE
    }
}
