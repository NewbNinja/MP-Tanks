using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace CBS.UI
{
    public class RouletteWindow : MonoBehaviour
    {
        [SerializeField]
        private RouletteScroller Scroller;
        [SerializeField]
        private Button SpinButton;
        [SerializeField]
        private int AnimationCircleCount;
        [SerializeField]
        private float AnimationSpeed;

        private readonly int MinPoolCount = 30;

        private IRoulette Roulette { get; set; }
        private RoulettePrefabs Prefabs { get; set; }

        private List<RoulettePosition> CachePositions { get; set; }
        private List<GameObject> CacheSlots { get; set; }
        private int ItemsCount { get; set; }
        private bool IsAnimationNow { get; set; }


        private void Awake()
        {
            Roulette = CBSModule.Get<CBSRoulette>();
            Prefabs = CBSScriptable.Get<RoulettePrefabs>();
        }

        private void OnEnable()
        {
            Clear();
            GetRouletteData();
        }

        private void Clear()
        {
            Scroller.HideAll();
            SpinButton.interactable = false;
            if (CacheSlots == null)
                return;
            foreach (var slot in CacheSlots)
            {
                slot.GetComponent<RouletteSlot>().ToDefault();
            }
        }

        private void GetRouletteData()
        {
            Roulette.GetRouletteTable(OnRouletteGettted);
        }

        private void OnRouletteGettted(GetRouletteTableResult result)
        {
            SpinButton.interactable = result.IsSuccess;
            if (result.IsSuccess)
            {
                ItemsCount = result.Table.Positions.Count;
                var list = GetPoolList(result.Table.Positions);
                var listCount = list.Count;
                Scroller.SetPoolCount(listCount);
                var prefab = Prefabs.RouletteSlot;
                CachePositions = AsignCircleCount(list);
                CacheSlots = Scroller.Spawn(prefab, CachePositions);
            }
        }

        private List<RoulettePosition> GetPoolList(List<RoulettePosition> current)
        {
            var tempList = current.ToList();
            var listCount = tempList.Count;
            float delta = (float)MinPoolCount / (float)listCount;
            if (delta > 1f)
            {
                int poolCount = Mathf.CeilToInt(delta);
                for (int i =0;i<poolCount;i++)
                {
                    tempList = tempList.Concat(current).ToList();
                }
            }

            return tempList;
        }

        private List<RoulettePosition> AsignCircleCount(List<RoulettePosition> current)
        {
            var tempList = current.ToList();
            for (int i = 0; i < AnimationCircleCount; i++)
            {
                tempList = tempList.Concat(current).ToList();
            }
            return tempList;
        }

        // buttons events
        public void OnSpinClick()
        {
            SpinButton.interactable = false;
            Roulette.Spin(onSpin => {
                SpinButton.interactable = !onSpin.IsSuccess;

                if (onSpin.IsSuccess)
                {
                    var droppedItem = onSpin.Position;
                    var itemID = droppedItem.ID;
                    Scroller.ResetPosition();
                    Scroller.HideAll();
                    var prefab = Prefabs.RouletteSlot;
                    CacheSlots = Scroller.Spawn(prefab, CachePositions);
                    Scroller.Spawn(prefab, CachePositions);
                    StartCoroutine(OnSpinAnimation(itemID));
                }
            });
        }

        public void OnCloseWindow()
        {
            if (IsAnimationNow)
                return;
            gameObject.SetActive(false);
        }

        // animation
        private IEnumerator OnSpinAnimation(string ID)
        {
            IsAnimationNow = true;
            var firstIndex = -1;
            for (int i = 0; i < CachePositions.Count; i++)
            {
                if (CachePositions[i].ID == ID)
                {
                    if (firstIndex == -1)
                        firstIndex = i;
                }
            }
            yield return null;
            var itemIndex = ItemsCount * AnimationCircleCount + firstIndex;
            while(ItemsCount + Scroller.GetPoolIndex() < ItemsCount * AnimationCircleCount + firstIndex)
            {
                yield return null;
                Scroller.SetPosition(Scroller.GetPositionX() - Time.deltaTime * AnimationSpeed);
            }
            // get last view object
            var lastObject = Scroller.GetLastViewObject();
            var lastRect = lastObject.GetComponent<RectTransform>();
            var scroll = Scroller.GetScroll();
            var scrollPosition = scroll.content.anchoredPosition;

            var delay = Mathf.Abs(lastRect.anchoredPosition.x + scrollPosition.x)/2f + Scroller.GetContentSpace()*2;
            Scroller.PushForward(4);
            var finalPosition = Scroller.GetPositionX() - delay;
            Scroller.PrepareFinishAnimation();
            var actualPosition = Scroller.GetPositionX();
            var currentPosition = actualPosition;
            
            while (Mathf.Abs(currentPosition - finalPosition) > 10)
            {
                yield return null;
                currentPosition = Mathf.Lerp(currentPosition, finalPosition, Time.deltaTime * AnimationSpeed * 0.001f);
                Scroller.SetPosition(actualPosition + currentPosition - actualPosition);
            }
            lastObject.GetComponent<RouletteSlot>().Activate();
            SpinButton.interactable = true;
            IsAnimationNow = false;
        }
    }
}