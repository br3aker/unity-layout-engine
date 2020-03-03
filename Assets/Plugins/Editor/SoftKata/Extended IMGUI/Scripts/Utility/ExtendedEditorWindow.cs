using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        private Vector2 _windowScroll;

        private void OnEnable(){
            ExtendedEditorGUI.InitEditorWindow(this);
            Initialize();
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;
            if (Event.current.type == EventType.Layout) LayoutEngine.ResetEngine();

            // ALWAYS use -1 as horizontal size for automatic layout
            // EditorWindow.position.width is 2 pixels lesser than EditorGUIUtility.currentViewWidth
            // if (LayoutEngine.BeginScrollGroup(new Vector2(LayoutEngine.AutoWidth, position.size.y), _windowScroll)) {
                IMGUI();
            // }
            // _windowScroll = LayoutEngine.EndScrollGroup();
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();
    }
}