using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow, IRepaintable {
        public const float HeaderBarPixelHeight = 20;
        public const float ShadowPixelHeight = 5;

        // Header bar
        protected WindowHeaderBar _headerBar;

        // Content
        private ScrollGroup _rootScrollGroup;

        // Repaint routine
        private int _repaintSubscribersCount;

        private void OnEnable() {
            CurrentView = this;

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
                IMGUI();
                Layout.EndLayoutGroup();
            }

            // Drawing header shadow
            // Hacky approach without accessing header itself
            // Each window reset GUI matrix so top-left border is (0, 0)
            // Header height = vertical_padding + IDrawable_size = (3 + 3) + 14
            GUI.DrawTexture(new Rect(0, WindowHeaderBar.HeaderHeight, EditorGUIUtility.currentViewWidth - 2, ShadowPixelHeight), ExtendedEditorGUI.Resources.Shadow);

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
    }

    public interface IRepaintable {
        void Repaint();

        void RegisterRepaintRequest();
        void UnregisterRepaintRequest();
    }
}