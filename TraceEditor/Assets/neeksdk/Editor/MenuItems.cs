using neeksdk.Scripts.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace neeksdk.Editor
{
    public static class MenuItems
    {
        private const string FIGURE_REDACTOR_SCENE = "Assets/neeksdk/Scenes/FigureEditorScene.unity";
        private const string TEST_SCENE = "Assets/neeksdk/Scenes/TestScene.unity";
        
        [MenuItem("Lalabook/Open Figure Editor %#z", priority = 0)]
        private static void OpenFigureRedactor()
        {
            EditorSceneManager.OpenScene(FIGURE_REDACTOR_SCENE, OpenSceneMode.Single);
            Selection.activeGameObject = GameObject.FindGameObjectWithTag("FigureConstructor");
            
            Vector3 position = SceneView.lastActiveSceneView.pivot;
            position.x = RedactorConstants.REDACTOR_WIDTH / 2f;
            position.y = RedactorConstants.REDACTOR_HEIGHT / 2f;
            SceneView.lastActiveSceneView.pivot = position;
            SceneView.lastActiveSceneView.size = (RedactorConstants.REDACTOR_WIDTH + RedactorConstants.REDACTOR_HEIGHT) / 2f;
            SceneView.lastActiveSceneView.Repaint();
            
            ShowTraceRedactor();
        }
        
        [MenuItem("Lalabook/Open Test Scene %#c", priority = 1)]
        private static void OpenTestScene()
        {
            EditorSceneManager.OpenScene(TEST_SCENE, OpenSceneMode.Single);
        }
        
        [MenuItem("Lalabook/Show Bezier Line Dots Pallete %#x", priority = 2)]
        private static void ShowTraceRedactor() {
            TraceRedactorWindow.ShowTraceRedactor();
        }
        
        
    }
}
