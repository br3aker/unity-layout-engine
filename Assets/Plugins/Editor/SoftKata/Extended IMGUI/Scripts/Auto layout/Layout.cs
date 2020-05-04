using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class Layout {
        internal const float FlexibleWidth = -1;

        internal static LayoutGroup _currentGroup;

        public static float CurrentContentWidth => _currentGroup?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);

        // Native Unity layout system get rect call
        internal static Rect GetRectFromUnityLayout(float width, float height) {
            var rect = GUILayoutUtility.GetRect(width, height);
            rect.width = width;
            return rect;
        }
        internal static Rect GetRectFromUnityLayout(float height) {
            return GetRectFromUnityLayout(EditorGUIUtility.currentViewWidth - 2, height);
        }

        // Layout group management
        public static bool BeginLayoutGroup(LayoutGroup group) {
            if(Event.current.type == EventType.Layout) {
                group.BeginLayout(_currentGroup);
                _currentGroup = group;
                return true;
            }
            if(group.BeginNonLayout()) {
                _currentGroup = group;
                return true;
            }
            return false;
        }
        public static void EndLayoutGroup() {
            var group = _currentGroup;
            if(Event.current.type == EventType.Layout) {
                group.EndLayout();
            }
            else if(group.IsGroupValid) {
                group.EndNonLayout();
            }
            _currentGroup = group.Parent;
        }
        
        public static bool BeginLayoutGroupRetained(LayoutGroup group) {
            if(Event.current.type == EventType.Layout) {
                if(group.BeginLayoutRetained(_currentGroup)) {
                    _currentGroup = group;
                    return true;
                }
                return false;
            }
            if(group.BeginNonLayout()) {
                _currentGroup = group;
                return true;
            }
            return false;
        }
        public static void EndLayoutGroupRetained() {
            var group = _currentGroup;
            if(Event.current.type == EventType.Layout) {
                _currentGroup = group.Parent;
                group.EndLayoutRetained();
            }
            else if(group.IsGroupValid) {
                _currentGroup = group.Parent;
                group.EndNonLayout();
            }
        }
        
        // Register array of equal elements in one batch
        public static void RegisterArray(int count, float elementWidth, float elementHeight) {
            if (_currentGroup != null)
                _currentGroup.RegisterEntriesArray(elementWidth, elementHeight, count);
            else
                GetRectFromUnityLayout(elementWidth, elementHeight * count);
        }
        public static void RegisterArray(int count, float elementHeight) {
            if (_currentGroup != null)
                _currentGroup.RegisterEntriesArray(elementHeight, count);
            else
                GetRectFromUnityLayout(elementHeight * count);
        }
    
        // Getting rect from layout engine
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
    
        // Extensions
        public static void DrawLeftSeparator(this LayoutGroup group, Color color) {
            var contentRect = group.ContentRect;
            var style = group.Style;

            var padding = style.padding;
            var width = style.border.left;
            var separatorRect = new Rect(
                contentRect.x - padding.left - width,
                contentRect.y - padding.top,
                width,
                contentRect.height + padding.vertical
            );
            EditorGUI.DrawRect(separatorRect, color);
        }
    }
}