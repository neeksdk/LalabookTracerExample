using System.Collections.Generic;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.LevelCreator;
using UnityEditor;
using UnityEngine;

namespace neeksdk.Editor
{
    public class TraceRedactorWindow : EditorWindow {
    
        public static TraceRedactorWindow instance;
    
        private List<TraceRedactorItem.Category> _categories;
        private List<string> _categoryLabels;
        private TraceRedactorItem.Category _categorySelected;
        private string _path = RedactorConstants.PREFABS_PATH;
        private List<TraceRedactorItem> _items;
        private Dictionary<TraceRedactorItem.Category, List<TraceRedactorItem>> _categorizedItems;
        private Dictionary<TraceRedactorItem, Texture2D> _previews;
        private Vector2 _scrollPosition;
        private const float BUTTON_WIDTH = 120;
        private const float BUTTON_HEIGHT = 110;

        private const string WINDOW_NAME = "Trace redactor";
    
        public static void ShowTraceRedactor() {
            instance = (TraceRedactorWindow) EditorWindow.GetWindow(typeof(TraceRedactorWindow));
            instance.titleContent = new GUIContent(WINDOW_NAME);
        }

        private void OnEnable() {
            if (_categories == null) InitCategories();
            if (_categorizedItems == null) InitContent();
        }
    
        private void InitCategories () {
            _categories = EditorUtils.GetListFromEnum<TraceRedactorItem.Category> ();
            _categoryLabels = new List<string> ();
            foreach (TraceRedactorItem.Category category in _categories) {
                _categoryLabels.Add (category.ToString());
            }
        }
    
        private void InitContent () {
            _items = EditorUtils.GetAssetsWithScript<TraceRedactorItem> (_path);
            _categorizedItems = new Dictionary<TraceRedactorItem.Category, List<TraceRedactorItem>> ();
            _previews = new Dictionary<TraceRedactorItem, Texture2D> ();
            
            foreach (TraceRedactorItem.Category category in _categories) {
                _categorizedItems.Add (category, new List<TraceRedactorItem> ());
            }
            
            foreach (TraceRedactorItem item in _items) {
                _categorizedItems [item.category].Add (item);
            }
        }

        private void OnGUI() {
            DrawTabs();
            DrawScroll();
        }

        private void DrawTabs () {
            int index = (int)_categorySelected;
            index = GUILayout.Toolbar (index, _categoryLabels.ToArray ());
            _categorySelected = _categories [index];
        }

        private void DrawScroll () {
            if (_categorizedItems [_categorySelected].Count == 0) {
                EditorGUILayout.HelpBox ("This category is empty!", MessageType.Info);
                return;
            }
            int rowCapacity = Mathf.FloorToInt (position.width / (BUTTON_WIDTH));
            _scrollPosition = GUILayout.BeginScrollView (_scrollPosition);
            int selectionGridIndex = -1;
            selectionGridIndex = GUILayout.SelectionGrid (selectionGridIndex, GetGUIContentsFromItems (),rowCapacity, GetGUIStyle ());
            GetSelectedItem (selectionGridIndex);
            GUILayout.EndScrollView ();
        }
    
        private void GeneratePreviews () {
            foreach (TraceRedactorItem item in _items) {
                if (!_previews.ContainsKey (item)) {
                    Texture2D preview = AssetPreview.GetAssetPreview (item.gameObject);
                    if (preview != null) {
                        _previews.Add (item, preview);
                    }
                }
            }
        }
    
        private void Update () {
            if (_previews.Count != _items.Count) {
                GeneratePreviews ();
            }
        }
    
        private GUIContent[] GetGUIContentsFromItems () {
            List<GUIContent> guiContents = new List<GUIContent> ();
            if(_previews.Count == _items.Count) {
                int totalItems = _categorizedItems [_categorySelected].Count;
                for (int i = 0; i < totalItems; i ++) {
                    GUIContent guiContent = new GUIContent ();
                    guiContent.text = _categorizedItems [_categorySelected][i].itemName;
                    guiContent.image = _previews [_categorizedItems [_categorySelected] [i]];
                    guiContents.Add (guiContent);
                }
            }
            return guiContents.ToArray ();
        }
    
        private GUIStyle GetGUIStyle () {
            GUIStyle guiStyle = new GUIStyle (GUI.skin.button);
            guiStyle.alignment = TextAnchor.LowerCenter;
            guiStyle.imagePosition = ImagePosition.ImageAbove;
            guiStyle.fixedWidth = BUTTON_WIDTH;
            guiStyle.fixedHeight = BUTTON_HEIGHT;
            return guiStyle;
        }
    
        public delegate void ItemSelectedDelegate (TraceRedactorItem item, Texture2D preview);
        public static event ItemSelectedDelegate ItemSelectedEvent;

        private void GetSelectedItem(int index)
        {
            if (index == -1)
            {
                return;
            }
            
            TraceRedactorItem selectedItem = _categorizedItems[_categorySelected][index];
            ItemSelectedEvent?.Invoke(selectedItem, _previews [selectedItem]);
            //Debug.Log("Selected Item is: " + selectedItem.itemName);
            //Debug.Log("Send event");
        }
    }
}
