using UnityEngine;
using UnityEditor;
using SoftKata.UnityEditor.Controls;

namespace SoftKata.UnityEditor {
    public abstract class HeaderWindow : ExtendedWindow {
        private WindowHeaderBar _headerBar;
        private Texture _dropdownShadow;

        protected sealed override void Initialize() {
            _dropdownShadow = Resources.ElevationShadow;

            Initialize(_headerBar = new WindowHeaderBar());
        }
        protected virtual void Initialize(WindowHeaderBar headerBar) {}
    
        public void OnGUI() {
            _headerBar.OnGUI();

            DrawContent();

            ExtendedEditor.DrawElevationShadow(new Vector2(0, WindowHeaderBar.HeaderHeight));
        }
        protected abstract void DrawContent();
    }
}
