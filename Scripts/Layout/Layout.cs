using UnityEditor;
using UnityEditor.WindowsStandalone;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public static partial class Layout {
        public const float UnityDefaultLineHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant

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
            var eventType = Event.current.type;
            if (eventType == EventType.Used || eventType == EventType.Ignore) return false;
            if (eventType == EventType.Layout)
                return group.BeginLayout();
            return group.BeginNonLayout();
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
        public static bool GetRect(out Rect rect) {
            return GetRect(UnityDefaultLineHeight, out rect);
        }
        public static Rect GetRect(float width, float height) {
            return _currentGroup?.GetRect(width, height) ?? GetRectFromUnityLayout(width, height);
        }
        public static Rect GetRect(float height = UnityDefaultLineHeight) {
            return _currentGroup?.GetRect(height) ?? GetRectFromUnityLayout(height);
        }
    }
}