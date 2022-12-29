using CBS.Playfab;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBS.Playfab
{
    public class FabExecuter
    {
        private static List<FabExecuter> Modules { get; set; } = new List<FabExecuter>();

        public FabExecuter()
        {
            Init();
        }

        protected virtual void Init() { }


        public static T Get<T>() where T : FabExecuter, new()
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
    }
}
