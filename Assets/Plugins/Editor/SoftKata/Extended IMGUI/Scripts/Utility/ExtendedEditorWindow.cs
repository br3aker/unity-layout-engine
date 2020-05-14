using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        public const float HeaderBarPixelHeight = 20;

        // Header bar
        private WindowHeaderBar _headerBar;

        // Content
        private ScrollGroup _rootScrollGroup;


        private void OnEnable() {
            CurrentWindow = this;

            // Header 
            _headerBar = CreateHeader();

            // Content
            _rootScrollGroup = new ScrollGroup(Vector2.zero, false);

            Initialize();
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;

            Profiler.BeginSample($"[{Event.current.type}] ExtendedEditorWindow");

            // Header bar
            _headerBar.OnGUI();

            var testRect = Layout.GetRect(16);
            EditorGUI.DrawRect(testRect, Color.black);
            EditorGUI.LabelField(testRect, testRect.size.ToString());

            // Content 
            _rootScrollGroup.ContainerSize = new Vector2(position.size.x, position.size.y - 100);
            // _rootScrollGroup.ContainerSize = position.size;
            if (Layout.BeginLayoutGroupRetained(_rootScrollGroup)) {
                IMGUI();
                Layout.EndLayoutGroupRetained();
            }

            // Drawing header shadow
            // Hacky approach without accessing header itself
            // Each window reset GUI matrix so top-left border is (0, 0)
            // Header height = vertical_padding + IDrawable_size = (3 + 3) + 14
            GUI.DrawTexture(new Rect(0, WindowHeaderBar.HeaderHeight, EditorGUIUtility.currentViewWidth - 2, ShadowPixelHeight), ExtendedEditorGUI.Resources.Shadow);

            Profiler.EndSample();
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();

        protected abstract WindowHeaderBar CreateHeader();
    }
}