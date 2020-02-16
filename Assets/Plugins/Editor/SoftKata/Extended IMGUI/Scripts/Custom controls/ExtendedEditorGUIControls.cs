using System;
using System.Globalization;
using System.Linq;
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

        // Postfix text
        private const float PostfixTextAreaWidth = 50;

        // Postfix icon
        private const float PostfixIconSize = 16;
        private const float PostfixIconAreaWidth = PostfixIconSize + 3;

        // Toggle array
        public const float ToggleArrayHeight = 20;
        private const float ToggleArrayIconOffset = 4;


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
        
        // background color -> normal/on normal
        // icon color -> active/on active

        public struct ToggleArrayData {
            internal Texture IconSet;
            internal int Count;

            public GUIContent[] Contents;

            public ToggleArrayData(int count, GUIContent[] contents, Texture iconSet) {
                Count = count;
                Contents = contents;
                IconSet = iconSet;
            }
        }

        public struct ToggleArrayGUIContent {
            public GUIContent[] guiContent;
            public Texture IconSet;

            internal float[] ElementsStartOffset;
            
            public float MaxTabWidth;
            internal float IconSize;
            
            public ToggleArrayGUIContent(GUIContent[] content, Texture iconSet) {
                guiContent = content;
                IconSet = iconSet;
                
                // Icon size
                var style = Resources.Buttons.Mid;
                IconSize = style.GetContentHeight() - style.padding.vertical;
                
                // Width for all elements
                var elementsWidth = guiContent.Select(q => style.CalcSize(q).x).ToArray();
                
                // Actual width for all toggles
                MaxTabWidth = elementsWidth.Max() + IconSize + ToggleArrayIconOffset;

                // Calculating offsets
                ElementsStartOffset = new float[guiContent.Length];
                var extraOffset = MaxTabWidth - IconSize + style.padding.horizontal - ToggleArrayIconOffset;
                for (int i = 0; i < guiContent.Length; i++) {
                    ElementsStartOffset[i] = (MaxTabWidth + 1) * i + (extraOffset - elementsWidth[i]) / 2;
                }
            }
        }

        public static int ToggleArray(Rect rect, int value, GUIContent[] contents, float width) {
            var buttonsStyles = Resources.Buttons;
            var leftStyle = buttonsStyles.Left;
            var midStyle = buttonsStyles.Mid;
            var rightStyle = buttonsStyles.Right;

            float iconHorizontalOffset = width + 1;

            var toggleRect = new Rect(rect.x, rect.y, width, ToggleArrayHeight);

            for (int i = 0; i < contents.Length; i++) {
                int checker = 1 << i;
                bool on = (value & checker) == checker;
                
                if (GUI.Toggle(toggleRect, on, contents[i], i == 0 ? leftStyle : i == contents.Length - 1 ? rightStyle : midStyle)) {
                    value |= checker;
                }
                else {
                    value &= ~checker;
                }

                toggleRect.x += iconHorizontalOffset;
            }

            return value;
        }
        public static int ToggleArray(Rect rect, int value, ToggleArrayGUIContent content) {
            var buttonsStyles = Resources.Buttons;
            var leftStyle = buttonsStyles.Left;
            var midStyle = buttonsStyles.Mid;
            var rightStyle = buttonsStyles.Right;
            
            var iconSize = content.IconSize;
            
            float iconTextureCoordsSize = 1f / content.guiContent.Length;

            var oldPadding = leftStyle.padding.left;
            var newPadding = (int) (content.IconSize + ToggleArrayIconOffset / 2);
            leftStyle.padding.left += newPadding;
            midStyle.padding.left += newPadding;
            rightStyle.padding.left += newPadding;
            
            value = ToggleArray(rect, value, content.guiContent, content.MaxTabWidth);

            var iconY = rect.y + leftStyle.padding.top;
            if (Event.current.type == EventType.Repaint) {
                for (int i = 0; i < content.guiContent.Length; i++) {
                    int checker = 1 << i;
                    bool on = (value & checker) == checker;
                    
                    var textureCoords = new Rect(
                        iconTextureCoordsSize * i, on ? 0 : 0.5f,
                        iconTextureCoordsSize, 0.5f
                    );
                    
                    var iconRect = new Rect(
                        rect.x + content.ElementsStartOffset[i], 
                        iconY,
                        iconSize,
                        iconSize
                    );
                    
                    GUI.DrawTextureWithTexCoords(iconRect, content.IconSet, textureCoords);
                }
            }
            
            leftStyle.padding.left = oldPadding;
            midStyle.padding.left = oldPadding;
            rightStyle.padding.left = oldPadding;

            return value;
        }
        public static void ToggleArray(Rect rect, SerializedProperty value, GUIContent[] contents, float width) {
            value.intValue = ToggleArray(rect, value.intValue, contents, width);
        }
        public static void ToggleArray(Rect rect, SerializedProperty value, ToggleArrayGUIContent content) {
            value.intValue = ToggleArray(rect, value.intValue, content);
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