using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        private static readonly Rect LayoutDummyRect = new Rect(0f, 0f, 1f, 1f);
        private static readonly Rect InvalidRect = new Rect(0f, 0f, -1f, -1f);

        private static readonly Queue<LayoutGroupBase> SubscribedForLayout = new Queue<LayoutGroupBase>();
        private static readonly Stack<LayoutGroupBase> ActiveGroupStack = new Stack<LayoutGroupBase>();
        
        private static LayoutGroupBase _topGroup;

        private static int _groupCount = 0;
        private static int _indentLevel = 0;

        public static int GetTotalGroupCount() {
            return SubscribedForLayout.Count;
        }

        public struct LayoutGroupDebugData {
            public bool Valid;
            public string Data;
        }
        public static LayoutGroupDebugData[] GetLayoutGroupsData() {
            using (var enumerator = SubscribedForLayout.GetEnumerator()) {
                LayoutGroupDebugData[] data = new LayoutGroupDebugData[SubscribedForLayout.Count];
                for (int i = 0; i < SubscribedForLayout.Count; i++) {
                    enumerator.MoveNext();
                    var group = enumerator.Current;
                    data[i] = new LayoutGroupDebugData {
                        Valid = group.IsGroupValid,
                        Data = $"{new string('\t', group.debugDataIndent)}{group.GetType().Name} | Child group count: {group._childrenCount}"
                    };
                }

                return data;
            }
        }

        private static Rect RequestRectRaw(float height, float width = 0f) {
            return GUILayoutUtility.GetRect(width, height);
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
        

        public static void ScrapGroups(int count) {
            _groupCount -= count;
            for (; count > 0; count--) {
                SubscribedForLayout.Dequeue();
            }
        }
    }
}