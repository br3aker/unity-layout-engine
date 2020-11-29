using UnityEditor;
using UnityEditor.WindowsStandalone;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public static partial class Layout {
        public const float UnityDefaultLineHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant

        internal const float FlexibleWidth = -1;

        internal static LayoutGroup CurrentGroup;

        public static float CurrentContentWidth {
            get => CurrentGroup?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);
        }

        // Native Unity layout system get rect call
        internal static Rect GetRectFromUnityLayout(float width, float height) {
            var rect = GUILayoutUtility.GetRect(width, height);
            rect.width = width;
            return rect;
        }
        internal static Rect GetRectFromUnityLayout(float height) {
            return GetRectFromUnityLayout(EditorGUIUtility.currentViewWidth - 2, height);
        }

        public static bool BeginLayoutScope(LayoutGroup group) {
            var valid = group.BeginScope(CurrentGroup);
            if (valid) CurrentGroup = group;
            return valid;
        }
        public static void EndCurrentScope() {
            CurrentGroup.EndScope();
            CurrentGroup = CurrentGroup.Parent;
        }
    
        public static bool GetRect(float width, float height, out Rect rect) {
            if(CurrentGroup != null) {
                return CurrentGroup.GetRect(width, height, out rect);
            }
            rect = GetRectFromUnityLayout(width, height);
            return true;
        }
        public static bool GetRect(float height, out Rect rect) {
            if(CurrentGroup != null) {
                return CurrentGroup.GetRect(height, out rect);
            }
            rect = GetRectFromUnityLayout(height);
            return true;
        }
        public static bool GetRect(out Rect rect) {
            return GetRect(UnityDefaultLineHeight, out rect);
        }
        public static Rect GetRect(float width, float height) {
            return CurrentGroup?.GetRect(width, height) ?? GetRectFromUnityLayout(width, height);
        }
        public static Rect GetRect(float height = UnityDefaultLineHeight) {
            return CurrentGroup?.GetRect(height) ?? GetRectFromUnityLayout(height);
        }
    }
}