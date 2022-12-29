using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Editor
{
    public class ColorUtils
    {
        private static Color[] TeamColors = new Color[] {
            Color.green,
            Color.red,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta
        };

        public static Color GetTeamColor(int index)
        {
            if (index >= TeamColors.Length)
            {
                return TeamColors[Random.Range(1, TeamColors.Length)];
            }
            return TeamColors[index];
        }
    }
}
