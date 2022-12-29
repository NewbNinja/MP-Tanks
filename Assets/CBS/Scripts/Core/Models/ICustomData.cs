using System.Collections;
using System.Collections.Generic;

namespace CBS
{
    public interface ICustomData
    {
        string CustomDataClassName { get; set; }
        string CustomRawData { get; set; }

        T GetCustomData<T>() where T : CBSBattlePassCustomData;
    }
}
