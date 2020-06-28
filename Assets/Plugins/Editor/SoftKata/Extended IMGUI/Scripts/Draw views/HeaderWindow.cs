using UnityEngine;
using UnityEditor;
using SoftKata.UnityEditor.Controls;

namespace SoftKata.UnityEditor {
    public abstract class HeaderWindow : ExtendedWindow {
        private WindowHeaderBar _headerBar;
        private Texture _dropdownShadow;

        protected sealed override void Initialize() {
            _dropdownShadow = ExtendedEditor.Resources.Shadow;

            Initialize(_headerBar = new WindowHeaderBar());
        }
        protected abstract void Initialize(WindowHeaderBar headerBar);
    
        public void OnGUI() {
            _headerBar.OnGUI();

            DrawContent();

            var shadowRect = new Rect(
                0, WindowHeaderBar.HeaderHeight, 
                EditorGUIUtility.currentViewWidth - 2, WindowHeaderBar.WindowHeaderShadowHeight
            );
            GUI.DrawTexture(shadowRect, _dropdownShadow);
        }
        protected abstract void DrawContent();
    }
}
