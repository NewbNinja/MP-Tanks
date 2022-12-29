using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class UIView : MonoBehaviour
    {
        private static readonly string InstanceName = "UIView(Object)";
        public static readonly string CanvasTag = "CanvasMain";
        private static readonly string CanvasPath = "UI/CanvasMain";

        private static UIView instance;
        private static UIView Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(InstanceName).AddComponent<UIView>();
                }
                return instance;
            }
        }

        public static GameObject ShowWindow(GameObject uiPrefab)
        {
            var objInstanceID = uiPrefab.GetInstanceID();
            return Instance.ShowOrCreateWindows(objInstanceID, uiPrefab);
        }

        public static void HideWindow(GameObject uiPrefab)
        {
            var objInstanceID = uiPrefab.GetInstanceID();
            Instance.HideWindow(objInstanceID);
        }

        public static void HideAll()
        {
            Instance.HideAllWindows();
        }

        public static GameObject GetInstance(GameObject uiPrefab)
        {
            var objInstanceID = uiPrefab.GetInstanceID();
            return Instance.GetInstance(objInstanceID);
        }

        private Dictionary<int, GameObject> WindowsDictionary = new Dictionary<int, GameObject>();

        public static GameObject RootCanvas => Instance.RootWindow.gameObject;

        private Transform rootWindow;
        private Transform RootWindow
        {
            get
            {
                if (rootWindow == null)
                {
                    GameObject canvas = GameObject.FindGameObjectWithTag(CanvasTag);
                    if (canvas == null)
                    {
                        var canvasPrefab = Resources.Load<GameObject>(CanvasPath);
                        if (canvasPrefab == null)
                        {
                            var commonData = CBSScriptable.Get<CommonPrefabs>();
                            canvasPrefab = commonData.Canvas;
                        }
                        var newCanvas = Instantiate(canvasPrefab);
                        rootWindow = newCanvas.transform;
                    }
                    else
                    {
                        rootWindow = canvas.gameObject.transform;
                    }
                }
                return rootWindow;
            }
        }

        public GameObject GetInstance(int id)
        {
            return WindowsDictionary.ContainsKey(id) ? WindowsDictionary[id] : null;
        }

        public GameObject ShowOrCreateWindows(int id, GameObject prefab)
        {
            if (WindowsDictionary.ContainsKey(id))
            {
                var window = WindowsDictionary[id];
                var rect = window.GetComponent<RectTransform>();
                if (rect != null) rect.SetAsLastSibling();
                window.SetActive(true);
                return window;
            }
            else
            {
                var newWindow = Instantiate(prefab, RootWindow);
                WindowsDictionary.Add(id, newWindow);
                var rect = newWindow.GetComponent<RectTransform>();
                if (rect != null) rect.SetAsLastSibling();
                return newWindow;
            }
        }

        public void HideAllWindows()
        {
            foreach(var keyPair in WindowsDictionary)
            {
                HideWindow(keyPair.Key);
            }
        }

        public void HideWindow(int id)
        {
            if (WindowsDictionary.ContainsKey(id))
            {
                var window = WindowsDictionary[id];
                window.SetActive(false);
            }
            else
            {
                Debug.Log("Window not found");
            }
        }
    }
}