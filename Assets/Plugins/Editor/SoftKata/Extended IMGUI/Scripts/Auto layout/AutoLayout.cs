using System.Collections.Generic;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static bool IsDebug { get; set; }
        
        private static readonly Rect LayoutDummyRect = new Rect(0f, 0f, 0f, 0f);
        private static readonly Rect InvalidDummyRect = new Rect(0f, 0f, -1f, -1f);

        private static readonly Queue<LayoutGroup> SubscribedForLayout = new Queue<LayoutGroup>();
        private static readonly Stack<LayoutGroup> ActiveGroupStack = new Stack<LayoutGroup>();
        private static LayoutGroup TopGroup;

        private static readonly float IndentStep = ExtendedEditorGUI.Resources.Margins.DefaultMargins.margin.left;
        private static float Indent;
        
        public static string GetEngineInfo() {
            return $"Layout group count: [{SubscribedForLayout.Count}] | Horizontal indent: [value: {Indent}]";
        }
        public static string GetCurrentLayoutGroupInfo(bool groupRect = true, bool nestingLevel = false) {
            if (TopGroup == null) {
                return "Engine is set to root";
            }

            string output = "";
            if (groupRect)
                output += $"Requested rect: {TopGroup.FullRect}. ";
            if (nestingLevel)
                output += $"Current nesting level: {ActiveGroupStack.Count}.";
            
            return output;
        }

        
        private static Rect RequestIndentedRect(float height) {
            var rect =  GUILayoutUtility.GetRect(0f, height);
            rect.xMin += Indent;
            return rect;
        }
        public static Rect RequestLayoutRect(int height) {
            return TopGroup?.GetRect(height) ?? RequestIndentedRect(height);
        }
        public static Rect RequestLayoutRect(GUIStyle style) {
            return RequestLayoutRect(style.GetContentHeight());
        }
    }
}