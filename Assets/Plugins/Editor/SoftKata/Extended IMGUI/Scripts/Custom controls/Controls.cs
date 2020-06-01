using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace SoftKata.UnityEditor {
    public static partial class ExtendedEditor {
        public const float LabelHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant

        // Postfix text
        private const float PostfixTextAreaWidth = 40;

        // No event type check
        public static void DrawPostfixUnsafe(Rect controlRect, string postfix) {
            var postfixRect = new Rect(controlRect.xMax - PostfixTextAreaWidth, controlRect.y, PostfixTextAreaWidth, controlRect.height);
            Resources.InputFieldPostfix.Draw(postfixRect, postfix, false, false, false, false);
        }
    }
}