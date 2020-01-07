using System;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;


// TODO [implement/control]: add underlined text control and use it in underlined foldout
namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        private const int FOLDOUT_DEFAULT_MARGIN = 12;
        private static GUIContent _tempContent = new GUIContent();
        
        // This is slow but it at least works
        //internal static readonly FieldInfo LastControlId = typeof(EditorGUIUtility).GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);


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


        public static bool PrefixToggle(Rect rect, bool value, GUIContent content, GUIStyle style) {
            var toggleGuiData = Resources.Toggle;
            
            var toggleStyle = toggleGuiData.Style;
            
            float height = style.GetContentHeight();
            float toggleOffset = (height - toggleStyle.fixedHeight) / 2;

            var toggleRect = new Rect(
                rect.x, rect.y + toggleOffset,
                toggleStyle.fixedWidth, toggleStyle.fixedHeight
            );
            
            rect.x += height + toggleStyle.margin.right;

            if (!GUI.enabled && value) {
                GUI.DrawTexture(toggleRect, toggleGuiData.DisabledIcon);
            }
            else {
                value = EditorGUI.Toggle(toggleRect, GUIContent.none, value, toggleStyle);
            }
            EditorGUI.LabelField(rect, content, style);

            return value;
        }
        public static bool PrefixToggle(Rect rect, SerializedProperty value, GUIContent content, GUIStyle style) {
            return value.boolValue = PrefixToggle(rect, value.boolValue, content, style);
        }

        
        // RULE #1
        // background color -> normal/on normal
        // icon color -> active/on active
        
        // RULE #2
        // push textures in this way: all off - all on
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
        public static int EnumToggles(Rect rect, SerializedProperty value, Texture[] icons, GUIStyle style) {
            return value.intValue = EnumToggles(rect, value.intValue, icons, style);
        }
        
        
        private static string DoGenericPostfixInputField(Rect rect, string value, GUIContent postfixContent, GUIContent errorMessage) {
            var styles = Resources.InputField;
            bool isError = errorMessage != null && GUI.enabled;
            var valueStyle = isError ? styles.UnderlineMainError : styles.UnderlineMainNormal;
            var postfixStyle = styles.Postfix;

            
            rect.width = valueStyle.fixedWidth;
            
            // Value & main background
            var expression = EditorGUI.DelayedTextField(rect, value, valueStyle);

            // Postfix
            if (Event.current.type == EventType.Repaint) {
                postfixStyle.Draw(rect, postfixContent, false, false,false,false);
            }
            
            // Error message
            if (isError) {
                var errorMessageStyle = styles.UnderlineErrorMessage;
                var errorRect = LayoutEngine.RequestLayoutRect(errorMessageStyle.GetContentHeight());
                errorRect.x = rect.x;
                
                EditorGUI.LabelField(
                    errorRect,
                    errorMessage,
                    errorMessageStyle
                );
            }

            return expression;
        }
        public static float FloatPostfixInputField(Rect rect, float value, string postfixContent, GUIContent errorMessage) {
            bool changed = GUI.changed;
            EditorGUI.BeginChangeCheck();
            var expression = DoGenericPostfixInputField(rect, value.ToString(CultureInfo.InvariantCulture), TempContent(postfixContent), errorMessage);
            if (EditorGUI.EndChangeCheck()) {
                if (ExpressionEvaluator.Evaluate(expression, out float newVal) && newVal != value)
                    return newVal;
                GUI.changed = changed;
            }

            return value;
        }
        public static float FloatPostfixInputField(Rect rect, SerializedProperty value, string postfixContent, GUIContent errorMessage = null) {
            return value.floatValue = FloatPostfixInputField(rect, value.floatValue, postfixContent, errorMessage);
        }
        public static int IntPostfixInputField(Rect rect, int value, string postfixContent, GUIContent errorMessage) {
            bool changed = GUI.changed;
            EditorGUI.BeginChangeCheck();
            var expression = DoGenericPostfixInputField(rect, value.ToString(CultureInfo.InvariantCulture), TempContent(postfixContent), errorMessage);
            if (EditorGUI.EndChangeCheck()) {
                if (ExpressionEvaluator.Evaluate(expression, out int newVal) && newVal != value)
                    return newVal;
                GUI.changed = changed;
            }
            return value;
        }
        public static int IntPostfixInputField(Rect rect, SerializedProperty value, string postfixContent, GUIContent errorMessage = null) {
            return value.intValue = IntPostfixInputField(rect, value.intValue, postfixContent, errorMessage);
        }
        
        
        // TODO [assertion]: add check for input style for border >= 1
        public static bool FoldoutUnderline(Rect rect, bool expanded, GUIContent label) {
            var styles = Resources.Foldout;
            var labelStyle = styles.UnderlineLabelStyle;
            var arrowStyle = styles.ArrowIconStyle;
            
            var margin = labelStyle.margin;
            
            rect.x += margin.left;
            rect.width -= margin.right + margin.left;

            // Underline
            float lineHeight = labelStyle.border.bottom;
            var underlineRect = new Rect(rect.x, rect.yMax - lineHeight, rect.width, lineHeight);
            EditorGUI.DrawRect(underlineRect, labelStyle.onNormal.textColor);

            // Arrow
            if (Event.current.type == EventType.Repaint) {
                var arrowRect = new Rect(
                    rect.x,
                    rect.y + (rect.height - labelStyle.padding.vertical) / 2 + labelStyle.padding.top -
                    arrowStyle.fixedHeight / 2,
                    rect.height,
                    rect.width
                );
                arrowStyle.Draw(arrowRect, false, false, expanded, false);
            }
            
            // Foldout
            rect.xMin += FOLDOUT_DEFAULT_MARGIN + arrowStyle.fixedWidth + arrowStyle.padding.right;
            return EditorGUI.Foldout(rect, expanded, label, true, labelStyle);
        }
        public static bool FoldoutUnderline(Rect rect, SerializedProperty expanded, GUIContent label) {
            return expanded.boolValue = FoldoutUnderline(rect, expanded.boolValue, label);
        }
        public static bool FoldoutUnderline(bool expanded, GUIContent label) {
            var autoRect = LayoutEngine.RequestLayoutRect(Resources.Foldout.UnderlineLabelStyle.GetContentHeight());
            return FoldoutUnderline(autoRect, expanded, label);
        }
        public static bool FoldoutUnderline(SerializedProperty expanded, GUIContent label) {
            return expanded.boolValue = FoldoutUnderline(expanded.boolValue, label);
        }
        
        
        public static void ColorField(Rect rect, SerializedProperty color) {
            rect.width = Resources.ColorField.Style.fixedWidth;
            color.colorValue = EditorGUI.ColorField(
                rect, GUIContent.none, color.colorValue, 
                false, true, false
            );
        }
    }
}