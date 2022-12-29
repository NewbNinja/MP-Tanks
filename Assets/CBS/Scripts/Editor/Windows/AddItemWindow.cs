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
    public class AddItemWindow : EditorWindow
    {
        private static Action<CatalogItem> AddCallback { get; set; }
        protected static CatalogItem CurrentData { get; set; }
        private static ItemAction Action { get; set; }

        private static ItemType ItemType { get; set; }

        private static List<string> Categories;
        protected static List<string> Currencies { get; set; }

        protected string[] Titles = new string[] { "Info", "Configs", "Icons", "Prices" };
        protected string AddTitle = "Add Item";
        protected string SaveTitle = "Save Item";

        private static int CategoryAtStart { get; set; }

        private string ID { get; set; }
        private string DisplayName { get; set; }
        private string Description { get; set; }
        private string ExternalUrl { get; set; }
        private string ItemCategory { get; set; }
        private string RawCustomData { get; set; }
        private string ItemClass { get; set; }
        private Sprite IconSprite { get; set; }
        private Dictionary<string, uint> Prices { get; set; }
        private bool IsStackable { get; set; }
        private bool IsTradable { get; set; }
        private bool IsConsumable { get; set; }
        private bool IsEquippable { get; set; }
        private bool HasLifeTime { get; set; }

        private uint UsageCount { get; set; }
        private uint LifeTime { get; set; }

        private Vector2 ScrollPos { get; set; }

        private bool IsInited { get; set; } = false;

        private int SelectedCurrencyIndex { get; set; }

        private int SelectedCategoryIndex { get; set; }

        private int SelectedTypeIndex { get; set; }

        private int SelectedToolBar { get; set; }

        private ItemsIcons Icons { get; set; }

        private List<Type> AllDataTypes = new List<Type>();

        private int MaxRawBytes = 1000;

        public static void Show<T>(CatalogItem current, Action<CatalogItem> addCallback, ItemAction action, List<string> category, List<string> currencies, ItemType type, int categoryIndex) where T : EditorWindow
        {
            AddCallback = addCallback;
            CurrentData = current;
            Action = action;
            Categories = category;
            Currencies = currencies;
            ItemType = type;
            CategoryAtStart = categoryIndex;

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
            AllDataTypes = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes())
                       .Where(type => type.IsSubclassOf(typeof(CBSItemData))).ToList();

            Icons = CBSScriptable.Get<ItemsIcons>();

            ID = CurrentData.ItemId;
            DisplayName = CurrentData.DisplayName;
            Description = CurrentData.Description;
            ExternalUrl = CurrentData.ItemImageUrl;
            ItemClass = string.IsNullOrEmpty(CurrentData.ItemClass) ? "CBSDefaultData" : CurrentData.ItemClass;
            RawCustomData = CurrentData.CustomData;

            var dataObject = JsonUtility.FromJson<CBSItemData>(RawCustomData);
            IsEquippable = dataObject == null ? false : dataObject.IsEquippable;
            Prices = CurrentData.VirtualCurrencyPrices ?? new Dictionary<string, uint>();
            IsStackable = CurrentData.IsStackable;
            IsTradable = CurrentData.IsTradable;

            CurrentData.Consumable = CurrentData.Consumable ?? new CatalogItemConsumableInfo();

            IsConsumable = CurrentData.Consumable.UsageCount != null;
            UsageCount = IsConsumable ? (uint)CurrentData.Consumable.UsageCount : 1;
            HasLifeTime = CurrentData.Consumable.UsagePeriod != null;
            LifeTime = HasLifeTime ? (uint)CurrentData.Consumable.UsagePeriod : 5;

            bool tagExist = CurrentData.Tags != null && CurrentData.Tags.Count != 0;
            ItemCategory = tagExist ? CurrentData.Tags[0] : CBSConstants.UndefinedCategory;
            if (Action == ItemAction.ADD)
            {
                ItemCategory = Categories == null || ItemCategory.Length == 0 ? CBSConstants.UndefinedCategory : Categories[CategoryAtStart];
            }
            SelectedCategoryIndex = Categories.Contains(ItemCategory) ? Categories.IndexOf(ItemCategory) : 0;
            var itemClassType = AllDataTypes.Where(x => x.Name == ItemClass).FirstOrDefault();
            SelectedTypeIndex = AllDataTypes.IndexOf(itemClassType);

            IconSprite = Icons.GetSprite(ID);

            IsInited = true;
        }

        protected virtual void CheckInputs()
        {
            DrawInfo();

            CurrentData.ItemId = ID;
            CurrentData.DisplayName = DisplayName;
            CurrentData.Description = Description;
            CurrentData.ItemImageUrl = ExternalUrl;
            CurrentData.CustomData = RawCustomData;
            CurrentData.VirtualCurrencyPrices = Prices;
            CurrentData.IsStackable = IsStackable;
            CurrentData.IsTradable = IsTradable;
            if (IsConsumable)
            {
                CurrentData.Consumable.UsageCount = UsageCount;
            }
            else
            {
                CurrentData.Consumable.UsageCount = null;
            }
            
            if (HasLifeTime)
            {
                CurrentData.Consumable.UsagePeriod = LifeTime;
            }
            else
            {
                CurrentData.Consumable.UsagePeriod = null;
            }

            if (!string.IsNullOrEmpty(ItemCategory))
            {
                if (CurrentData.Tags == null )
                {
                    CurrentData.Tags = new List<string>();
                }
                ItemCategory = Categories[SelectedCategoryIndex];
                if (CurrentData.Tags.Count == 0)
                {
                    CurrentData.Tags.Add(ItemCategory);
                }
                else
                {
                    CurrentData.Tags[0] = ItemCategory;
                }
            }

            CurrentData.ItemClass = AllDataTypes[SelectedTypeIndex].Name;
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
                    case 3:
                        DrawPrices();
                        break;
                    default:
                        break;
                }

                // apply button
                GUILayout.Space(30);
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
            EditorGUILayout.HelpBox("Sprite for game item. ATTENTION! The sprite is not saved on the server, it will be included in the build", MessageType.Info);

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
                ID = EditorGUILayout.TextField("Item ID", ID);
            }
            if (Action == ItemAction.EDIT)
            {
                EditorGUILayout.LabelField("Item ID", ID);
            }
            EditorGUILayout.HelpBox("Unique id for item.", MessageType.Info);

            GUILayout.Space(15);
            DisplayName = EditorGUILayout.TextField("Title", DisplayName);
            EditorGUILayout.HelpBox("Full name of the item", MessageType.Info);

            // description
            GUILayout.Space(15);
            var descriptionTitle = new GUIStyle(GUI.skin.textField);
            descriptionTitle.wordWrap = true;
            EditorGUILayout.LabelField("Description");
            Description = EditorGUILayout.TextArea(Description, descriptionTitle, new GUILayoutOption[] { GUILayout.Height(150) });

            //item tag
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Category");
            SelectedCategoryIndex = EditorGUILayout.Popup(SelectedCategoryIndex, Categories.ToArray());
            EditorGUILayout.HelpBox("Item category", MessageType.Info);

            // draw custom data
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Custom Data");
            SelectedTypeIndex = EditorGUILayout.Popup(SelectedTypeIndex, AllDataTypes.Select(x=>x.Name).ToArray());
            var selectedType = AllDataTypes[SelectedTypeIndex];
            var dataObject = JsonUtility.FromJson(RawCustomData, selectedType);
            if (dataObject == null)
            {
                dataObject = Activator.CreateInstance(selectedType);
            }
            foreach (var f in selectedType.GetFields().Where(f => f.IsPublic))
            {
                // set item type
                if (f.Name == "ItemType")
                {
                    f.SetValue(dataObject, ItemType);
                }
                // set consumable
                else if (f.Name == "IsConsumable")
                {
                    f.SetValue(dataObject, IsConsumable);
                }
                // set consumable
                else if (f.Name == "IsTradable")
                {
                    f.SetValue(dataObject, IsTradable);
                }
                // set equipable
                else if (f.Name == "IsEquippable")
                {
                    f.SetValue(dataObject, IsEquippable);
                }
                // draw string
                else if (f.FieldType  == typeof(string))
                {
                    string stringTitle = f.Name;
                    string stringValue = f.GetValue(dataObject) == null ? string.Empty : f.GetValue(dataObject).ToString();
                    var text = EditorGUILayout.TextField(stringTitle, stringValue);
                    f.SetValue(dataObject, text);
                }
                // draw int
                else if (f.FieldType == typeof(int))
                {
                    string stringTitle = f.Name;
                    int intValue =  (int)f.GetValue(dataObject);
                    var i = EditorGUILayout.IntField(stringTitle, intValue);
                    f.SetValue(dataObject, i);
                }
                // draw float
                else if (f.FieldType == typeof(float))
                {
                    string stringTitle = f.Name;
                    float floatValue = (float)f.GetValue(dataObject);
                    var fl = EditorGUILayout.FloatField(stringTitle, floatValue);
                    f.SetValue(dataObject, fl);
                }
                // draw bool
                else if (f.FieldType == typeof(bool))
                {
                    string stringTitle = f.Name;
                    bool boolValue = (bool)f.GetValue(dataObject);
                    var b = EditorGUILayout.Toggle(stringTitle, boolValue);
                    f.SetValue(dataObject, b);
                }
                // draw enum
                else if (f.FieldType.IsEnum)
                {
                    var enumType = f.FieldType;
                    var enumList = Enum.GetNames(enumType);
                    string stringTitle = f.Name;
                    int enumValue = (int)f.GetValue(dataObject);
                    var e = EditorGUILayout.Popup(enumValue, enumList);
                    f.SetValue(dataObject, e);
                }
            }
            RawCustomData = JsonUtility.ToJson(dataObject);

            // draw raw data progress bar
            int byteCount = System.Text.ASCIIEncoding.Unicode.GetByteCount(RawCustomData);
            float difValue = (float)byteCount / (float)MaxRawBytes;
            string progressTitle = byteCount.ToString() + "/" + MaxRawBytes.ToString() + " bytes";
            float lastY = GUILayoutUtility.GetLastRect().y;
            var lastColor = GUI.color;
            if (byteCount > MaxRawBytes)
            {
                GUI.color = Color.red;
            }
            EditorGUI.ProgressBar(new Rect(3, lastY + 25, position.width - 6, 20), difValue, progressTitle);
            GUI.color = lastColor;
        }

        protected virtual void DrawConfigs()
        {
            GUILayout.Space(15);
            // draw consumable
            EditorGUILayout.LabelField("Is Consumable");
            IsConsumable = EditorGUILayout.Toggle(IsConsumable);
            EditorGUILayout.HelpBox("Determines if item can be used. For example use Consumable option for potions. For static items - disable this option (Sword, armor, shield, etc.). Consumable cant be equippable", MessageType.Info);
            if (IsConsumable)
            {
                IsEquippable = false;
                GUILayout.Space(15);
                // draw uage count
                EditorGUILayout.LabelField("Usage Count");
                UsageCount = (uint)EditorGUILayout.IntField((int)UsageCount, GUILayout.Width(150));
                EditorGUILayout.HelpBox("Determines how many times the item can be used. After use, it is automatically removed from the invertoty. The value cannot be less than 1.", MessageType.Info);
                if (UsageCount < 1)
                    UsageCount = 1;
            }
            GUILayout.Space(15);
            // draw stackable
            EditorGUILayout.LabelField("Is Stackable");
            IsStackable = EditorGUILayout.Toggle(IsStackable);
            if (IsStackable)
            {
                IsTradable = false;
                IsEquippable = false;
            }
                
            EditorGUILayout.HelpBox("Determines if the item can be collected in one stack in the inventory. Stackable cant be tradable", MessageType.Info);
            GUILayout.Space(15);
            // draw equippable
            EditorGUILayout.LabelField("Is Equippable");
            IsEquippable = EditorGUILayout.Toggle(IsEquippable);
            if (IsEquippable)
            {
                IsConsumable = false;
                IsStackable = false;
            }  
            EditorGUILayout.HelpBox("Determines if the item can be equip to the user/character. IsEquippable cant be consumable or stackable", MessageType.Info);
            GUILayout.Space(15);
            // draw tradable
            EditorGUILayout.LabelField("Is Tradable");
            IsTradable = EditorGUILayout.Toggle(IsTradable);
            if (IsTradable)
                IsStackable = false;
            EditorGUILayout.HelpBox("Determines if a player can trade this item with other players. Tradable cant be stackable", MessageType.Info);
            GUILayout.Space(15);
            // draw has life time
            EditorGUILayout.LabelField("Has Lifetime");
            HasLifeTime = EditorGUILayout.Toggle(HasLifeTime);
            EditorGUILayout.HelpBox("Determines if the item has a lifetime. The countdown begins after the item enters the invertony. After the passage of time - the item is automatically deleted.", MessageType.Info);

            if (HasLifeTime)
            {
                GUILayout.Space(15);
                // draw uage count
                EditorGUILayout.LabelField("Life time in seconds");
                LifeTime = (uint)EditorGUILayout.IntField((int)LifeTime, GUILayout.Width(150));
                EditorGUILayout.HelpBox("Lifetime of the item. Cannot be less than 5.", MessageType.Info);
                if (LifeTime < 5)
                    LifeTime = 5;
            }
        }

        private void DrawPrices()
        {
            if (Currencies != null && Currencies.Count != 0)
            {
                // draw currencies list
                GUILayout.Space(15);
                var contentTitle = new GUIStyle(GUI.skin.label);
                contentTitle.fontStyle = FontStyle.Bold;
                EditorGUILayout.LabelField("List of prices", contentTitle);

                if (Prices != null && Prices.Count != 0)
                {
                    for (int i = 0; i < Prices.Count; i++)
                    {
                        string key = Prices.Keys.ElementAt(i);
                        int val = (int)Prices.Values.ElementAt(i);
                        GUILayout.BeginHorizontal();
                        Prices[key] = (uint)EditorGUILayout.IntField(key, val);

                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            Prices.Remove(key);
                        }

                        GUILayout.EndHorizontal();
                    }
                }
                // add currency button
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();
                if (Currencies != null && Currencies.Count != 0)
                {
                    SelectedCurrencyIndex = EditorGUILayout.Popup(SelectedCurrencyIndex, Currencies.ToArray());
                    string defaultKey = Currencies[SelectedCurrencyIndex];
                    if (GUILayout.Button("Add currency"))
                    {
                        if (!Prices.ContainsKey(defaultKey))
                            Prices[defaultKey] = 0;
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("List of virtual currencies", MessageType.Info);
            }
        }

        private bool IsInputValid()
        {
            int byteCount = System.Text.ASCIIEncoding.Unicode.GetByteCount(RawCustomData);
            return byteCount < MaxRawBytes;
        }
    }
}
#endif