using System.Collections.Generic;
using System.Linq;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.Infrastructure.Factory;
using neeksdk.Scripts.Infrastructure.SaveLoad;
using neeksdk.Scripts.LevelCreator;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using neeksdk.Scripts.Properties;
using neeksdk.Scripts.StaticData.LinesData;
using UnityEditor;
using UnityEngine;

namespace neeksdk.Editor {
    [CustomEditor(typeof(FigureConstructor))]
    public class FigureConstructorEditorScript : UnityEditor.Editor {
        private FigureConstructor _myTarget;
        private SerializedObject _mySerializedObject;
        private TraceRedactorItem _itemSelected;
        private GameObject _itemInspected;
        private Texture2D _itemPreview;
        private GameObject _pieceSelected;
        
        private BezierLine _selectedBezierLine;
        private Transform _selectedBezierDot;
        private bool _bezierDotLocked;
        private Vector3 _dotPositionBeforeDrag;

        private BezierLineFactory _bezierLineFactory;

        private enum FigureConstructorModes {
            View,
            Paint,
            Move
        }

        private FigureConstructorModes _selectedFigureConstructorModes;
        private FigureConstructorModes _currentFigureConstructorModes;

        private BezierLineFactory BezierLineFactory
        {
            get
            {
                if (_bezierLineFactory == null)
                {
                    _bezierLineFactory = new BezierLineFactory();
                }

                return _bezierLineFactory;
            }
        }

        private void OnEnable()
        {
            _myTarget = (FigureConstructor) target;
            _mySerializedObject = new SerializedObject(_myTarget);
            
            if (_myTarget.LineRenderers == null) {
                _myTarget.LineRenderers = new List<BezierLine>();
            }
            
            SubscribeEvents();
        }

        private void OnDisable() {
            UnsubscribeEvents();
        }

        #region DrawGUI

        private void OnSceneGUI() {
            DrawModeGui();
            ModeHandler();
            EventHandler();
            if (Event.current.type == EventType.MouseMove) SceneView.RepaintAll();
        }

        public override void OnInspectorGUI() {
            DrawLevelGuiControls();
            DrawLineSelectedGui();
            DrawDotSelectedGui();
            DrawInspectedItemGui();
            DrawLineRendersGui();
            
            if (GUI.changed && _myTarget != null) {
                EditorUtility.SetDirty(_myTarget);
            }
        }

