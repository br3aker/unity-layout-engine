using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static class ExtendedEditorGUILayout {
        public static int ToggleArray(int value, GUIContent[] contents) {
            var rect = LayoutEngine.GetRect(ExtendedEditorGUI.ToggleArrayHeight);
            return ExtendedEditorGUI.ToggleArray(rect, value, contents);
        }

        public static void ToggleArray(SerializedProperty value, GUIContent[] contents) {
            value.intValue = ToggleArray(value.intValue, contents);
        }

        public static int IntDelayedField(int value, string postfix, string errorMessage) {
            var rect = LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight);
            return ExtendedEditorGUI.IntDelayedField(rect, value, postfix, errorMessage);
        }

        public static void IntDelayedField(SerializedProperty value, string postfix, string errorMessage) {
            value.intValue = IntDelayedField(value.intValue, postfix, errorMessage);
        }

        public static float FloatDelayedField(float value, string postfix, string errorMessage) {
            var rect = LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight);
            return ExtendedEditorGUI.FloatDelayedField(rect, value, postfix, errorMessage);
        }

        public static void FloatDelayedField(SerializedProperty value, string postfix, string errorMessage) {
            value.floatValue = FloatDelayedField(value.floatValue, postfix, errorMessage);
        }

        public static bool UnderlineFoldout(bool expanded, string label) {
            var rect = LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight);
            return ExtendedEditorGUI.UnderlineFoldout(rect, expanded, label);
        }

        public static void UnderlineFoldout(SerializedProperty expanded, string label) {
            var rect = LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight);
            expanded.isExpanded = ExtendedEditorGUI.UnderlineFoldout(rect, expanded.isExpanded, label);
        }

        public static ExtendedEditorGUI.KeyboardShortcut
            KeyboardShortcutField(ExtendedEditorGUI.KeyboardShortcut value) {
            var rect = LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight);
            return ExtendedEditorGUI.KeyboardShortcutField(rect, value);
        }
    }
}