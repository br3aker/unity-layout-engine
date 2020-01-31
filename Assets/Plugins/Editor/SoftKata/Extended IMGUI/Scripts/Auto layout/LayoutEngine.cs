using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        private static readonly Rect InvalidRect = new Rect(0, 0, -1, -1);
        private static readonly RectOffset ZeroRectOffset = new RectOffset(0, 0, 0, 0);

        private const float AutoWidthValue = -1f;

        private static readonly Queue<LayoutGroupBase> SubscribedForLayout = new Queue<LayoutGroupBase>();
        
        private static LayoutGroupBase _topGroup;

        public struct LayoutDebugData {
            public string Data;
            public bool IsValid;
        }
        private static List<LayoutDebugData> _debugDataList = new List<LayoutDebugData>();
        private static LayoutDebugData[] _debugDataOut;

        
        internal static Rect RequestRectRaw(float height, float width = AutoWidthValue) {
            var rect = GUILayoutUtility.GetRect(width, height);
            if (width > 0f) {
                rect.width = Mathf.Min(width, EditorGUIUtility.currentViewWidth);
            }
            return rect;
        }

        public static Rect RequestLayoutRect(float height, float width = AutoWidthValue) {
            return _topGroup?.GetRect(height, width) ?? RequestRectRaw(height, width);
        }

        public static void RegisterElementsArray(int count, float elementHeight) {
            if (_topGroup != null) {
                _topGroup.RegisterRectArray(elementHeight, count);
            }
            else {
                RequestRectRaw(elementHeight * count);
            }
        }
        public static void RegisterElementsArray(int count, float elementHeight, float elementWidth) {
            if (_topGroup != null) {
                _topGroup.RegisterRectArray(elementHeight, elementWidth, count);
            }
            else {
                RequestRectRaw(elementHeight * count);
            }
        }

        public static void ScrapGroups(int count) {
            for (; count > 0; count--) {
                SubscribedForLayout.Dequeue();
            }
        }

        public static LayoutDebugData[] GetEngineGroupHierarchyData() {
            if (Event.current.type == EventType.Layout) {
                _debugDataOut = _debugDataList.ToArray();
                _debugDataList.Clear();
            }

            return _debugDataOut;
        }
    }
}