        private void DrawLevelGuiControls() {
            EditorGUILayout.BeginVertical();
            _mySerializedObject.Update();

            _myTarget.figureName = EditorGUILayout.TextField("Figure name:", string.IsNullOrEmpty(_myTarget.figureName) ? "Stage" : _myTarget.figureName);
            _myTarget.figureId = EditorGUILayout.IntField("Figure ID: ", Mathf.Max(1, _myTarget.figureId));

            EditorGUILayout.BeginHorizontal();
            bool buttonSave =
                GUILayout.Button("Save", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonSave) {
                if (EditorUtility.DisplayDialog("Save figure",
                    "Do you really want to save this figure?", "Yes",
                    "No")) {
                    SaveFigure();
                    GUIUtility.ExitGUI();
                } else {
                    GUIUtility.ExitGUI();  
                }
            } 
        
            bool buttonLoad =
                GUILayout.Button("Load", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonLoad) {
                if (EditorUtility.DisplayDialog("Loading figure",
                        "Do you really want to load a figure?\n Make sure you save your work. This action can't be cancelled.", "Yes",
                        "No")) {
                    ClearFigure();
                    LoadFigure();
                    GUIUtility.ExitGUI();
                } else {
                    GUIUtility.ExitGUI();  
                }
            } 
            
            bool buttonClear =
                GUILayout.Button("Очистить", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonClear) {
                if (EditorUtility.DisplayDialog("Figure clearing",
                    "Do you really want to clean up current figure?\n This action can't be cancelled.", "Yes",
                    "No")) {
                    ClearFigure();
                    GUIUtility.ExitGUI();
                } else {
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawLineSelectedGui()
        {
            EditorGUILayout.LabelField("Line Selected", EditorStyles.boldLabel);
            if (_selectedBezierLine == null)
            {
                EditorGUILayout.HelpBox("Line not selected!", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(new GUIContent(AssetPreview.GetAssetPreview(_selectedBezierLine.gameObject)), GUILayout.Height(40));
                EditorGUILayout.LabelField(_selectedBezierLine.name);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDotSelectedGui() {
            if (_itemSelected == null || _currentFigureConstructorModes == FigureConstructorModes.Move)
            {
                return;
            }
            
            EditorGUILayout.LabelField("Dot Selected", EditorStyles.boldLabel);

            if (_pieceSelected == null) {
                EditorGUILayout.HelpBox("No dot selected!", MessageType.Info);
            } else {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(new GUIContent(_itemPreview), GUILayout.Height(40));
                EditorGUILayout.LabelField(_itemSelected.itemName);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawInspectedItemGui() {
            if(_currentFigureConstructorModes != FigureConstructorModes.Move) {
                return;
            }
            
            EditorGUILayout.LabelField ("Dot Edited", EditorStyles.boldLabel);
            if (_itemInspected != null) {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Name: " + _itemInspected.name);
                CreateEditor(_itemInspected).OnInspectorGUI();
                EditorGUILayout.EndVertical();
            } else {
                EditorGUILayout.HelpBox("No dot to move!", MessageType.Info);
            }
        }

        private void DrawLineRendersGui()
        {
            SerializedProperty lineRenderersProperty = _mySerializedObject.FindProperty("_lineRenderers");

            int deletedLineIndex = -1;
            
            for (int i = 0; i < lineRenderersProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(lineRenderersProperty.GetArrayElementAtIndex(i));

                bool selectLineRenderer = GUILayout.Button("select", GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (selectLineRenderer)
                {
                    SelectBezierLineForEditing(i);
                } 
                
                bool deleteLineRenderer = GUILayout.Button("delete", GetRedGuyStile(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (deleteLineRenderer)
                {
                    deletedLineIndex = i;
                } 
                
                EditorGUILayout.EndHorizontal();
                if (_selectedBezierLine != null && _selectedBezierLine == _myTarget.LineRenderers[i])
                {
                    ShowDotGUI(_selectedBezierLine);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(1f);
            }

            if (_selectedBezierLine != null)
            {
                bool buttonShowAllLines = GUILayout.Button("Show all lines", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));
                if (buttonShowAllLines)
                {
                    ActivateAllLines();
                    _selectedBezierLine = null;
                }
            }

            bool buttonAdd = GUILayout.Button("Add new line", GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (buttonAdd)
            {
                BezierLine newLinePrefab = BezierLineFactory.InstantiateNewLinePrefab((0, 0), _myTarget.transform);
                if (newLinePrefab != null)
                {
                    _myTarget.LineRenderers.Add(newLinePrefab);
                }
                
                Repaint();
                EditorUtility.SetDirty(_myTarget);
            }

            if (deletedLineIndex >= 0)
            {
                BezierLine myLine = _myTarget.LineRenderers[deletedLineIndex];
                _myTarget.LineRenderers.RemoveAt(deletedLineIndex);
                _selectedBezierLine = null;
                ActivateAllLines();
                DestroyImmediate(myLine.gameObject);
                deletedLineIndex = -1;
            }
            
            _mySerializedObject.Update();
            _mySerializedObject.ApplyModifiedProperties();
        }

        private void ShowDotGUI(BezierLine bezierLine)
        {
            int deletedIndex = -1;
            int selectedIndex = -1;
            for (int index = 0; index < bezierLine.Dots.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Dot line {index}  ");
                
                bool selectDot = GUILayout.Button("select dot", GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (selectDot)
                {
                    selectedIndex = index;
                }
                
                bool deleteDot = GUILayout.Button("delete dot", GetRedGuyStile(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (deleteDot)
                {
                    deletedIndex = index;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (selectedIndex >= 0)
            {
                bezierLine.ChangeDotColors(ColorTypes.Normal);
                bezierLine.Dots[selectedIndex].ApplyColor(ColorTypes.Selected);
                selectedIndex = -1;
            }
            
            if (deletedIndex >= 0)
            {
                IBezierLinePart deletedDot = bezierLine.Dots[deletedIndex];
                bezierLine.DeletePoint(deletedIndex);
                DestroyImmediate(deletedDot.GameObject);
                deletedIndex = -1;
            }
            
            EditorUtility.SetDirty(_myTarget);
        }

        private void SelectBezierLineForEditing(int i)
        {
            ActivateAllLines(false);
            _selectedBezierLine = _myTarget.LineRenderers[i];
            _selectedBezierLine.ChangeDotsAlpha(true);
            if (_selectedBezierLine.Dots.Count == 0 && _selectedBezierLine.transform.childCount > 0)
            {
                foreach (IBezierLinePart bezierLinePart in _selectedBezierLine.transform.GetComponentsInChildren<IBezierLinePart>())
                {
                    _selectedBezierLine.Dots.Add(bezierLinePart);
                }
            }
        }

        private void ActivateAllLines(bool show = true)
        {
            foreach (BezierLine lineRenderer in _myTarget.LineRenderers)
            {
                lineRenderer.ChangeDotsAlpha(show);
            }
        }

        private void DrawModeGui() {
            List<FigureConstructorModes> modes = EditorUtils.GetListFromEnum<FigureConstructorModes>();
            List<string> modeLabels = new List<string>();
            foreach(FigureConstructorModes mode in modes) {
                modeLabels.Add(mode.ToString());
            }
        
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10f, 10f, 300, 30f));
            _selectedFigureConstructorModes = (FigureConstructorModes) GUILayout.Toolbar(
                (int) _currentFigureConstructorModes,
                modeLabels.ToArray(),
                GUILayout.ExpandHeight(true));
            GUILayout.EndArea();
            Handles.EndGUI();
        }
        
        #endregion

        #region Event Handlers And Subscriptions
        
        private void SubscribeEvents() {
            TraceRedactorWindow.ItemSelectedEvent += new TraceRedactorWindow.ItemSelectedDelegate(UpdateCurrentPieceInstance);
        }

        private void UnsubscribeEvents() {
            TraceRedactorWindow.ItemSelectedEvent -= new TraceRedactorWindow.ItemSelectedDelegate(UpdateCurrentPieceInstance);
        }

        private void UpdateCurrentPieceInstance(TraceRedactorItem item, Texture2D preview) {
            _itemSelected = item;
            _itemPreview = preview;
            _pieceSelected = item.gameObject;
            Repaint();
        }
        
        private void ModeHandler () {
            switch (_selectedFigureConstructorModes) {
                case FigureConstructorModes.Paint:
                case FigureConstructorModes.Move:
                    Tools.current = Tool.None;
                    break;
                case FigureConstructorModes.View:
                default:
                    Tools.current = Tool.View;
                    break;
            }
        
            if(_selectedFigureConstructorModes != _currentFigureConstructorModes) {
                _currentFigureConstructorModes = _selectedFigureConstructorModes;
                _itemInspected = null;
                _bezierDotLocked = false;
                Repaint();
            }

            _myTarget.isHillSelected = false;
            
            if (_currentFigureConstructorModes == FigureConstructorModes.Paint) {
                Tools.current = Tool.None;
            }
        
            SceneView.currentDrawingSceneView.in2DMode = true;
        }
    
        private void EventHandler() {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            mousePosition.z = 0;
        
            Vector3 gridPos = mousePosition.WorldToGridVectorCoordinates();
            int col = (int) gridPos.x;
            int row = (int) gridPos.y;

            switch(_currentFigureConstructorModes) {
                case FigureConstructorModes.Paint:
                    if(Event.current.type == EventType.MouseDown) {
                        Paint(col, row);
                    }
                    break;
                case FigureConstructorModes.Move:
                        MoveDots(GridExtensions.IsInsideGridBounds(row, col));
                    break;
                case FigureConstructorModes.View:
                default:
                    break;
            }
        }
        
        #endregion

        #region Draw Dot Methods

        private void Paint(int col, int row) {
            if (!GridExtensions.IsInsideGridBounds(col,row) || _pieceSelected == null) {
                return;
            }
            
            BezierLineFactory.InstantiateDotPrefab((col, row), _pieceSelected, _selectedBezierLine,  _myTarget.transform.GridToWorldCoordinates(col, row));
            Repaint();
            EditorUtility.SetDirty(_myTarget);
        }
        
        private void MoveDots(bool isInsideGridBounds)
        {
            if (_currentFigureConstructorModes != FigureConstructorModes.Move)
            {
                return;
            }

            if (_bezierDotLocked)
            {
                CheckMouseDrag(isInsideGridBounds);
                return;
            }

            if (RaycastSceneObjects(out RaycastHit raycastHit))
            {
                Collider collider = raycastHit.collider;
                if (collider.tag.Equals("LinePoint"))
                {
                    if (_selectedBezierDot != collider.transform)
                    {
                        _selectedBezierDot = collider.transform;
                        collider.GetComponent<SprColorChanger>()?.ApplyColor(ColorTypes.Selected);
                        _dotPositionBeforeDrag = collider.transform.position;
                        CheckMouseDrag(isInsideGridBounds);
                    }
                    else
                    {
                        CheckMouseDrag(isInsideGridBounds);
                    }
                }
            }
            else
            {
                foreach (BezierLine bezierLine in _myTarget.LineRenderers)
                {
                    bezierLine.ChangeDotColors(ColorTypes.Normal);
                }

                _selectedBezierDot = null;
            }
        }

        private void CheckMouseDrag(bool isInsideGridBounds)
        {
            if (_selectedBezierLine == null)
            {
                return;
            }
            
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    _bezierDotLocked = true;
                    _itemInspected = _selectedBezierDot.gameObject;
                    EditorUtility.SetDirty(_myTarget);
                    break;
                case EventType.MouseUp:
                    _bezierDotLocked = false;
                    _itemInspected = null;
                    CorrectDotPositionAfterDrag(isInsideGridBounds);
                    break;
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    if (_bezierDotLocked)
                    {
                        Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                        mousePosition.z = 0;
                        _selectedBezierDot.transform.position = mousePosition;
                        _selectedBezierLine.UpdateLineWithFingerPoint();
                    }
                    break;
                default:
                    break;
            }
        }

        private void CorrectDotPositionAfterDrag(bool isInsideGridBounds)
        {
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            mousePosition.z = 0;
            _selectedBezierDot.position = isInsideGridBounds
                ? mousePosition.WorldToGridVectorCoordinatesCentered()
                : _dotPositionBeforeDrag;
            _selectedBezierLine.UpdateLineWithFingerPoint();
            EditorUtility.SetDirty(_myTarget);
        }

        #endregion

        private void ClearFigure() {
            foreach (BezierLine bezierLine in _myTarget.LineRenderers) {
                if (bezierLine != null) {
                    DestroyImmediate(bezierLine.gameObject);
                }
            }
            
            _myTarget.LineRenderers.Clear();
        }

        private void SaveFigure() => SaveLoadDataClass.SaveFigureWithDialog(_myTarget.LineRenderers);

        private void LoadFigure() {
            if (!SaveLoadDataClass.TryLoadFigureWithDialog(out BezierFigureData bezierFigureData))
            {
                return;
            }
            
            ClearFigure();
            _currentFigureConstructorModes = FigureConstructorModes.Paint;
            
            for (int index = 0; index < bezierFigureData.BezierLinesData.Count; index++)
            {
                BezierDotsData lineData = bezierFigureData.BezierLinesData[index];
                BezierLine newLinePrefab = BezierLineFactory.InstantiateNewLinePrefab(lineData.FirstDot.FromSerializedVector().WorldToGridCoordinates(), _myTarget.transform);
                _myTarget.LineRenderers.Add(newLinePrefab);
                _selectedBezierLine = _myTarget.LineRenderers[index];
                for (int i = 0; i < lineData.LineDots.Count(); i++)
                {
                    Vector3 dotPos = lineData.LineDots[i].FromSerializedVector();
                    Vector3 bezierControlDotPos = lineData.BezierControlDots[i].FromSerializedVector();
                    (int row, int col) coords = dotPos.WorldToGridCoordinates();
                    BezierLineFactory.InstantiateDotPrefab(coords, _pieceSelected, _selectedBezierLine,  _myTarget.transform.GridToWorldCoordinates(coords.col, coords.row));
                    _selectedBezierLine.Dots[i].SetBezierControlPointPosition(bezierControlDotPos);
                }
            }   
                
            _currentFigureConstructorModes = FigureConstructorModes.View;
            Resources.UnloadUnusedAssets();
            Repaint();
            EditorUtility.SetDirty(_myTarget);
        }

        private GUIStyle GetRedGuyStile()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = new Color(1f, 0.48f, 0.53f);
            buttonStyle.fontStyle = FontStyle.Bold;
            return buttonStyle;
        }

        private bool RaycastSceneObjects(out RaycastHit hit)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            object raycastResult = HandleUtility.RaySnap(ray);
            if( raycastResult != null && raycastResult is RaycastHit result)
            {
                hit = result;
                return true;
            }
 
            hit = new RaycastHit();
            return false;
        }
    }
}