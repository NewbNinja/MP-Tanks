using CBS.Scriptable;
using CBS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class ExampleConfigurator : BaseConfigurator
    {
        protected override string Title => "Examples Configurator";

        protected override bool DrawScrollView => true;

        private ExamplesConfig Config { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            Config = CBSScriptable.Get<ExamplesConfig>();
        }

        protected override void OnDrawInside()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 12;

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Avatar Display Options", titleStyle);
            Config.AvatarDisplay = (AvatarDisplayOptions)EditorGUILayout.EnumPopup(Config.AvatarDisplay, new GUILayoutOption[] { GUILayout.Width(150) });

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Use Cache For Avatars", titleStyle);
            Config.UseCacheForAvatars = EditorGUILayout.Toggle(Config.UseCacheForAvatars);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Default Avatar Sprite", titleStyle);
            Config.DefaultAvatar = (Sprite)EditorGUILayout.ObjectField((Config.DefaultAvatar as Object), typeof(Sprite), false, new GUILayoutOption[] { GUILayout.MaxWidth(150) });

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Default Clan Sprite", titleStyle);
            Config.DefaultClanAvatar = (Sprite)EditorGUILayout.ObjectField((Config.DefaultClanAvatar as Object), typeof(Sprite), false, new GUILayoutOption[] { GUILayout.MaxWidth(150) });

            Config.Save();
        }
    }
}
