using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Core
{
    public interface IScrollableItem<T> where T : class
    {
        void Display(T data);
    }
}
