using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

namespace CBS
{
    public abstract class CBSBaseItem
    {
        public string ID { get; protected set; }
        public string DisplayName { get; protected set; }
        public string Description { get; protected set; }
        public string Category { get; protected set; }
        public string ItemClass { get; protected set; }
        public string ExternalIconURL { get; protected set; }
        public ItemType Type { get; protected set; }
        public Dictionary<string, uint> Prices { get; protected set; } = new Dictionary<string, uint>();

        internal string CustomData { get; set; }

        public virtual T GetCustomData<T>() where T : CBSItemData
        {
            return JsonUtility.FromJson<T>(CustomData);
        }

        public Dictionary<string, object> GetCustomDataAsDictionary()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).FirstOrDefault(x => x.Name == ItemClass);
            var data = JsonUtility.FromJson(CustomData, type);
            var baseList = typeof(CBSItemData).GetFields().Where(f => f.IsPublic).Select(x=>x.Name).ToList();
            var list = type.GetFields().Where(f => f.IsPublic && !baseList.Contains(f.Name));
            return list.ToDictionary(x => x.Name, x => x.GetValue(data));
        }
    }
}
