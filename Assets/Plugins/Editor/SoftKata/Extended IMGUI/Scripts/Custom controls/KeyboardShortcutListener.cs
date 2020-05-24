using UnityEngine;

namespace SoftKata.EditorGUI {
    public partial class ExtendedEditorGUI {
        private static readonly int s_KeyboardShortcutListenerHint =
            nameof(s_KeyboardShortcutListenerHint).GetHashCode();

        private static readonly ShortcutFieldState _shortcutFieldStateObject = new ShortcutFieldState();

        public static KeyboardShortcut KeyboardShortcutField(Rect rect, KeyboardShortcut value) {
            var controlId = GUIUtility.GetControlID(s_KeyboardShortcutListenerHint, FocusType.Keyboard, rect);

            var current = Event.current;
            var eventType = current.GetTypeForControl(controlId);

            var lastRecordingControlId = _shortcutFieldStateObject.LastRecordingControlId;

            var isRecording = controlId == GUIUtility.keyboardControl &&
                              controlId == lastRecordingControlId;

            var postfixRect = new Rect(rect.xMax - PostfixIconAreaWidth, rect.y, PostfixIconAreaWidth, rect.height);

            switch (eventType) {
                case EventType.MouseDown:
                    // Main rect click
                    // Used for keyboard/focus control capture
                    if (rect.Contains(current.mousePosition)) {
                        GUIUtility.hotControl = controlId;
                        GUIUtility.keyboardControl = controlId;

                        // Postfix rect click
                        // Used for immediate record start
                        if (postfixRect.Contains(current.mousePosition)) {
                            if (controlId == lastRecordingControlId) {
                                value = _shortcutFieldStateObject.LastRecordingControlValue;
                                _shortcutFieldStateObject.LastRecordingControlId = NoActiveControlId;
                            }
                            else {
                                _shortcutFieldStateObject.LastRecordingControlValue = value;
                                _shortcutFieldStateObject.LastRecordingControlId = controlId;
                            }
                        }

                        current.Use();
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == controlId) {
                        var keyCode = current.keyCode;

                        if (keyCode == KeyCode.None) break;

                        if (isRecording) {
                            switch (keyCode) {
                                case KeyCode.Escape:
                                case KeyCode.Tab:
                                    _shortcutFieldStateObject.LastRecordingControlId = NoActiveControlId;
                                    break;
                                case KeyCode.Return:
                                    value = _shortcutFieldStateObject.LastRecordingControlValue;
                                    _shortcutFieldStateObject.LastRecordingControlId = NoActiveControlId;
                                    break;
                                default:
                                    _shortcutFieldStateObject.LastRecordingControlValue.Set(keyCode, current.modifiers);
                                    break;
                            }

                            current.Use();
                        }
                        else {
                            // delete current selection
                            if (current.keyCode == KeyCode.Delete || current.keyCode == KeyCode.Backspace) {
                                value = new KeyboardShortcut {Key = KeyCode.None, Modifiers = EventModifiers.None};
                                current.Use();
                            }
                            // start listening for a new shortcut
                            else if (keyCode == KeyCode.Return) {
                                _shortcutFieldStateObject.LastRecordingControlValue = value;
                                _shortcutFieldStateObject.LastRecordingControlId = controlId;
                                current.Use();
                            }
                        }
                    }

                    break;
                case EventType.Repaint:
                    var resources = Resources.ShortcutRecorder;

                    // Label
                    var label = (isRecording ? _shortcutFieldStateObject.LastRecordingControlValue : value).ToString();
                    resources.Style.Draw(rect, TempContent(label), controlId, isRecording);

                    // Postfix
                    Resources.InputFieldPostfix.Draw(postfixRect, TempContent(null),
                        postfixRect.Contains(current.mousePosition), false, false, false);

                    var coordsRect = new Rect(isRecording ? 0.5f : 0f, 0f, 0.5f, 1);
                    var postfixIconRect = new Rect(
                        postfixRect.x + 1,
                        postfixRect.y + LabelHeight / 2 - PostfixIconSize / 2,
                        PostfixIconSize,
                        PostfixIconSize
                    );

                    GUI.DrawTextureWithTexCoords(postfixIconRect, resources.RecordStateIconSet, coordsRect);

                    break;
            }

            return value;
        }

        public struct KeyboardShortcut {
            private const string DefaultShortcutRecorderLabel = "No shortcut";

            public KeyCode Key;
            public EventModifiers Modifiers;

            private string _stringRepresentation;

            public void Set(KeyCode key, EventModifiers modifiers) {
                Key = key;
                Modifiers = modifiers;

                if (key != KeyCode.None) {
                    _stringRepresentation = key.ToString();
                    if (modifiers != EventModifiers.None) _stringRepresentation += " + " + modifiers;
                }
                else {
                    Reset();
                }
            }

            public void Reset() {
                Key = KeyCode.None;
                Modifiers = EventModifiers.None;

                _stringRepresentation = null;
            }

            public override string ToString() {
                return _stringRepresentation ?? DefaultShortcutRecorderLabel;
            }
        }

        private class ShortcutFieldState {
            public int LastRecordingControlId = -1;
            public KeyboardShortcut LastRecordingControlValue;
        }
    }
}