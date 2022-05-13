using UnityEngine;

namespace neeksdk.Scripts.LevelCreator
{
    public class TraceRedactorItem : MonoBehaviour {
#if UNITY_EDITOR
        public enum Category {
            Lines,
            LineDots
        }

        public Category category = Category.Lines;
        public string itemName = "";
        public Object inspectedScript;
#endif
    }
}
