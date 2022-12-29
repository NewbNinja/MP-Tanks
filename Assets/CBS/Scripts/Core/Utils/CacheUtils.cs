using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CBS.Utils
{
    public class CacheUtils
    {
        public static bool IsInCache(string url)
        {
            string hash = GetMd5Hash(url);
            return DeviceStorage.FileExists(hash);
        }

        public static void Save(string url, byte[] bytes)
        {
            string hash = GetMd5Hash(url);
            DeviceStorage.WriteFile(hash, bytes);
        }

        public static byte[] Get(string url)
        {
            string hash = GetMd5Hash(url);
            return DeviceStorage.ReadFile(hash);
        }

        public static Texture2D GetTexture(string url)
        {
            var bytes = Get(url);
            Texture2D tex = new Texture2D(16, 16);
            tex.LoadImage(bytes);
            //tex.LoadRawTextureData(bytes);
            tex.Apply();
            //tex.EncodeToJPG();
            return tex;
        }

        private static string GetMd5Hash(string str)
        {
            // step 1, calculate MD5 hash from input
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(str);
                byte[] hash = md5.ComputeHash(inputBytes);
                // step 2, convert byte array to hex string
                var sb = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                var res = sb.ToString().ToLower();
                return res;
            }
        }
    }
}
