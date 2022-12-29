using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.UI
{
    public class ClanAvatarDrawer : AvatarDrawer
    {
        public override void ClickAvatar()
        {
            if (!string.IsNullOrEmpty(ProfileID))
            {
                new PopupViewer().ShowClanInfo(ProfileID);
            }
        }

        protected override void DisplayDefaultAvatar()
        {
            AvatarImage.sprite = Config.DefaultClanAvatar;
        }

        public void LoadFromClanProfile(string clanId)
        {
            if (DisplayOption == AvatarDisplayOptions.ONLY_DEFAULT)
                return;

            if (UseCache && CacheUtils.IsInCache(clanId))
            {
                var tex = CacheUtils.GetTexture(clanId);
                var avatarSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                AvatarImage.sprite = avatarSprite;
            }

            CBSModule.Get<CBSClan>().GetClanInfo(clanId, onGet => {
                if (onGet.IsSuccess)
                {
                    var imageUrl = onGet.Info.ImageURL;
                    if (gameObject.activeInHierarchy)
                        StartCoroutine(DisplayFromUrl(imageUrl, clanId));
                }
                else
                {
                    DisplayDefaultAvatar();
                }
            });
        }
    }
}
