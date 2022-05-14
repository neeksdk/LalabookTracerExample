﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using neeksdk.Scripts.Configs;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.Extensions;
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
        private int _originalPosX;
        private int _originalPosY;
        
        private BezierLineConfig _bezierLineConfig;
        private BezierLine _selectedBezierLine;
        private Transform _selectedBezierDot;
        private bool _bezierDotLocked;
        private Vector3 _dotPositionBeforeDrag;

        private enum FigureConstructorModes {
            View,
            Paint,
            Move
        }

        private FigureConstructorModes _selectedFigureConstructorModes;
        private FigureConstructorModes _currentFigureConstructorModes;

        private BezierLineConfig BezierLineConfig
        {
            get
            {
                if (_bezierLineConfig == null)
                {
                    _bezierLineConfig = AssetDatabase.LoadAssetAtPath<BezierLineConfig>(Path.Combine(RedactorConstants.CONFIGS_PATH, RedactorConstants.BEZIER_CONFIG));
                }

                return _bezierLineConfig;
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
            if (_myTarget.LineRenderers.Count == 0)
            {
                return;
            }
            
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
                    LoadStage(_myTarget.figureId);
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
                InstantiateNewLinePrefab(0, 0);
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
                    GUIUtility.ExitGUI();
                }
                
                bool deleteDot = GUILayout.Button("delete dot", GetRedGuyStile(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (deleteDot)
                {
                    deletedIndex = index;
                    GUIUtility.ExitGUI();
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
            _selectedBezierLine.gameObject.SetActive(true); 
        }

        private void ActivateAllLines(bool show = true)
        {
            foreach (BezierLine lineRenderer in _myTarget.LineRenderers)
            {
                lineRenderer.gameObject.SetActive(show);
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
        
            Vector3 gridPos = mousePosition.WorldToGridCoordinates();
            int col = (int) gridPos.x;
            int row = (int) gridPos.y;

            switch(_currentFigureConstructorModes) {
                case FigureConstructorModes.Paint:
                    if(Event.current.type == EventType.MouseDown) {
                        Paint(col, row);
                    }
                    break;
                case FigureConstructorModes.Move:
                        MoveDots();
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
            
            InstantiateDotPrefab(col, row);
        }
        
        private void MoveDots()
        {
            if (_currentFigureConstructorModes != FigureConstructorModes.Move)
            {
                return;
            }

            if (_bezierDotLocked)
            {
                CheckMouseDrag();
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
                        CheckMouseDrag();
                    }
                    else
                    {
                        CheckMouseDrag();
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

        private void CheckMouseDrag()
        {
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
                    CorrectDotPositionAfterDrag();
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

        private void CorrectDotPositionAfterDrag()
        {
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            mousePosition.z = 0;
            _selectedBezierDot.position = mousePosition.IsInsideGridBounds()
                ? mousePosition.WorldToGridCoordinates()
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

        private void SaveFigure()
        {
            BezierFigureData bezierFigureData = new BezierFigureData();
            for (int i = 0; i < _myTarget.LineRenderers.Count; i++)
            {
                BezierLine bezierLine = _myTarget.LineRenderers[i];
                if (bezierLine != null)
                {
                    int dotsCount = bezierLine.Dots.Count;
                    BezierDotsData dotsData = new BezierDotsData();
                    dotsData.LineDots = new SerializedVectorData[dotsCount];
                    dotsData.BezierControlDots = new SerializedVectorData[dotsCount];
                    bezierFigureData.FirstDot = bezierLine.StartPointTransform.position.ToSerializedVector();
                    for (int j = 0; j < dotsCount; j++)
                    {
                        IBezierLinePart linePart = bezierLine.Dots[j];
                        dotsData.LineDots[j] = linePart.GetLineDotPosition.ToSerializedVector();
                        dotsData.BezierControlDots[j] = linePart.GetBezierControlDotPosition.ToSerializedVector();
                    }
                }
            }

            string destination = EditorUtility.OpenFilePanel("Select file to save", Path.Combine(Application.streamingAssetsPath, "FigureAssets"), "dat");
            FileStream file;

            if (File.Exists(destination)) {
                file = File.OpenWrite(destination);
            } else {
                file = File.Create(destination);
            }
 
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, bezierFigureData);
            file.Close();
        }

        private void LoadStage(int stage) {
            
            
            
            string path = Application.streamingAssetsPath + $"/FigureAssets/Stage_{stage:0000}.dat";
            
            Debug.Log($"stage: {stage}, exists: {File.Exists(path)}");
            
            if (!File.Exists(path)) return;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            file.Close();
            Resources.UnloadUnusedAssets();
            Repaint();
            EditorUtility.SetDirty(_myTarget);
        }

        private void InstantiateDotPrefab(int col, int row) {
            if (_selectedBezierLine == null)
            {
                return;
            }
            
            GameObject go = PrefabUtility.InstantiatePrefab(_pieceSelected) as GameObject;
            if (go == null)
            {
                return;
            }
            
            go.transform.parent = _selectedBezierLine.transform;
            go.name = $"[{col},{row}][{go.name}]";
            Vector3 tilePosition = _myTarget.transform.GridToWorldCoordinates(col, row);
            go.transform.position = Vector3.zero;
            
            IBezierLinePart bezierLineDot = go.GetComponent<IBezierLinePart>();
            bezierLineDot.SetLinePointPosition(tilePosition);
            Vector3 lastDotPosition = _selectedBezierLine.StartPointTransform.position;
            if (_selectedBezierLine.Dots.Count != 0)
            {
                int lastDotIndex = _selectedBezierLine.Dots.Count - 1;
                lastDotPosition = _selectedBezierLine.transform.TransformPoint(_selectedBezierLine.Dots[lastDotIndex].GetLineDotPosition);
            }
            
            Vector3 centerPos = (lastDotPosition + tilePosition) / 2;
            bezierLineDot.SetBezierControlPointPosition(centerPos);
            _selectedBezierLine.AddPoint(bezierLineDot);
        }

        private void InstantiateNewLinePrefab(int col, int row)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(BezierLineConfig.DefaultBezierLinePrefab.gameObject) as GameObject;
            if (go == null)
            {
                return;
            }
            
            go.transform.parent = _myTarget.transform;
            go.name = $"[{col},{row}][{go.name}]";
            Vector3 tilePosition = _myTarget.transform.GridToWorldCoordinates(col, row);
            go.transform.position = Vector3.zero;
            
            BezierLine bezierLine = go.GetComponent<BezierLine>();
            bezierLine.StartPointTransform.position = tilePosition;
            _myTarget.LineRenderers.Add(bezierLine);
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