using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        private static readonly Rect InvalidRect = new Rect(0, 0, -1, -1);
        private static readonly RectOffset ZeroRectOffset = new RectOffset(0, 0, 0, 0);

        public const float AutoWidth = -1f;

        private static readonly Queue<LayoutGroupBase> SubscribedForLayout = new Queue<LayoutGroupBase>();

        public static int GetGroupQueueSize() => SubscribedForLayout.Count;
        
        private static LayoutGroupBase _topGroup;

        internal static Rect RequestRectRaw(float height, float width = AutoWidth) {
            var rect = GUILayoutUtility.GetRect(width, height);
            if (width > 0f) {
                rect.width = Mathf.Min(width, EditorGUIUtility.currentViewWidth);
            }
            return rect;
        }

        public static Rect RequestLayoutRect(float height, float width = AutoWidth) {
            return _topGroup?.GetRect(height, width) ?? RequestRectRaw(height, width);
        }

        public static bool GetRect(float height, float width, out Rect rect) {
            rect = _topGroup?.GetRect(height, width) ?? RequestRectRaw(height, width);
            return rect.IsValid();
        }
        
        public static void RegisterElementsArray(int count, float elementHeight, float elementWidth = AutoWidth) {
            if (_topGroup != null) {
                _topGroup.RegisterRectArray(elementHeight, elementWidth, count);
            }
            else {
                RequestRectRaw(elementHeight * count);
            }
        }

        private static void ScrapGroups(int count) {
            for (; count > 0; count--) {
                SubscribedForLayout.Dequeue();
            }
        }

        public static void ResetEngine() {
            _topGroup = null;
            SubscribedForLayout.Clear();
        }
    }
}