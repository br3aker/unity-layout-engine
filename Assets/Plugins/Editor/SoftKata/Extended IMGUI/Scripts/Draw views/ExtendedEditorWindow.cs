using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

using SoftKata.UnityEditor.Controls;

namespace SoftKata.UnityEditor {
    public abstract class ScrollEditorWindow : EditorWindow, IRepaintable {
        protected WindowHeaderBar _headerBar;
        private Texture _dropdownShadow;

        private ScrollGroup _contentRoot;

        [NonSerialized]
        private int _repaintSubscribersCount = 0;

        #pragma warning disable IDE0051
        private void OnEnable() {
            ExtendedEditor.CurrentView = this;
            
            _dropdownShadow = ExtendedEditor.Resources.Shadow;

            _contentRoot = new ScrollGroup(Vector2.zero);

            Initialize();
        }
        #pragma warning restore

        public void OnGUI() {
            if (Event.current.type == EventType.Used) return;

            _headerBar?.OnGUI();

            _contentRoot.ContainerSize = new Vector2(position.size.x, position.size.y - 100);
            if (Layout.BeginLayoutGroup(_contentRoot)) {
                IMGUI();
                Layout.EndLayoutGroup();
            }

            // Drawing header shadow
            // Hacky approach without accessing header itself
            // Each window reset GUI matrix so top-left border is (0, 0)
            // Header height = vertical_padding + IDrawable_size = (2 + 2) + 16 = 20
            // This value is stored in constant named WindowHeaderBar.HeaderHeight
            var shadowRect = new Rect(
                0, WindowHeaderBar.HeaderHeight, 
                EditorGUIUtility.currentViewWidth - 2, WindowHeaderBar.WindowHeaderShadowHeight
            );
            GUI.DrawTexture(shadowRect, ExtendedEditor.Resources.Shadow);
        }

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
}
