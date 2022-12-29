using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.Scriptable
{
    public abstract class CBSScriptable : ScriptableObject
    {
        private static List<CBSScriptable> Scribtables { get; set; } = new List<CBSScriptable>();

        public abstract string ResourcePath { get; }

        internal virtual T Load<T>() where T : CBSScriptable
        {
            return Resources.Load<T>(ResourcePath);
        }

        public void Save()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public static T Get<T>() where T : CBSScriptable, new()
        {
            bool containData = Scribtables.Any(x => x.GetType() == typeof(T));
            if (containData)
            {
                var data = Scribtables.FirstOrDefault(x => x.GetType() == typeof(T));
                return (T)data;
            }
            else
            {
                var newData = CreateInstance<T>().Load<T>();
                Scribtables.Add(newData);
                return newData;
            }
        }
    }
}
