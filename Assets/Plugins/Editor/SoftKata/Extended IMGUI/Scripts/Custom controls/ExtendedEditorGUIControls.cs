using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


// TODO [implement/control]: add underlined text control and use it in underlined foldout
namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        private const int NoActiveControlId = int.MinValue;

        public const float LabelHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant
        public const float ErrorSubLabelHeight = 10;
        public const float LabelWithErrorHeight = LabelHeight + ErrorSubLabelHeight;

        // Postfix text
        private const float PostfixTextAreaWidth = 50;

        // Postfix icon
        private const float PostfixIconSize = 16;
        private const float PostfixIconAreaWidth = PostfixIconSize + 3;

        // Toggle array
        public const float ToggleArrayHeight = 20;

        // Element list
        public const float ElementListMainLabelHeight = 18;
        public const float ElementListSubLabelHeight = 14;
        public const float ElementListHeight = ElementListMainLabelHeight + ElementListSubLabelHeight;

        public const int ShortcutRecorderWidth = 200;

        public static readonly Func<int> GetLastControlId = 
            Utility.CreateStaticGetter<int>(
                typeof(EditorGUIUtility).GetField(
                    "s_LastControlID", 
                    BindingFlags.Static | BindingFlags.NonPublic
                )
            );


        private static GUIContent _tempContent = new GUIContent();
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


        public static int ToggleArray(Rect rect, int value, GUIContent[] contents) {
            var buttonsStyles = Resources.Buttons;
            var leftStyle = buttonsStyles.Left;
            var midStyle = buttonsStyles.Mid;
            var rightStyle = buttonsStyles.Right;
            
            var actualContentLength = contents.Length / 2;

            var fixedWidthFromStyle = midStyle.fixedWidth;
            var cellWidth = fixedWidthFromStyle > 0 
                ? fixedWidthFromStyle
                : (rect.width - 1 * actualContentLength) / actualContentLength;

            var iconHorizontalOffset = cellWidth + 1;

            var toggleRect = new Rect(rect.x, rect.y, cellWidth, ToggleArrayHeight);
            
            for (int i = 0; i < actualContentLength; i++) {
                int checker = 1 << i;
                bool on = (value & checker) == checker;

                var content = contents[on ? i : i + actualContentLength];

                var style = i == 0 ? leftStyle : i == actualContentLength - 1 ? rightStyle : midStyle;
                if (GUI.Toggle(toggleRect, on, content, style)) {
                    value |= checker;
                }
                else {
                    value &= ~checker;
                }

                toggleRect.x += iconHorizontalOffset;
            }

            return value;
        }
        public static void ToggleArray(Rect rect, SerializedProperty value, GUIContent[] contents) {
            value.intValue = ToggleArray(rect, value.intValue, contents);
        }
        
        private static T GenericAssertedField<T>(Rect rect, T value, string postfix, string errorMessage) {
            bool isError = errorMessage != null && GUI.enabled;
            
            // Styles
            var styles = Resources.PostfixInputField;
            var valueStyle = isError ? styles.Error : Resources.GenericInputField;

            EditorGUI.BeginChangeCheck();
            var expression = EditorGUI.DelayedTextField(rect, value.ToString(), valueStyle);
            if (isError) {
                var errorStyle = styles.ErrorMessage;
                var errorRect = new Rect(rect.x, rect.yMax + errorStyle.margin.top, rect.width, ErrorSubLabelHeight);

                EditorGUI.LabelField(errorRect, errorMessage, styles.ErrorMessage);
            }
            if (EditorGUI.EndChangeCheck()) {
                if (ExpressionEvaluator.Evaluate(expression, out T newVal))
                    return newVal;
            }
            
            // Postfix
            if (Event.current.type == EventType.Repaint) {
                var postfixRect = new Rect(rect.xMax - PostfixTextAreaWidth, rect.y, PostfixTextAreaWidth, rect.height);
                Resources.GenericPostfix.Draw(postfixRect, TempContent(postfix), false, false, false, false);
            }

            return value;
        }

        public static int IntDelayedField(Rect rect, int value, string postfix, string errorMessage) {
            return GenericAssertedField(rect, value, postfix, errorMessage);
        }
        public static void IntDelayedField(Rect rect, SerializedProperty value, string postfix, string errorMessage) {
            value.intValue = GenericAssertedField(rect, value.intValue, postfix, errorMessage);
        }
        
        public static float FloatDelayedField(Rect rect, float value, string postfix, string errorMessage) {
            return GenericAssertedField(rect, value, postfix, errorMessage);
        }
        public static void FloatDelayedField(Rect rect, SerializedProperty value, string postfix, string errorMessage) {
            value.floatValue = GenericAssertedField(rect, value.floatValue, postfix, errorMessage);
        }

        public static bool UnderlineFoldout(Rect rect, bool expanded, string label) {
            var style = Resources.Foldout.Underline;
            if (EditorGUI.Foldout(rect, expanded, label, style)) {
                var underlineRect = new Rect(rect.x, rect.yMax - 1, rect.width, 1);
                EditorGUI.DrawRect(underlineRect, style.active.textColor);
                return true;
            }
            return false;
        }

        public static void UnderlineFoldout(Rect rect, SerializedProperty expanded, string label) {
            expanded.isExpanded = UnderlineFoldout(rect, expanded.isExpanded, label);
        }
        
        public static void ListElement(Rect rect, GUIContent mainLabel, GUIContent subLabel) {
            if (Event.current.type != EventType.Repaint) return;
            
            var styles = Resources.ListElement;
            
            // Main label
            styles.MainLabel.Draw(rect, mainLabel, rect.Contains(Event.current.mousePosition), false, false, false);

            // Sub label
            rect.y += ElementListMainLabelHeight;
            rect.height = ElementListSubLabelHeight;
            styles.SubLabel.Draw(rect, subLabel, false, false, false, false);
        }
    }
}