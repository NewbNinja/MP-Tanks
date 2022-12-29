using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Utils
{
    public static class TransformUtils
    {
        public static Transform Clear(this Transform transform)
        {
            for (var i = transform.childCount; i-- > 0;)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
            return transform;
        }
    }
}