using CBS.Core;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class ObjectCustomDataDrawer<T> where T : CBSBaseCustomData
    {
        private List<Type> AllDataTypes = new List<Type>();
        public string ClassName { get; set; }
        public int SelectedTypeIndex { get; set; }

        private object LastSavedObject { get; set; }

        private Dictionary<string, string> RawDataCache;

        private ISerializerPlugin JsonPlugin { get; set; }

        public ObjectCustomDataDrawer()
        {
            AllDataTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t => t.IsSubclassOf(typeof(T))).ToList();
            JsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
        }

        private void Reset()
        {
            Debug.Log("reset");
            SelectedTypeIndex = 0;
            ClassName = string.Empty;
            RawDataCache = new Dictionary<string, string>();
            GUI.FocusControl(null);
        }

        public string Draw(ICustomData oblectToDraw)
        {
            if (oblectToDraw != LastSavedObject)
            {
                LastSavedObject = oblectToDraw;
                Reset();
            }
            var customRawData = oblectToDraw.CustomRawData;
            var className = oblectToDraw.CustomDataClassName;
            if (string.IsNullOrEmpty(customRawData))
                customRawData = string.Empty;
            if (AllDataTypes.Count == 0)
                return string.Empty;
            if (string.IsNullOrEmpty(className))
            {
                className = string.Empty;
                SelectedTypeIndex = 0;
            }
            var itemClassType = string.IsNullOrEmpty(className) ? AllDataTypes.FirstOrDefault() : AllDataTypes.Where(x => x.Name == className).FirstOrDefault();
            SelectedTypeIndex = AllDataTypes.IndexOf(itemClassType);
            
            SelectedTypeIndex = EditorGUILayout.Popup(SelectedTypeIndex, AllDataTypes.Select(x => x.Name).ToArray());

            if (SelectedTypeIndex < 0)
                SelectedTypeIndex = 0;

            var selectedType = AllDataTypes[SelectedTypeIndex];
            className = selectedType.Name;

            ClassName = className;

            var cachedRawData = RawDataCache.ContainsKey(ClassName) ? RawDataCache[ClassName] : customRawData;
            var dataObject = JsonUtility.FromJson(cachedRawData, selectedType);
            if (dataObject == null)
            {
                dataObject = Activator.CreateInstance(selectedType);
            }
            foreach (var f in selectedType.GetFields().Where(f => f.IsPublic))
            {
                // draw string
                if (f.FieldType == typeof(string))
                {
                    string stringTitle = f.Name;
                    string stringValue = f.GetValue(dataObject) == null ? string.Empty : f.GetValue(dataObject).ToString();
                    var text = EditorGUILayout.TextField(stringTitle, stringValue);
                    f.SetValue(dataObject, text);
                }
                // draw int
                else if (f.FieldType == typeof(int))
                {
                    string stringTitle = f.Name;
                    int intValue = (int)f.GetValue(dataObject);
                    var i = EditorGUILayout.IntField(stringTitle, intValue);
                    f.SetValue(dataObject, i);
                }
                // draw float
                else if (f.FieldType == typeof(float))
                {
                    string stringTitle = f.Name;
                    float floatValue = (float)f.GetValue(dataObject);
                    var fl = EditorGUILayout.FloatField(stringTitle, floatValue);
                    f.SetValue(dataObject, fl);
                }
                // draw bool
                else if (f.FieldType == typeof(bool))
                {
                    string stringTitle = f.Name;
                    bool boolValue = (bool)f.GetValue(dataObject);
                    var b = EditorGUILayout.Toggle(stringTitle, boolValue);
                    f.SetValue(dataObject, b);
                }
                // draw enum
                else if (f.FieldType.IsEnum)
                {
                    var enumType = f.FieldType;
                    var enumList = Enum.GetNames(enumType);
                    string stringTitle = f.Name;
                    int enumValue = (int)f.GetValue(dataObject);
                    var e = EditorGUILayout.Popup(enumValue, enumList);
                    f.SetValue(dataObject, e);
                }
            }
            var rawData = JsonUtility.ToJson(dataObject);
            RawDataCache[ClassName] = rawData;
            oblectToDraw.CustomRawData = rawData;
            oblectToDraw.CustomDataClassName = ClassName;
            return rawData;
        }
    }
}
