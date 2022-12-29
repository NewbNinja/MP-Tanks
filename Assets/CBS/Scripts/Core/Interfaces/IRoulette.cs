using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS
{
    public interface IRoulette
    {
        /// <summary>
        /// Get list of all roulette positions
        /// </summary>
        /// <param name="result"></param>
        void GetRouletteTable(Action<GetRouletteTableResult> result);

        /// <summary>
        /// Start spin roulette and get spin result
        /// </summary>
        /// <param name="result"></param>
        void Spin(Action<SpinRouletteResult> result);
    }
}
