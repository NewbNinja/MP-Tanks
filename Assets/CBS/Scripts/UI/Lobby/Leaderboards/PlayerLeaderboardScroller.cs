using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class PlayerLeaderboardScroller : PreloadScroller<PlayerLeaderboardEntry>
    {
        protected override float DeltaToPreload => 0.75f;
    }
}
