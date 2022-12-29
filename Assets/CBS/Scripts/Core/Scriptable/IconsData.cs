using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace CBS.Scriptable
{
    public abstract class IconsData : CBSScriptable
    {
        public List<IconData> AllSprites;

        public abstract string EditorPath { get; }
        public abstract string EditorAssetName { get; }

        internal override T Load<T>()
        {
#if UNITY_EDITOR
            var pathExist = Directory.Exists(EditorPath);
            if (!pathExist)
            {
                var directory = Directory.CreateDirectory(EditorPath);
                UnityEditor.AssetDatabase.Refresh();
            }
            
            
            var pathToAsset = EditorPath + "/" + EditorAssetName;
            var fileExists = File.Exists(pathToAsset);

            if (!fileExists)
            {
                T asset = CreateInstance<T>();
                UnityEditor.AssetDatabase.CreateAsset(asset, pathToAsset);
                UnityEditor.AssetDatabase.SaveAssets();

                UnityEditor.AssetDatabase.Refresh();
            }
#endif
            return base.Load<T>();
        }

        public Sprite GetSprite(string id)
        {
            if (AllSprites == null) return null;
            if (string.IsNullOrEmpty(id)) return null;
            var existedData = AllSprites.FirstOrDefault(x => x.ID == id);
            return existedData == null ? null : existedData.Sprite;
        }

#if UNITY_EDITOR
        public void SaveSprite(string id, Sprite sprite)
        {
            if (AllSprites == null)
                AllSprites = new List<IconData>();
            if (string.IsNullOrEmpty(id)) return;

            var existedData = AllSprites.FirstOrDefault(x => x.ID == id);

            if (existedData != null)
            {
                existedData.Sprite = sprite;
            }
            else
            {
                AllSprites.Add(new IconData {
                    ID = id,
                    Sprite = sprite
                });
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void RemoveSprite(string id)
        {
            if (AllSprites == null)
                return;
            if (string.IsNullOrEmpty(id)) return;
            var existedData = AllSprites.FirstOrDefault(x => x.ID == id);

            if (existedData == null)
                return;

            AllSprites.Remove(existedData);
            AllSprites.TrimExcess();

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    [System.Serializable]
    public class IconData
    {
        public string ID;
        public Sprite Sprite;
    }
}
