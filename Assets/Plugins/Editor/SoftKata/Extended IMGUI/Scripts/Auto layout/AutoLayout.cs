﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        private static readonly Rect LayoutDummyRect = new Rect(0f, 0f, 0f, 0f);
        private static readonly Rect InvalidDummyRect = new Rect(0f, 0f, -1f, -1f);

        private static readonly Queue<LayoutGroupBase> SubscribedForLayout = new Queue<LayoutGroupBase>();
        private static readonly Stack<LayoutGroupBase> ActiveGroupStack = new Stack<LayoutGroupBase>();
        private static LayoutGroupBase TopGroup;

        private static readonly float IndentStep = ExtendedEditorGUI.Resources.Margins.DefaultMargins.margin.left;
        private static float Indent;

        
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