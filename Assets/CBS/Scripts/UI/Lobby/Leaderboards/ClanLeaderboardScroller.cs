using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanLeaderboardScroller : PreloadScroller<ClanLeaderboardEntry>
    {
        protected override float DeltaToPreload => 0.75f;
    }
}
