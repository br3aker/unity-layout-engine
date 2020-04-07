using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public const float AutoWidth = -1f;

        internal static LayoutGroup _currentGroup;

        public static float CurrentContentWidth => _currentGroup?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);

        // Native Unity layout system get rect call
        internal static Rect GetRectFromUnityLayout(float height, float width = AutoWidth) {
            var rect = GUILayoutUtility.GetRect(width, height);
            rect.width = width > 0f ? width : (EditorGUIUtility.currentViewWidth - 2);
            return rect;
        }

        // Layout group management
        public static bool BeginLayoutGroup(LayoutGroup group) {
            if(Event.current.type == EventType.Layout) {
                group.ResetLayout();
                group.BeginLayout(_currentGroup);
                _currentGroup = group;
                return true;
            }
            group.BeginNonLayout();
            _currentGroup = group;
            return group.IsGroupValid;
        }
        public static void EndLayoutGroup<T>() {
            var group = _currentGroup;
            if(Event.current.type == EventType.Layout) {
                group.EndLayout();
            }
            else if(group.IsGroupValid) {
                group.EndNonLayout();
            }
            _currentGroup = group.Parent;
        }
        
        // Register array of equal elements in one batch
        public static void RegisterArray(int count, float elementHeight, float elementWidth) {
            if (_currentGroup != null)
                _currentGroup.RegisterArray(elementWidth, elementHeight, count);
            else
                GetRectFromUnityLayout(elementHeight * count, elementWidth);
        }
        public static void RegisterArray(int count, float elementHeight) {
            if (_currentGroup != null)
                _currentGroup.RegisterArray(elementHeight, count);
            else
                GetRectFromUnityLayout(elementHeight * count);
        }
    
        // Getting rect from layout engine
        public static bool GetRect(float height, float width, out Rect rect) {
            if(_currentGroup != null) {
                return _currentGroup.GetRect(height, width, out rect);
            }
            rect = GetRectFromUnityLayout(height, width);
            return true;
        }
        public static Rect GetRect(float height, float width = AutoWidth) {
            return _currentGroup?.GetRect(height, width) ?? GetRectFromUnityLayout(height, width);
        }
    }
}