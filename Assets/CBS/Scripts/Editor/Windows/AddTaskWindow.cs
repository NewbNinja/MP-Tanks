#if ENABLE_PLAYFABADMIN_API
using CBS.Scriptable;
using PlayFab;
using PlayFab.AdminModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CBS.Editor.Window
{
    public class AddTaskWindow<TTask> : EditorWindow where TTask : CBSTask, new()
    {
        private static Action<TTask> AddCallback { get; set; }
        protected static TTask CurrentData { get; set; }
        private static ItemAction Action { get; set; }

        protected static List<string> Currencies { get; set; }

        protected string[] Titles = new string[] { "Info", "Configs", "Icons" };
        protected string AddTitle = "Add Task";
        protected string SaveTitle = "Save Task";

        private string ID { get; set; }
        private string Title { get; set; }
        private string Description { get; set; }
        private string ExternalUrl { get; set; }
        private string ItemTag { get; set; }
        private Sprite IconSprite { get; set; }
        private TaskType TaskType { get; set; }
        private int Steps { get; set; }
        private bool LockedByLevel { get; set; }
        private int LockedLevel { get; set; }

        private Vector2 ScrollPos { get; set; }

        private bool IsInited { get; set; } = false;

        private int SelectedToolBar { get; set; }

        private TasksIcons Icons { get; set; }

        public static void Show<T>(TTask current, ItemAction action, Action<TTask> addCallback) where T : EditorWindow
        {
            AddCallback = addCallback;
            CurrentData = current;
            Action = action;

            var window = ScriptableObject.CreateInstance<T>();
            window.maxSize = new Vector2(400, 700);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }

        private void Hide()
        {
            this.Close();
        }

        protected virtual void Init()
        {
            Icons = CBSScriptable.Get<TasksIcons>();

            ID = CurrentData.ID;
            Title = CurrentData.Title;
            Description = CurrentData.Description;
            ExternalUrl = CurrentData.ExternalUrl;
            ItemTag = CurrentData.Tag;
            TaskType = CurrentData.Type;
            IconSprite = Icons.GetSprite(ID);
            Steps = CurrentData.Steps;
            LockedByLevel = CurrentData.IsLockedByLevel;
            LockedLevel = CurrentData.LockLevel;

            IsInited = true;
        }

        protected virtual void CheckInputs()
        {
            DrawInfo();

            CurrentData.ID = ID;
            CurrentData.Title = Title;
            CurrentData.Description = Description;
            CurrentData.ExternalUrl = ExternalUrl;
            CurrentData.Tag = ItemTag;
            CurrentData.Type = TaskType;
            CurrentData.Steps = TaskType == TaskType.ONE_SHOT ? 0 : Steps;
            CurrentData.IsLockedByLevel = LockedByLevel;
            CurrentData.LockLevel = LockedLevel;
        }

        void OnGUI()
        {
            using (var areaScope = new GUILayout.AreaScope(new Rect(0,0, 400, 700)))
            {
                ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

                SelectedToolBar = GUILayout.Toolbar(SelectedToolBar, Titles);

                // init start values
                if (!IsInited)
                {
                    Init();
                }

                switch (SelectedToolBar)
                {
                    case 0:
                        DrawInfo();
                        break;
                    case 1:
                        DrawConfigs();
                        break;
                    case 2:
                        DrawIcons();
                        break;
                    default:
                        break;
                }

                // apply button
                GUILayout.FlexibleSpace();
                string buttonTitle = Action == ItemAction.ADD ? AddTitle : SaveTitle;
                if (GUILayout.Button(buttonTitle))
                {
                    if (IsInputValid())
                    {
                        if (IconSprite == null)
                        {
                            Icons.RemoveSprite(ID);
                        }
                        else
                        {
                            Icons.SaveSprite(ID, IconSprite);
                        }
                        CheckInputs();
                        AddCallback?.Invoke(CurrentData);
                        Hide();
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawIcons()
        {
            GUILayout.Space(15);
            // draw icon
            EditorGUILayout.LabelField("Sprite", new GUILayoutOption[] { GUILayout.MaxWidth(150) });
            IconSprite = (Sprite)EditorGUILayout.ObjectField((IconSprite as UnityEngine.Object), typeof(Sprite), false, new GUILayoutOption[] { GUILayout.MaxWidth(150) });
            EditorGUILayout.HelpBox("Sprite for game task. ATTENTION! The sprite is not saved on the server, it will be included in the build", MessageType.Info);

            // draw preview
            var iconTexture = IconSprite == null ? null : IconSprite.texture;
            GUILayout.Button(iconTexture, GUILayout.Width(100), GUILayout.Height(100));

            // external url
            GUILayout.Space(15);
            ExternalUrl = EditorGUILayout.TextField("External Icon URL", ExternalUrl);
            EditorGUILayout.HelpBox("You can use it for example for remote texture url", MessageType.Info);
        }

        private void DrawInfo()
        {
            GUILayout.Space(15);
            if (Action == ItemAction.ADD)
            {
                ID = EditorGUILayout.TextField("Task ID", ID);
            }
            if (Action == ItemAction.EDIT)
            {
                EditorGUILayout.LabelField("Task ID", ID);
            }
            EditorGUILayout.HelpBox("Unique id for task", MessageType.Info);
            if (string.IsNullOrEmpty(ID))
            {
                EditorGUILayout.HelpBox("ID can not be empty", MessageType.Error);
            }

            GUILayout.Space(15);
            Title = EditorGUILayout.TextField("Title", Title);
            EditorGUILayout.HelpBox("Full name of the task", MessageType.Info);

            // description
            GUILayout.Space(15);
            var descriptionTitle = new GUIStyle(GUI.skin.textField);
            descriptionTitle.wordWrap = true;
            EditorGUILayout.LabelField("Description");
            Description = EditorGUILayout.TextArea(Description, descriptionTitle, new GUILayoutOption[] { GUILayout.Height(150) });

            //item tag
            GUILayout.Space(15);
            ItemTag = EditorGUILayout.TextField("Tag", ItemTag);
            EditorGUILayout.HelpBox("Task tag", MessageType.Info);

        }

        protected virtual void DrawConfigs()
        {
            // draw type
            GUILayout.Space(15);
            TaskType = (TaskType)EditorGUILayout.EnumPopup("Task Type", TaskType);
            EditorGUILayout.HelpBox("Task type. If OneShot is selected, the task will be completed the first time it is executed. If Steps - you need to perform several steps of the actions specified by you", MessageType.Info);
            if (TaskType == TaskType.STEPS)
            {
                GUILayout.Space(15);
                Steps = EditorGUILayout.IntField("Steps", Steps);
                EditorGUILayout.HelpBox("Number of steps to complete the task", MessageType.Info);
                if (Steps <= 0)
                    Steps = 1;
            }

            // draw level
            GUILayout.Space(15);
            LockedByLevel = EditorGUILayout.Toggle("Locked by level?", LockedByLevel);
            EditorGUILayout.HelpBox("Determines whether the task will be locked by player level. The player will be able to perform the task only when his level is equal to or higher", MessageType.Info);
            if (LockedByLevel)
            {
                GUILayout.Space(15);
                LockedLevel = EditorGUILayout.IntField("Level", LockedLevel);
                EditorGUILayout.HelpBox("Level to lock task", MessageType.Info);
                if (LockedLevel < 0)
                    LockedLevel = 0;
            }

        }

        private bool IsInputValid()
        {
            var validID = !string.IsNullOrEmpty(ID);
            return validID;
        }
    }
}
#endif