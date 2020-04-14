using UnityEditor;
using UnityEngine;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        private ScrollGroup _rootScrollGroup;

        private void OnEnable(){
            _rootScrollGroup = new ScrollGroup(Vector2.zero, Vector2.zero, false, ExtendedEditorGUI.LayoutResources.ScrollGroup);
            InitEditorWindow(this);
            Initialize();
        }

        private void OnFocus() {
            InitEditorWindow(this);
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;

            // _rootScrollGroup.ContainerSize = new Vector2(Layout.AutoWidth, position.size.y);
            // if (Layout.BeginLayoutGroup(_rootScrollGroup)) {
                IMGUI();
            // }
            // Layout.EndLayoutGroup();
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();
    }
}