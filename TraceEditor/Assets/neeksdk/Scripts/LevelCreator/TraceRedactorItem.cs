using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    public class TraceRedactorItem : MonoBehaviour {
#if UNITY_EDITOR
        public enum Category {
            GizmoLines
        }

        public Category category = Category.GizmoLines;
        public string itemName = "";
        public Object inspectedScript;
#endif
    }
}
