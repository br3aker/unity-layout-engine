using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public const float AutoWidth = -1f;
        private static readonly Rect InvalidRect = new Rect(float.MaxValue, float.MaxValue, -1, -1);
        private static readonly RectOffset ZeroRectOffset = new RectOffset(0, 0, 0, 0);

        private static readonly Queue<LayoutGroupBase> LayoutGroupQueue = new Queue<LayoutGroupBase>();

        private static LayoutGroupBase _topGroup;
        public static LayoutGroupBase CurrentGroup => _topGroup;
        public static float CurrentContentWidth => _topGroup?.VisibleContentWidth ?? EditorGUIUtility.currentViewWidth;

        public static int GetGroupQueueSize() {
            return LayoutGroupQueue.Count;
        }

        private static Rect GetRectFromRoot(float height, float width = AutoWidth) {
            var rect = GUILayoutUtility.GetRect(width, height);
            rect.width = width > 0f ? width : EditorGUIUtility.currentViewWidth;
            return rect;
        }

        public static Rect GetRect(float height, float width = AutoWidth) {
            return _topGroup?.GetNextEntryRect(width, height) ?? GetRectFromRoot(height, width);
        }
        public static bool GetRect(float height, float width, out Rect rect) {
            rect = _topGroup?.GetNextEntryRect(width, height) ?? GetRectFromRoot(height, width);
            return rect.IsValid();
        }

        public static void RegisterArray(int count, float elementHeight, float elementWidth = AutoWidth) {
            if (_topGroup != null)
                _topGroup.RegisterArray(elementWidth, elementHeight, count);
            else
                GetRectFromRoot(elementHeight * count);
        }

        private static void ScrapGroups(int count) {
            for (; count > 0; count--) LayoutGroupQueue.Dequeue();
        }

        public static void ResetEngine() {
            _topGroup = null;
            LayoutGroupQueue.Clear();
        }
    }
}