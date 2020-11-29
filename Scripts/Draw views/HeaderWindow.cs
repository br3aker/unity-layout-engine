using UnityEngine;
using UnityEditor;
using SoftKata.UnityEditor.Controls;

namespace SoftKata.UnityEditor {
    public abstract class HeaderWindow : ExtendedWindow {
        private WindowHeaderBar _headerBar;

        // Initialization
        protected sealed override void Initialize() {
            Initialize(_headerBar = new WindowHeaderBar());
            OnHeaderDraw += _headerBar.OnGUI;
            OnFooterDraw += DrawHeaderShadow;
        }
        protected virtual void Initialize(WindowHeaderBar headerBar) {}

        private static void DrawHeaderShadow() {
            ExtendedEditor.DrawElevationShadow(new Vector2(0, WindowHeaderBar.HeaderHeight));
        }
    }
}
