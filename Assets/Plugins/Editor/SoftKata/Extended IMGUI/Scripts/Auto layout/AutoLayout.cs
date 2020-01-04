using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        private static readonly Rect LayoutDummyRect = new Rect(0f, 0f, 1f, 1f);
        private static readonly Rect InvalidRect = new Rect(0f, 0f, -1f, -1f);

        private static readonly Queue<LayoutGroupBase> SubscribedForLayout = new Queue<LayoutGroupBase>();
        private static readonly Stack<LayoutGroupBase> ActiveGroupStack = new Stack<LayoutGroupBase>();
        private static LayoutGroupBase TopGroup;

        public static int GetTotalGroupCount() {
            return SubscribedForLayout.Count;
        }

        public static int GetCurrentGroupDepth() {
            return ActiveGroupStack.Count;
        }


        
        private static Rect RequestRectRaw(float height, float width = 0f) {
            return GUILayoutUtility.GetRect(width, height);
        }
        public static Rect RequestLayoutRect(int height) {
            return TopGroup?.GetRect(height) ?? RequestRectRaw(height);
        }
        public static Rect RequestLayoutRect(int height, int width) {
            return TopGroup?.GetRect(height, width) ?? RequestRectRaw(height, width);
        }
        public static Rect RequestLayoutRect(GUIStyle style) {
            return RequestLayoutRect(style.GetContentHeight());
        }


        public static void ScrapGroups(int count) {
            if (Event.current.type != EventType.Layout) {
                for (; count > 0; count--) {
                    SubscribedForLayout.Dequeue();
                }
            }
        }
    }
}