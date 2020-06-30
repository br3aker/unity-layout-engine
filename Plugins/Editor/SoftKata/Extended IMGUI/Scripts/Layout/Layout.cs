using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public static partial class Layout {
        internal const float FlexibleWidth = -1;

        internal static LayoutGroup _currentGroup;

        public static float CurrentContentWidth {
            get => _currentGroup?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);
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
            return 
                Event.current.type == EventType.Layout 
                ? group.BeginLayout()
                : group.BeginNonLayout();
        }
        public static void EndCurrentScope() {
            var group = _currentGroup;
            if(Event.current.type == EventType.Layout) {
                group.EndLayout();
            }
            else {
                group.EndNonLayout();
            }
            _currentGroup = group.Parent;
        }
    
        public static bool GetRect(float width, float height, out Rect rect) {
            if(_currentGroup != null) {
                return _currentGroup.GetRect(width, height, out rect);
            }
            rect = GetRectFromUnityLayout(width, height);
            return true;
        }
        public static bool GetRect(float height, out Rect rect) {
            if(_currentGroup != null) {
                return _currentGroup.GetRect(height, out rect);
            }
            rect = GetRectFromUnityLayout(height);
            return true;
        }
        public static Rect GetRect(float width, float height) {
            return _currentGroup?.GetRect(width, height) ?? GetRectFromUnityLayout(width, height);
        }
        public static Rect GetRect(float height) {
            return _currentGroup?.GetRect(height) ?? GetRectFromUnityLayout(height);
        }
    }
}