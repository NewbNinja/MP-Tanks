using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor
{
    public abstract class BaseConfigurator
    {
        public event Action OnDraw;

        protected static List<BaseConfigurator> AllConfigurator = new List<BaseConfigurator>();

        protected abstract string Title { get; }

        protected abstract bool DrawScrollView { get; }

        private bool ShowProgressBar { get; set; }

        private Vector2 ScrollPos { get; set; }

        private GUILayoutOption[] TitleOptions
        {
            get
            {
                return new GUILayoutOption[] { GUILayout.Height(54.66f), GUILayout.Width(454.66f) };
            }
        }

        public virtual void Init(MenuTitles menu)
        {
            MenuTitle = menu;
        }

        protected MenuTitles MenuTitle { get; private set; }

        public static T Get<T>() where T : BaseConfigurator, new()
        {
            return (T)AllConfigurator.LastOrDefault(x => x.GetType() == typeof(T));
        }


        public void Draw(Rect rect)
        {
            OnDraw?.Invoke();

            using (var areaScope = new GUILayout.AreaScope(rect))
            {
                if (DrawScrollView)
                    ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

                // draw title
                GUIStyle style = new GUIStyle("Label");
                var tex = ResourcesUtils.GetTitleTexture(MenuTitle);

                GUILayout.BeginHorizontal();
                GUILayout.Space(300);
                GUILayout.Button(tex, style, TitleOptions);
                GUILayout.EndHorizontal();

                var colorLine = Color.black;
                colorLine.a = 0.5f;
                EditorUtils.DrawUILine(colorLine, 4, 20);
                GUILayout.Space(2);
                
                OnDrawInside();
                
                if (DrawScrollView)
                    EditorGUILayout.EndScrollView();
            }

            if (ShowProgressBar)
            {
                EditorUtility.DisplayProgressBar("Procesing", "Please wating...", 1f);
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }

        protected abstract void OnDrawInside();

        protected void ShowProgress()
        {
            ShowProgressBar = true;
        }

        protected void HideProgress()
        {
            ShowProgressBar = false;
        }

        protected void AddErrorLog(string message)
        {
            Debug.LogError(message);
        }

        protected void AddErrorLog(PlayFabError error)
        {
            Debug.LogError(error?.ErrorMessage);
        }
    }
}
