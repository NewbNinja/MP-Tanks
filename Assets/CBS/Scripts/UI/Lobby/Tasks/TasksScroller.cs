using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class TasksScroller : PreloadScroller<CBSTask>
    {
        protected override float DeltaToPreload => 0.7f;
    }
}
