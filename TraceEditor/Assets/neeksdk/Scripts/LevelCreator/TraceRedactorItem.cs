using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    public class TraceRedactorItem : MonoBehaviour {
#if UNITY_EDITOR
        public enum Category {
            LineDots
        }

        public Category category = Category.LineDots;
        public string itemName = "";
        public Object inspectedScript;
#endif
    }
}
