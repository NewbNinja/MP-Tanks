using CBS.Core;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class MessageDialogScroller : PreloadScroller<MessageDialogObject>
    {
        protected override float DeltaToPreload => 0.7f;
    }
}
