using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CBS.UI
{
    public class AvatarDrawer : MonoBehaviour
    {
        [SerializeField]
        protected private Image AvatarImage;

        protected ExamplesConfig Config { get; set; }

        protected AvatarDisplayOptions DisplayOption { get; set; }

        protected bool UseCache { get; set; }

        protected string ProfileID { get; set; }

        private void Awake()
        {
            Config = CBSScriptable.Get<ExamplesConfig>();
            UseCache = Config.UseCacheForAvatars;
            DisplayOption = Config.AvatarDisplay;
        }

        private void OnEnable()
        {
            DisplayDefaultAvatar();
        }

        private void OnDisable()
        {
            ProfileID = string.Empty;
            StopAllCoroutines();
        }

        public void LoadAvatarFromUrl(string url, string profileID)
        {
            if (DisplayOption == AvatarDisplayOptions.ONLY_DEFAULT)
                return;

            StartCoroutine(DisplayFromUrl(url, profileID));
        }

        public void LoadProfileAvatar(string profileID)
        {
            if (DisplayOption == AvatarDisplayOptions.ONLY_DEFAULT)
                return;

            if (UseCache && CacheUtils.IsInCache(profileID))
            {
                var tex = CacheUtils.GetTexture(profileID);
                var avatarSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                AvatarImage.sprite = avatarSprite;
            }

            CBSModule.Get<CBSProfile>().GetPlayerProfile(new CBSGetProfileRequest { 
                ProfileID = profileID
            }, onGet => {
                if (onGet.IsSuccess)
                {
                    var imageUrl = onGet.AvatarURL;
                    if (gameObject.activeInHierarchy)
                        StartCoroutine(DisplayFromUrl(imageUrl, profileID));
                }
                else
                {
                    DisplayDefaultAvatar();
                }
            });
        }

        public void SetClickable(string profileID)
        {
            ProfileID = profileID;
        }

        public virtual void ClickAvatar()
        {
            if (!string.IsNullOrEmpty(ProfileID))
            {
                new PopupViewer().ShowUserInfo(ProfileID);
            }
        }

        protected virtual void DisplayDefaultAvatar()
        {
            AvatarImage.sprite = Config.DefaultAvatar;
        }

        protected IEnumerator DisplayFromUrl(string url, string profile)
        {
            if (string.IsNullOrEmpty(url))
                yield break;

            if (UseCache && CacheUtils.IsInCache(url))
            {
                var tex = CacheUtils.GetTexture(url);
                var avatarSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                AvatarImage.sprite = avatarSprite;
                yield break;
            }

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
#else
                if (www.isNetworkError)
#endif
                {
                    DisplayDefaultAvatar();
                }
                else
                {
                    var tex = DownloadHandlerTexture.GetContent(www);

                    if (UseCache)
                    {
                        var bytes = tex.EncodeToPNG();
                        CacheUtils.Save(url, bytes);
                        CacheUtils.Save(profile, bytes);
                    }

                    if (tex == null)
                    {
                        DisplayDefaultAvatar();
                    }
                    else
                    {
                        var avatarSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        AvatarImage.sprite = avatarSprite;
                    }
                }
            }
        }
    }
}