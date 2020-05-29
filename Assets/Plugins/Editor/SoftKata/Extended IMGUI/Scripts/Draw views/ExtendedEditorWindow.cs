using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

using SoftKata.UnityEditor.Controls;

namespace SoftKata.UnityEditor {
    public abstract class ExtendedEditorWindow : EditorWindow, IRepaintable {
        // Header bar
        protected WindowHeaderBar _headerBar;

        // Content
        private ScrollGroup _rootScrollGroup;

        // Repaint routine
        [NonSerialized]
        private int _repaintSubscribersCount = 0;

        private void OnEnable() {
            ExtendedEditor.CurrentView = this;

            // Content
            _rootScrollGroup = new ScrollGroup(Vector2.zero, false);

            Initialize();
        }

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;

            Profiler.BeginSample($"[{Event.current.type}] ExtendedEditorWindow");

            // Header bar
            _headerBar?.OnGUI();

            // Content 
            _rootScrollGroup.ContainerSize = new Vector2(position.size.x, position.size.y - 100);
            if (Layout.BeginLayoutGroup(_rootScrollGroup)) {
                DrawDebugInfo();
                IMGUI();
                Layout.EndLayoutGroup();
            }

            // Drawing header shadow
            // Hacky approach without accessing header itself
            // Each window reset GUI matrix so top-left border is (0, 0)
            // Header height = vertical_padding + IDrawable_size = (3 + 3) + 14
            GUI.DrawTexture(new Rect(0, WindowHeaderBar.HeaderHeight, EditorGUIUtility.currentViewWidth - 2, WindowHeaderBar.WindowHeaderShadowHeight), ExtendedEditor.Resources.Shadow);

            Profiler.EndSample();
        }

        // Repaint routine
        public void RegisterRepaintRequest() {
            if(_repaintSubscribersCount++ == 0) {
                EditorApplication.update += Repaint;
            }
        }
        public void UnregisterRepaintRequest() {
            if(--_repaintSubscribersCount == 0) {
                EditorApplication.update -= Repaint;
            }
        }

        protected abstract void Initialize();
        protected abstract void IMGUI();

        // Debug
        private void DrawDebugInfo() {
            EditorGUI.LabelField(_rootScrollGroup.GetRect(16), $"Repaint request count: {_repaintSubscribersCount}");
        }
    }
}
