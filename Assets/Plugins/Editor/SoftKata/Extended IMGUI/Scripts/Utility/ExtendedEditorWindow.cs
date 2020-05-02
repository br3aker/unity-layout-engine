using UnityEditor;
using UnityEngine;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        public const float HeaderBarPixelHeight = 20;

        // Header bar
        private WindowHeaderBar _headerBar;

        // Content
        private ScrollGroup _rootScrollGroup;


        private void OnEnable() {
            InitEditorWindow(this);

            // Header 
            _headerBar = CreateHeader();

            // Content
            _rootScrollGroup = new ScrollGroup(Vector2.zero, Vector2.zero, false);

            Initialize();
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;

            // Header bar
            _headerBar.OnGUI();

            // Content 
            _rootScrollGroup.ContainerSize = position.size;
            if (Layout.BeginLayoutGroup(_rootScrollGroup)) {
                IMGUI();
                Layout.EndLayoutGroup();
            }

            // Drawing header shadow
            // Hacky approach without accessing header itself
            // Each window reset GUI matrix so top-left border is (0, 0)
            // Header height = vertical_padding + IDrawable_size = (3 + 3) + 14
            GUI.DrawTexture(new Rect(0, WindowHeaderBar.HeaderHeight, EditorGUIUtility.currentViewWidth - 2, ShadowPixelHeight), ExtendedEditorGUI.Resources.Shadow);
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();

        protected abstract WindowHeaderBar CreateHeader();
    }
}