using UnityEditor;
using UnityEngine;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        public const float HeaderBarPixelHeight = 20;

        private ScrollGroup _rootScrollGroup;

        private void OnEnable() {
            _rootScrollGroup = new ScrollGroup(Vector2.zero, Vector2.zero, false);
            InitEditorWindow(this);
            Initialize();
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;

            // Header bar
            var headerRect = Layout.GetRectFromUnityLayout(HeaderBarPixelHeight);
            EditorGUI.DrawRect(headerRect, new Color(0.235f, 0.235f, 0.235f));

            _rootScrollGroup.ContainerSize = position.size;
            if (Layout.BeginLayoutGroup(_rootScrollGroup)) {
                IMGUI();
                Layout.EndLayoutGroup();
            }


            // Drawing header shadow
            GUI.DrawTexture(new Rect(headerRect.x, headerRect.yMax, headerRect.width, ShadowPixelHeight), ElementsResources.Shadow);
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();
    }
}