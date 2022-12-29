using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public interface IMatchmakingFactory
    {
        GameObject SpawnMatchmakingResult(CBSMatchmakingQueue queue);
    }
}
