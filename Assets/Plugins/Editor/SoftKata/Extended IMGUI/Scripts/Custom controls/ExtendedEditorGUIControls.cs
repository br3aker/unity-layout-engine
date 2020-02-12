using System;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;


// TODO [implement/control]: add underlined text control and use it in underlined foldout
namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        private const int NoActiveControlId = int.MinValue;

        public const float LabelHeight = 18; // equal to EditorGUIUtility.singleLineHeight which is a getter, not constant
        public const float ErrorSubLabelHeight = 10;
        public const float LabelWithErrorHeight = LabelHeight + ErrorSubLabelHeight;
        
        private const float AbsoluteBorderOffset = 3;
        
        // Postfix text
        private const float PostfixTextAreaWidth = 50;

        // Postfix icon
        private const float PostfixIconSize = 16;
        private const float PostfixIconAreaWidth = PostfixIconSize + AbsoluteBorderOffset;


        public const int ShortcutRecorderWidth = 200; 
        
        private const float ColorFieldFixedWidth = 25;

        private static GUIContent _tempContent = new GUIContent();
        
        public static readonly Func<int> GetLastControlId = 
            Utility.CreateStaticGetter<int>(
                typeof(EditorGUIUtility).GetField(
                    "s_LastControlID", 
                    BindingFlags.Static | BindingFlags.NonPublic
                )
            );


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

//        // RULE #1
//        // background color -> normal/on normal
//        // icon color -> active/on active
//        
//        // RULE #2
//        // push textures in this way: all off - all on
        public static int EnumToggles(Rect rect, int value, Texture[] icons, GUIStyle style) {
            float backgroundHorizontalOffset = style.fixedWidth + style.margin.right;
            float iconHorizontalOffset = backgroundHorizontalOffset;
            int togglesCount = icons.Length / 2;

            rect.xMax = rect.x + style.fixedWidth;
            
            var padding = style.padding;
            var iconRect = new Rect(
                new Vector2(rect.x + padding.left, rect.y + padding.top),
                new Vector2(rect.width - padding.horizontal, rect.height - padding.vertical)
            );
            
            for (int i = 0; i < icons.Length / 2; i++) {
                int checker = 1 << i;
                bool on = (value & checker) == checker;
                
                Color colorBackup = GUI.color;
                {
                    GUI.color = on ? style.onNormal.textColor : style.normal.textColor;
                    if (EditorGUI.Toggle(rect, GUIContent.none, on, style)) {
                        value |= checker;
                    }
                    else {
                        value &= ~checker;
                    }

                    GUI.color = on ? style.onActive.textColor : style.active.textColor;
                    GUI.DrawTexture(iconRect, icons[on ? i + togglesCount : i]);
                }
                GUI.color = colorBackup;

                rect.x += backgroundHorizontalOffset;
                iconRect.x += iconHorizontalOffset;
            }

            return value;
        }
        public static void EnumToggles(Rect rect, SerializedProperty value, Texture[] icons, GUIStyle style) {
            value.intValue = EnumToggles(rect, value.intValue, icons, style);
        }

        private static string GetTextInput(Rect rect, string value, string postfix, GUIStyle style, GUIStyle postfixStyle) {
            // Main
            var expression = EditorGUI.DelayedTextField(rect, value, style);
            
            // Postfix
            var postfixRect = new Rect(rect.xMax - PostfixTextAreaWidth, rect.y, PostfixTextAreaWidth, rect.height);

            if (Event.current.type == EventType.Repaint) {
                postfixStyle.Draw(postfixRect, TempContent(postfix), false, false, false, false);
            }
            
            return expression;
        }
        private static T GenericAssertedField<T>(Rect rect, T value, string postfix, string errorMessage) {
            bool isError = errorMessage != null && GUI.enabled;
            
            // Styles
            var styles = Resources.InputField;
            var valueStyle = isError ? styles.Error : styles.Main;
            var postfixStyle = styles.Postfix;

            EditorGUI.BeginChangeCheck();
            var expression = GetTextInput(rect, value.ToString(), postfix, valueStyle, postfixStyle);
            if (isError) {
                var errorStyle = styles.ErrorMessage;
                var errorRect = new Rect(rect.x, rect.yMax + errorStyle.margin.top, rect.width, ErrorSubLabelHeight);

                EditorGUI.LabelField(errorRect, errorMessage, styles.ErrorMessage);
            }
            if (EditorGUI.EndChangeCheck()) {
                if (ExpressionEvaluator.Evaluate(expression, out T newVal))
                    return newVal;
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

        public static Color ColorField(Rect rect, Color color) {
            rect.width = ColorFieldFixedWidth;

            return EditorGUI.ColorField(
                rect, GUIContent.none, color, 
                false, true, false
            );
        }
        public static void ColorField(Rect rect, SerializedProperty color) {
            color.colorValue = ColorField(rect, color.colorValue);
        }
    }
}