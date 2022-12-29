using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.UI
{
    public class RouletteScroller : PreloadScroller<RoulettePosition>
    {
        protected override float DeltaToPreload => 0.99f;

        private ScrollerSide CacheDirection { get; set; }

        private void Start()
        {
            CacheDirection = Direction;
        }

        public void SetPoolCount(int value)
        {
            PoolCount = value;
        }

        public void SetPosition(float positionX)
        {
            GetScroll().content.anchoredPosition = new Vector2(positionX, 0);
        }

        public void SetLocalPosition(float positionX, float t)
        {
            var position = GetScroll().content.localPosition;
            GetScroll().content.localPosition = Vector2.Lerp(position, new Vector2(positionX, 0), t);
        }

        public void ResetPosition()
        {
            PoolIndex = 0;
            ResetSroll();
            OnEndDrag(null);
            Direction = CacheDirection;
        }

        public float GetPositionX()
        {
            return GetScroll().content.anchoredPosition.x;
        }

        public float GetDelta()
        {
            return DeltaToPreload;
        }

        public float GetContentSpace()
        {
            return Layout.spacing;
        }

        public int GetPoolIndex()
        {
            return PoolIndex;
        }

        public GameObject GetLastViewObject()
        {
            return PoolItems.Last();
        }

        public void PrepareFinishAnimation()
        {
            Direction = ScrollerSide.NONE;
        }

        public void PushForward(int num)
        {
            Direction = CacheDirection;
            for (int i=0;i<num;i++)
            {
                MoveNext();
            }
            Direction = ScrollerSide.NONE;
        }
    }
}
