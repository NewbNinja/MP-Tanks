using CBS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS
{
    public abstract class CBSModule
    {
        private static List<CBSModule> Modules { get; set; } = new List<CBSModule>();

        public CBSModule()
        {
            Init();
        }

        protected virtual void Init() { }

        public void Bind() { }

        public static T Get<T>() where T : CBSModule, new()
        {
            bool containModule = Modules.Any(x => x.GetType() == typeof(T));
            if (containModule)
            {
                var module = Modules.FirstOrDefault(x => x.GetType() == typeof(T));
                return (T)module;
            }
            else
            {
                var newModule = new T();
                Modules.Add(newModule);
                return newModule;
            }
        }

        internal void LogoutPrecces()
        {
            foreach (var module in Modules)
                module?.OnLogout();
        }

        protected virtual void OnLogout() { }
    }
}
