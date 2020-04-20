﻿using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class Layout {
        public const float AutoWidth = -1f;

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
            if(group.IsGroupValid) {
                if(group.BeginNonLayout()) {
                    _currentGroup = group;
                    return true;
                }
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
        
        // Register array of equal elements in one batch
        public static void RegisterArray(int count, float elementHeight, float elementWidth) {
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
                return _currentGroup.GetRect(height, width, out rect);
            }
            rect = GetRectFromUnityLayout(width, height);
            return true;
        }
        public static Rect GetRect(float height) {
            return _currentGroup?.GetRect(height, AutoWidth) ?? GetRectFromUnityLayout(AutoWidth, height);
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