using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanRequestedUserScroller : PreloadScroller<ClanRequestUser>
    {
        protected override float DeltaToPreload => 0.7f;
    }
}
