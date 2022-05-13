using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using neeksdk.Scripts.Configs;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.LevelCreator;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using neeksdk.Scripts.Properties;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace neeksdk.Editor {
    [CustomEditor(typeof(FigureConstructor))]
    public class FigureConstructorEditorScript : UnityEditor.Editor {
        private FigureConstructor _myTarget;
        private SerializedObject _mySerializedObject;
        private TraceRedactorItem _itemSelected;
        private TraceRedactorItem _itemInspected;
        private Texture2D _itemPreview;
        private GameObject _pieceSelected;
        private int _originalPosX;
        private int _originalPosY;
        
        private BezierLineConfig _bezierLineConfig;
        private BezierLine _selectedBezierLine;
        private Transform _selectedBezierDot;

        private enum Mode {
            View,
            Paint,
            Move,
            Erase
        }

        private Mode _selectedMode;
        private Mode _currentMode;

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
        
        private void OnEnable() {
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
            DetectDots();
            if (Event.current.type == EventType.MouseMove) SceneView.RepaintAll();
        }

        public override void OnInspectorGUI() {
            DrawLevelGuiControls();
            DrawPieceSelectedGui();
            DrawInspectedItemGui();
            DrawLineRendersGui();
            
            if (GUI.changed && _myTarget != null) {
                EditorUtility.SetDirty(_myTarget);
            }
        }

        private void DrawLevelGuiControls() {
            EditorGUILayout.BeginVertical();
            _mySerializedObject.Update();

            _myTarget.stageName = EditorGUILayout.TextField("Stage name:", string.IsNullOrEmpty(_myTarget.stageName) ? "Stage" : _myTarget.stageName);
            _myTarget.stageId = EditorGUILayout.IntField("Stage ID: ", Mathf.Max(1, _myTarget.stageId));

            EditorGUILayout.BeginHorizontal();
            bool buttonSave =
                GUILayout.Button("Сохранить", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonSave) {
                if (EditorUtility.DisplayDialog("Инструмент сохранения уровня",
                    "Вы действительно хотите сохранить текущий уровень?", "Да",
                    "Нет")) {
                    SaveStage();
                    GUIUtility.ExitGUI();
                } else {
                    GUIUtility.ExitGUI();  
                }
            } 
        
            bool buttonLoad =
                GUILayout.Button("Загрузить", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonLoad) {
                if (EditorUtility.DisplayDialog("Инструмент загрузки уровня",
                    "Вы действительно хотите загрузить уровень?\n Убедитесь, что сохранили свою работу. Это действие нельзя отменить.", "Да",
                    "Нет")) {
                    ClearStage();
                    LoadStage(_myTarget.stageId);
                    GUIUtility.ExitGUI();
                } else {
                    GUIUtility.ExitGUI();  
                }
            } 
            
            bool buttonClear =
                GUILayout.Button("Очистить", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonClear) {
                if (EditorUtility.DisplayDialog("Инструмент очистки уровня",
                    "Вы действительно хотите очистить уровень и удалить все объекты?\n Это действие нельзя отменить.", "Да",
                    "Нет")) {
                    ClearStage();
                    GUIUtility.ExitGUI();
                } else {
                    GUIUtility.ExitGUI();
                }
            } 
        
            bool buttonClose =
                GUILayout.Button("Закрыть", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

            if (buttonClose) {
                CloseStageConstructor();
                GUIUtility.ExitGUI();
            }
        
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawPieceSelectedGui() {
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
            if(_currentMode != Mode.Move) {
                return;
            }
            
            EditorGUILayout.LabelField ("Dot Edited", EditorStyles.boldLabel);
            if (_itemInspected != null) {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Name: " + _itemInspected.name);
                CreateEditor(_itemInspected.inspectedScript).OnInspectorGUI();
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
                
                bool deleteLineRenderer = GUILayout.Button("delete", GUILayout.Height(EditorGUIUtility.singleLineHeight));
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
            
            bool buttonAdd = GUILayout.Button("add new line", GUILayout.Height(EditorGUIUtility.singleLineHeight));
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
            for (int index = 0; index < bezierLine.Dots.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                IBezierLinePart bezierLinePart = bezierLine.Dots[index];
                EditorGUILayout.LabelField($"Dot line {index}  ");
                
                bool selectDot = GUILayout.Button("select dot", GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (selectDot)
                {
                    //todo: add dot selection to editor
                }
                
                bool deleteDot = GUILayout.Button("delete dot", GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (deleteDot)
                {
                    deletedIndex = index;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (deletedIndex >= 0)
            {
                IBezierLinePart deletedDot = _selectedBezierLine.Dots[deletedIndex];
                _selectedBezierLine.DeletePoint(deletedIndex);
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
            List<Mode> modes = EditorUtils.GetListFromEnum<Mode>();
            List<string> modeLabels = new List<string>();
            foreach(Mode mode in modes) {
                modeLabels.Add(mode.ToString());
            }
        
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10f, 10f, 300, 30f));
            _selectedMode = (Mode) GUILayout.Toolbar(
                (int) _currentMode,
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
            switch (_selectedMode) {
                case Mode.Paint:
                case Mode.Move:
                case Mode.Erase:
                    Tools.current = Tool.None;
                    break;
                case Mode.View:
                default:
                    Tools.current = Tool.View;
                    break;
            }
        
            if(_selectedMode != _currentMode) {
                _currentMode = _selectedMode;
                _itemInspected = null;
                Repaint();
            }

            _myTarget.isHillSelected = false;
            
            if (_currentMode == Mode.Paint) {
                Tools.current = Tool.None;
            }
        
            SceneView.currentDrawingSceneView.in2DMode = true;
        }
    
        private void EventHandler() {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
            Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            mousePosition.z = 0;
        
            Vector3 gridPos = _myTarget.WorldToGridCoordinates(mousePosition);
            int col = (int) gridPos.x;
            int row = (int) gridPos.y;
            //Debug.LogFormat("GridPos {0},{1}", col, row);
        
            switch(_currentMode) {
                case Mode.Paint:
                    if(Event.current.type == EventType.MouseDown ||
                       Event.current.type == EventType.MouseDrag) {
                        Paint(col, row);
                    }
                    break;
                case Mode.Move:
                    if (Event.current.type == EventType.MouseDown) {
                        Edit(col, row);
                        _originalPosX = col;
                        _originalPosY = row;
                    }

                    if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore) {
                        if (_itemInspected != null) {
                            MoveTile();
                        }
                    }

                    if (_itemInspected != null) {
                        Transform iit = _itemInspected.transform;
                        iit.position = Handles.FreeMoveHandle(iit.position,
                            iit.rotation, 0.5f, 0.5f * Vector3.one, Handles.RectangleHandleCap);
                    }
                    break;
                case Mode.Erase:
                    if(Event.current.type == EventType.MouseDown ||
                       Event.current.type == EventType.MouseDrag) {
                        EraseTile(col, row);
                    }
                    break;
                case Mode.View:
                    break;
                default:
                    break;
            }
        }
        
        #endregion

        #region Draw Tile Methods

        private static int RandomizeNums(int cNum, int lRange, int rRange) {
            int randomNum = Random.Range(lRange, rRange);
            while (cNum == randomNum) {
                randomNum = Random.Range(lRange, rRange);
            }
            
            return randomNum;
        }

        private void Paint(int col, int row) {
            if (!_myTarget.IsInsideGridBounds(col,row) || _pieceSelected == null) {
                return;
            }

            /*int pieceNum = col + row * RedactorConstants.REDACTOR_WIDTH;
            if (_myTarget.LineRenderers[pieceNum] != null) {
                DestroyImmediate(_myTarget.LineRenderers[pieceNum].gameObject);
            }*/
            
            InstantiateDotPrefab(col, row);
        }
    
        private void Edit(int col, int row) {
            if (!_myTarget.IsInsideGridBounds(col, row)) {
                _itemInspected = null;
            } else {
                if (_myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH] == null) {
                    _itemInspected = null;
                } else {
                    if (_itemInspected != null) {
                        //_itemInspected = _myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH].GetComponent<TraceRedactorItem>() as TraceRedactorItem;
                    }
                }
            }
            Repaint();
        }

        private void MoveTile() {
            
            Vector3 gridPoint = _myTarget.WorldToGridCoordinates(_itemInspected.transform.position);
            int col = (int) gridPoint.x;
            int row = (int) gridPoint.y;
            if(col == _originalPosX && row == _originalPosY) {
                return;
            }
            if(!_myTarget.IsInsideGridBounds(col,row) || _myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH] != null) {
                _itemInspected.transform.position = _myTarget.GridToWorldCoordinates( _originalPosX, _originalPosY);
            } else {
                _myTarget.LineRenderers[ _originalPosX + _originalPosY * RedactorConstants.REDACTOR_WIDTH] = null;
                //_myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH] = _itemInspected.GetComponent<LineRenderer>();
                //_myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH].transform.position = _myTarget.GridToWorldCoordinates(col,row);
            }
        }

        private void EraseTile(int col, int row) {
            if (!_myTarget.IsInsideGridBounds(col, row)) {
                return;
            }

            /*LineRenderer targetPoint = _myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH];
            if (targetPoint != null) {
                DestroyImmediate(targetPoint.gameObject);
            }*/
        }
        
        #endregion

        private void ClearStage() {
            /*foreach (LineRenderer spt in _myTarget.LineRenderers) {
                if (spt != null) {
                    DestroyImmediate(spt.gameObject);
                }
            }
            
            _myTarget.LineRenderers = new List<LineRenderer>();*/
        }

        private void SaveStage() {
            //StageSettings data = new StageSettings();
            /*for (int j = 0; j < _myTarget.LineRenderers.Count; j++) {
                if (_myTarget.LineRenderers[j] != null) {
                    LinePoint sp = _myTarget.LineRenderers[j].GetComponent<LinePoint>();
                    data.PostTile(j, sp.tileNum, sp.PointType);
                } else {
                    data.PostTile(j);
                }
            }*/
        
            string destination = Application.streamingAssetsPath + $"/StageAssets/{_myTarget.stageName}_{_myTarget.stageId:000}.dat";
            FileStream file;

            if (File.Exists(destination)) {
                file = File.OpenWrite(destination);
            } else {
                file = File.Create(destination);
            }
 
            BinaryFormatter bf = new BinaryFormatter();
            //bf.Serialize(file, data);
            
            file.Close();

            CloseStageConstructor();
        }

        private void LoadStage(int stage) {
            string path = Application.streamingAssetsPath + $"/StageAssets/Stage_{stage:000}.dat";
            
            Debug.Log($"stage: {stage}, exists: {File.Exists(path)}");
            
            if (!File.Exists(path)) return;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            //StageSettings data = (StageSettings) bf.Deserialize(file);
                
            //List<LinePoint> spl = EditorUtils.GetAssetsWithScript<LinePoint>("Assets/Prefabs/StagePrefabs");
            
            //Debug.Log($"Found pieces: {spl.Count}");
                
            /*for (int j = 0; j < data.stageTiles.Length; j++) {
                if (data.stageTiles[j].myTileNum != -1) {
                   
                    foreach (LinePoint sp in spl) {
                        if (data.stageTiles[j].myTileNum == sp.tileNum && data.stageTiles[j].myPieceType == sp.PointType) {
                            GameObject obj = PrefabUtility.InstantiatePrefab(sp.gameObject) as GameObject;
                                
                            if (obj != null) {
                                obj.transform.parent = _myTarget.transform;

                                int row = Mathf.FloorToInt(j / (float)RedactorConstants.REDACTOR_WIDTH);
                                int col = j - row * RedactorConstants.REDACTOR_WIDTH;

                                obj.name = $"[{col},{row}][{obj.name}]";
                                obj.transform.position = _myTarget.GridToWorldCoordinates(col, row);

                                _myTarget.LineRenderers[col + row * RedactorConstants.REDACTOR_WIDTH] = obj.GetComponent<LineRenderer>();
                            }

                            break;
                        }
                    }
                }
            }*/
            file.Close();
            Resources.UnloadUnusedAssets();
            Repaint();
            EditorUtility.SetDirty(_myTarget);
        }

        private void InstantiateDotPrefab(int col, int row) {
            if (_selectedBezierLine == null)
            {
                //todo: inform user about selection line first
                return;
            }
            
            GameObject go = PrefabUtility.InstantiatePrefab(_pieceSelected) as GameObject;
            if (go == null)
            {
                return;
            }
            
            go.transform.parent = _selectedBezierLine.transform;
            go.name = $"[{col},{row}][{go.name}]";
            Vector3 tilePosition = _myTarget.GridToWorldCoordinates(col, row);
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
            Vector3 tilePosition = _myTarget.GridToWorldCoordinates(col, row);
            go.transform.position = Vector3.zero;
            
            BezierLine bezierLine = go.GetComponent<BezierLine>();
            bezierLine.StartPointTransform.position = tilePosition;
            _myTarget.LineRenderers.Add(bezierLine);
        }

        private void CloseStageConstructor() =>
            SceneView.lastActiveSceneView.FrameSelected();

        
        private void DetectDots()
        {
            if (_currentMode != Mode.Move)
            {
                return;
            }

            if (RaycastSceneObjects(out RaycastHit raycastHit))
            {
                Collider collider = raycastHit.collider;
                if (collider.tag.Equals("LinePoint") && _selectedBezierDot != collider.transform)
                {
                    _selectedBezierDot = collider.transform;
                    collider.GetComponent<SprColorChanger>()?.ApplyColor(ColorTypes.Selected);
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