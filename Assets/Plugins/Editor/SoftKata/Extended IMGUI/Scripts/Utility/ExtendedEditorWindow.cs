using UnityEditor;
using UnityEngine;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        private LayoutEngine.ScrollGroup _rootScrollGroup;

        private void OnEnable(){
            _rootScrollGroup = new LayoutEngine.ScrollGroup(Vector2.zero, Vector2.zero, false, Constraints.None, ExtendedEditorGUI.LayoutResources.ScrollGroup);
            InitEditorWindow(this);
            Initialize();
        }

        private void OnFocus() {
            InitEditorWindow(this);
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;
            if (Event.current.type == EventType.Layout) LayoutEngine.ResetEngine();

            // ALWAYS use -1 as horizontal size for automatic layout
            // EditorWindow.position.width is 2 pixels lesser than EditorGUIUtility.currentViewWidth
            //_rootScrollGroup.ContainerSize = new Vector2(LayoutEngine.AutoWidth, position.size.y);
            //if (LayoutEngine.BeginScrollGroup(_rootScrollGroup)) {
                IMGUI();
            //}
            //LayoutEngine.EndScrollGroup();
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();
    }
}