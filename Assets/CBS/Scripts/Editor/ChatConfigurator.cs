using CBS.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public class ChatConfigurator : BaseConfigurator
    {
        protected override string Title => "Auth Configurator";

        protected override bool DrawScrollView => true;

        private ChatConfig ChatData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            ChatData = CBSScriptable.Get<ChatConfig>();
        }

        protected override void OnDrawInside()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("General options", titleStyle);
            GUILayout.Space(10);

            int maxMessageLength = ChatData.MaxMessageLength;
            maxMessageLength = EditorGUILayout.IntField("Max message length", ChatData.MaxMessageLength, new GUILayoutOption[] { GUILayout.Width(400) });
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The maximum length of a message that the user can send", MessageType.Info);

            ChatData.MaxMessageLength = maxMessageLength;

            ChatData.Save();
        }
    }
}
