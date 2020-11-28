using UnityEditor;
using UnityEngine;


namespace SoftKata.UnityEditor {
    public static partial class ExtendedEditor {
        // Elevation shadow
        private const float ElevationShadowheight = 5;

        // Postfix text
        private const float PostfixTextAreaWidth = 40;

        // WARNING: No event type check, only viable at [Repaint] event
        public static void DrawPostfixUnsafe(Rect controlRect, string postfix) {
            var postfixRect = new Rect(controlRect.xMax - PostfixTextAreaWidth, controlRect.y, PostfixTextAreaWidth, controlRect.height);
            Resources.InputFieldPostfix.Draw(postfixRect, postfix, false, false, false, false);
        }
    
        // Elevation shadow
        public static void DrawElevationShadow(Vector2 position, float width) {
            var rect = new Rect(position, new Vector2(width, ElevationShadowheight));
            GUI.DrawTexture(rect, Resources.ElevationShadow);
        }
        public static void DrawElevationShadow(Vector2 position) {
            DrawElevationShadow(position, Layout.CurrentContentWidth);
        }
        public static void DrawHeaderShadow() {
            DrawElevationShadow(Vector2.zero);
        }
    }
}