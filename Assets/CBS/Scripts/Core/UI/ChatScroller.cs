using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.Core
{
    public class ChatScroller : PreloadScroller<MessageBody>
    {
        protected override float DeltaToPreload => 0.7f;

        public void PushNewMessage (MessageBody message)
        {
            PushNew(message);
            SetScrollPosition(0);
        }

        public void SetScrollPosition(float position)
        {
            StartCoroutine(ApplyScrollPosition(Scroll, 0));
        }

        public void SetPoolCount(int count)
        {
            PoolCount = count;
        }

        IEnumerator ApplyScrollPosition(ScrollRect sr, float verticalPos)
        {
            yield return new WaitForEndOfFrame();
            sr.verticalNormalizedPosition = verticalPos;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform);
        }
    }
}
