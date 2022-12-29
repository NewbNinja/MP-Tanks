using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class DailyBonusScroller : PreloadScroller<DailyBonusInfo>
    {
        protected override float DeltaToPreload => 0.7f;
    }
}
