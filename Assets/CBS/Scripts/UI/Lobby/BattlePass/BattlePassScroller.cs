using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class BattlePassScroller : PreloadScroller<BattlePassLevelInfo>
    {
        protected override float DeltaToPreload => 0.7f;
    }
}
