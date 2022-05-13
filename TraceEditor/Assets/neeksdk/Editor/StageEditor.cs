using System.Collections.Generic;
using neeksdk.Scripts.LevelCreator;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stage))]
public class StageEditor : UnityEditor.Editor {
   private Stage _myTarget;

   private void OnEnable() {
      _myTarget = (Stage) target;
   }

   public override void OnInspectorGUI() {
      DrawDefaultInspector();
      DrawStageGUI();
   }

   private void DrawStageGUI() {
      bool editStageButton =
         GUILayout.Button("Открыть редактор уровня", GUILayout.Height(2 * EditorGUIUtility.singleLineHeight));

      if (editStageButton) {
         List<FigureConstructor> lsc = EditorUtils.GetAssetsWithScript<FigureConstructor>("Assets/Prefabs/StageConstructor");
         FigureConstructor myFigureConstructor = lsc[0];
         
         GameObject obj = PrefabUtility.InstantiatePrefab(myFigureConstructor.gameObject) as GameObject;
         int stage = _myTarget.myStageInfo;

         if (obj != null) {
            obj.name = $"Stage Constructor (stage: {stage})";
            FigureConstructor figureConst = obj.GetComponent<FigureConstructor>();
            figureConst.stageId = stage;
            PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Selection.activeGameObject = obj;
         }

         SceneView.lastActiveSceneView.FrameSelected();
         _myTarget.transform.parent.parent.gameObject.SetActive(false);
      }
   }
}
