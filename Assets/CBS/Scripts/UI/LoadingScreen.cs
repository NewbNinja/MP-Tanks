using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private Transform CircleTransform;
        [SerializeField]
        private float AnimationSpeed;

        private void LateUpdate()
        {
            if (CircleTransform == null)
                return;
            CircleTransform.Rotate(new Vector3(0, 0 ,-AnimationSpeed * Time.deltaTime));
        }
    }
}
