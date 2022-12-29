using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace CBS.Utils
{
    public class DeviceStorage
    {
        public static string LocalFolderAbsolutePath
        {
            get
            {
                return Application.persistentDataPath + "/";
            }
        }

        private static void WriteText(string relativePath, string data)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, data);
        }

        public static byte[] ReadFile(string relativePath)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            if (File.Exists(path))
                return File.ReadAllBytes(path);
            return null;
        }

        public static void WriteFile(string relativePath, byte[] data)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllBytes(path, data);
        }


        private static string ReadText(string relativePath)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            if (File.Exists(path))
                return File.ReadAllText(path);
            return string.Empty;
        }

        public static void Delete(string relativePath)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            if (File.Exists(path))
                File.Delete(path);
        }

        public static bool FileExists(string relativePath)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            return File.Exists(path);
        }


        public static bool PathExists(string path)
        {
            return File.Exists(path);
        }

        private static bool TextExists(string relativePath)
        {
            var path = Path.Combine(LocalFolderAbsolutePath, relativePath);

            return File.Exists(path);
        }

        public static T GetJsonFromDisk<T>(string _pathKey)
        {
            T _tempObj = default;
            if (TextExists(_pathKey))
            {
                string _deviceJson = ReadText(_pathKey);
                _tempObj = JsonUtility.FromJson<T>(_deviceJson);
            }
            return _tempObj;
        }

        public static void SaveJsonToDisk<T>(T _obj, string _pathKey)
        {
            string _deviceListJson = JsonUtility.ToJson(_obj);
            WriteText(_pathKey, _deviceListJson);
        }
    }
}

