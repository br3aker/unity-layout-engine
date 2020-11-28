using UnityEditor;
using UnityEditor.WindowsStandalone;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public static partial class Layout {
        public const float UnityDefaultLineHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant

        internal const float FlexibleWidth = -1;

        private static LayoutGroup _currentGroup;

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

            var isValid = eventType == EventType.Layout ? group.BeginLayout(_currentGroup) : group.BeginNonLayout();
            if(isValid) {
                _currentGroup = group;
                return true;
            }
            return false;
        }
        public static void EndCurrentScope() {
            if(Event.current.type == EventType.Layout) {
                _currentGroup.EndLayout();
            }
            else {
                _currentGroup.EndNonLayout();
            }
            _currentGroup = _currentGroup.Parent;
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