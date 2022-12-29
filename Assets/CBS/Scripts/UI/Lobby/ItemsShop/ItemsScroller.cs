using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ItemsScroller : PreloadScroller<CBSBaseItem>
    {
        protected override float DeltaToPreload => 0.3f;
    }
}
