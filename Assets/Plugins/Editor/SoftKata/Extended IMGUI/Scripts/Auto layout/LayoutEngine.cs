using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        private static readonly Rect LayoutDummyRect = new Rect(0f, 0f, 1f, 1f);
        private static readonly Rect InvalidRect = new Rect(0f, 0f, -1f, -1f);

        private static readonly Queue<LayoutGroupBase> SubscribedForLayout = new Queue<LayoutGroupBase>();
        private static readonly Stack<LayoutGroupBase> ActiveGroupStack = new Stack<LayoutGroupBase>();
        
        private static LayoutGroupBase _topGroup;

        private static int _groupCount = 0;
        
        // TODO [remove]: this is only used for debug 
        private static int _globalIndentLevel = 0;

        public struct LayoutDebugData {
            public string Data;
            public bool IsValid;
        }
        private static List<LayoutDebugData> _debugDataList = new List<LayoutDebugData>();
        private static LayoutDebugData[] _debugDataOut;

        
        internal static Rect RequestRectRaw(float height, float width = -1f) {
            var rect = GUILayoutUtility.GetRect(width, height);
            if (width > 0f) {
                rect.width = width;
            }
            return rect;
        }
        
        public static Rect RequestLayoutRect(int height) {
            return _topGroup?.GetRect(height) ?? RequestRectRaw(height);
        }
        public static Rect RequestLayoutRect(int height, int width) {
            return _topGroup?.GetRect(height, width) ?? RequestRectRaw(height, width);
        }
        public static Rect RequestLayoutRect(GUIStyle style) {
            return RequestLayoutRect(style.GetContentHeight());
        }

        public static void RegisterElementsArray(float elementHeight, int count) {
            RegisterElementsArray(elementHeight, EditorGUIUtility.currentViewWidth, count);
        }
        public static void RegisterElementsArray(float elementHeight, float elementWidth, int count) {
            if (_topGroup != null) {
                _topGroup.RegisterRectArray(elementHeight, elementWidth, count);
            }
            else {
                RequestRectRaw(elementHeight * count);
            }
        }
        

        public static void ScrapGroups(int count) {
//            _groupCount -= count;
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