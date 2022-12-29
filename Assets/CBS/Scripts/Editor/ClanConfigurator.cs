#if ENABLE_PLAYFABADMIN_API
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.AdminModels;
using UnityEditor;
using System.Linq;
using System;
using CBS.Playfab;
using CBS.Scriptable;

namespace CBS.Editor
{
    public class ClanConfigurator : BaseConfigurator
    {
        protected override string Title => "Clan Configuration";

        private readonly int MembersMaxMalue = 100;

        protected override bool DrawScrollView => true;

        private int MaxMembers { get; set; } = 20; // max 100

        private EditorData EditorData { get; set; }

        public override void Init(MenuTitles title)
        {
            base.Init(title);
            EditorData = CBSScriptable.Get<EditorData>();
            GetClanData();
        }

        protected override void OnDrawInside()
        {
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            // draw apa key
            EditorGUILayout.LabelField("Max members", titleStyle);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            MaxMembers = EditorGUILayout.IntField(MaxMembers, new GUILayoutOption[] { GUILayout.Width(100) });
            if (MaxMembers < 0)
            {
                MaxMembers = 0;
            }
            if (MaxMembers > MembersMaxMalue)
            {
                MaxMembers = MembersMaxMalue;
            }
            GUILayout.FlexibleSpace();
            if (EditorUtils.DrawButton("Save", EditorData.EditColor, 12))
            {
                SaveDataByKey(CBSConstants.MaxClanMembersKey, MaxMembers.ToString());
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Max members in group. Max value = 100", MessageType.Info);

            GUILayout.Space(20);
        }

        private void GetClanData()
        {
            ShowProgress();
            var keys = new List<string>();
            keys.Add(CBSConstants.MaxClanMembersKey);
            var request = new GetTitleDataRequest {
                Keys = keys
            };
            PlayFabAdminAPI.GetTitleInternalData(request, OnInternalDataGetted, OnGetDataFailed);
        }

        private void OnInternalDataGetted(GetTitleDataResult result)
        {
            HideProgress();
            var dictionary = result.Data;
            bool keyExist = dictionary.ContainsKey(CBSConstants.MaxClanMembersKey);
            string rawMaxValue = keyExist ? dictionary[CBSConstants.MaxClanMembersKey] : MaxMembers.ToString();
            MaxMembers = int.Parse(rawMaxValue);
        }

        private void OnGetDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }

        private void SaveDataByKey(string key, string value)
        {
            ShowProgress();
            var request = new SetTitleDataRequest
            {
                Key = key,
                Value = value
            };
            PlayFabAdminAPI.SetTitleInternalData(request, OnSetData, OnSetDataFailed);
        }

        private void OnSetData(SetTitleDataResult result)
        {
            HideProgress();
            GetClanData();
        }

        private void OnSetDataFailed(PlayFabError error)
        {
            AddErrorLog(error);
            HideProgress();
        }
    }
}
#endif
