using UnityEditor;

namespace neeksdk.Editor
{
    public static class MenuItems {
        [MenuItem("Lalabook/Show Trace Redactor %#z")]
        private static void ShowTraceRedactor() {
            TraceRedactorWindow.ShowTraceRedactor();
        }
    }
}
