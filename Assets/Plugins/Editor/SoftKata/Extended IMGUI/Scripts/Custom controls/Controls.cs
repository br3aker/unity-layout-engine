using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace SoftKata.UnityEditor {
    public static partial class ExtendedEditor {
        private const int NoActiveControlId = int.MinValue;

        public const float LabelHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant

        // Postfix text
        private const float PostfixTextAreaWidth = 50;

        // Postfix icon
        private const float PostfixIconSize = 16;
        private const float PostfixIconAreaWidth = PostfixIconSize + 3;

        // Toggle array
        public const float ToggleArrayHeight = 20;

        // Element list
        public const int ShortcutRecorderWidth = 200;

        private static readonly GUIContent _tempContent = new GUIContent();

        private static void ClearTempContent() {
            _tempContent.text = null;
            _tempContent.image = null;
            _tempContent.tooltip = null;
        }

        private static GUIContent TempContent(string text) {
            ClearTempContent();
            _tempContent.text = text;
            return _tempContent;
        }

        private static T GenericInputFieldWithPostfix<T>(Rect rect, T value, string postfix) {
            global::UnityEditor.EditorGUI.BeginChangeCheck();
            var expression = global::UnityEditor.EditorGUI.DelayedTextField(rect, value.ToString());

            if (global::UnityEditor.EditorGUI.EndChangeCheck())
                if (ExpressionEvaluator.Evaluate(expression, out T newVal))
                    return newVal;

            // Postfix
            if (Event.current.type == EventType.Repaint) {
                var postfixRect = new Rect(rect.xMax - PostfixTextAreaWidth, rect.y, PostfixTextAreaWidth, rect.height);
                Resources.InputFieldPostfix.Draw(postfixRect, TempContent(postfix), false, false, false, false);
            }

            return value;
        }

        public static int IntDelayedField(Rect rect, int value, string postfix) {
            return GenericInputFieldWithPostfix(rect, value, postfix);
        }

        public static void IntDelayedField(Rect rect, SerializedProperty value, string postfix) {
            value.intValue = GenericInputFieldWithPostfix(rect, value.intValue, postfix);
        }

        public static float FloatDelayedField(Rect rect, float value, string postfix) {
            return GenericInputFieldWithPostfix(rect, value, postfix);
        }

        public static void FloatDelayedField(Rect rect, SerializedProperty value, string postfix) {
            value.floatValue = GenericInputFieldWithPostfix(rect, value.floatValue, postfix);
        }
    }
}