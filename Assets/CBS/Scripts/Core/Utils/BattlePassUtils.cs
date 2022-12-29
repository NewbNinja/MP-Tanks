using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public static class BattlePassUtils
    {
        public static string GetFrameTimeLabel(BattlePassUserInfo info)
        {
            if (info.IsActive)
            {
                var timeToEnd = info.MilisecondsToEnd;
                var timeSpan = TimeSpan.FromMilliseconds(timeToEnd);
                return "End in " + timeSpan.ToReadableString();
            }
            else
            {
                if (info.MilisecondsToStart > 0)
                {
                    var timeToStart = info.MilisecondsToStart;
                    var timeSpan = TimeSpan.FromMilliseconds(timeToStart);
                    return "Start in " + timeSpan.ToReadableString();
                }
            }
            return string.Empty;
        }
    }
}
