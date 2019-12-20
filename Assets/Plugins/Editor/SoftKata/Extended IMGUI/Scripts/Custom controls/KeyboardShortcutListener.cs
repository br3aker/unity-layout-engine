using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public partial class ExtendedEditorGUI {
        private static readonly int s_KeyboardShortcutListenerHint = nameof(s_KeyboardShortcutListenerHint).GetHashCode();
        private class KeyboardShortcutListenerStateObject {
            public bool IsListening;

            private KeyCode _keyCode;
            private KeyCode _keyCodeModifier;

            public void Set(KeyCode keyCode, EventModifiers modifiers) {
                if (keyCode == KeyCode.None) return;

                _keyCode = keyCode;
                _keyCodeModifier = EventModifierToKeycode(modifiers);
            }
            public void SaveCurrentShortcut(SerializedProperty property) {
                property.FindPropertyRelative("keyCode").intValue = (int) _keyCode;
                property.FindPropertyRelative("modifier").intValue = (int) _keyCodeModifier;
            }
            public void Reset() {
                _keyCode = KeyCode.None;
                _keyCodeModifier = KeyCode.None;
            }

            public void SynchronizeWithCurrentProperty(SerializedProperty property) {
                _keyCode = (KeyCode) property.FindPropertyRelative("keyCode").intValue;
                _keyCodeModifier = (KeyCode) property.FindPropertyRelative("modifier").intValue;
            }
            
            public string KeyBindingPropertyToString(SerializedProperty property) {
                if (IsListening) {
                    if (_keyCodeModifier == KeyCode.None) {
                        return _keyCode.ToString();
                    }
                    return string.Join(" + ", _keyCode, _keyCodeModifier);
                }
                var keyCodeProperty = (KeyCode) property.FindPropertyRelative("keyCode").intValue;
                var modifierProperty = (KeyCode) property.FindPropertyRelative("modifier").intValue;
                if (modifierProperty == KeyCode.None) {
                    return keyCodeProperty.ToString();
                }
                return String.Join(" + ", keyCodeProperty, modifierProperty);
            }

            // TODO [editor/quality-of-life]: Right now this method only supports left variants of modifying keys, right keys support is a thing or not? 
            private static KeyCode EventModifierToKeycode(EventModifiers modifier) {
                // This method only support these modifier keys:
                // Shift, Control, Alt, Command (mac os)
                if ((modifier & EventModifiers.Shift) == EventModifiers.Shift) {
                    return KeyCode.LeftShift;
                }
                if ((modifier & EventModifiers.Alt) == EventModifiers.Alt) {
                    return KeyCode.LeftAlt;
                }
                if ((modifier & EventModifiers.Control) == EventModifiers.Control) {
                    return KeyCode.LeftControl;
                }
                #if UNITY_EDITOR_OSX
                if ((modifier & EventModifiers.Command) == EventModifiers.Command) {
                    return KeyCode.LeftCommand;
                }
                #endif

                return KeyCode.None;
            }
            
        }
        public static void KeyboardShortcutListener(Rect rect, SerializedProperty property) {
            int controlID = GUIUtility.GetControlID(s_KeyboardShortcutListenerHint, FocusType.Keyboard, rect);

            var current = Event.current;
            var eventType = current.GetTypeForControl(controlID);

            var stateObject = 
                (KeyboardShortcutListenerStateObject)GUIUtility.GetStateObject(
                    typeof(KeyboardShortcutListenerStateObject), 
                    controlID
                );
            
            var isListening = stateObject.IsListening;

            switch (eventType) {
                case EventType.MouseDown:
                    if (GUIUtility.hotControl == 0) {
                        if (rect.Contains(current.mousePosition)) {
                            GUIUtility.hotControl = controlID;
                            GUIUtility.keyboardControl = controlID;
                            
                            current.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        if (rect.Contains(current.mousePosition)) {
                            isListening = !isListening;
                        }
                        else {
                            isListening = false;
                        }

                        if (isListening) {
                            stateObject.SynchronizeWithCurrentProperty(property);
                        }
                        else {
                            GUIUtility.hotControl = 0;
                            stateObject.SaveCurrentShortcut(property);
                        }

                        stateObject.IsListening = isListening;

                        current.Use();
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == controlID) {
                        if (!current.isKey || current.keyCode == KeyCode.None) return;

                        var isReturnPressed = current.keyCode == KeyCode.Return;
                        
                        if (isListening) {
                            // cancel/apply current shortcut
                            if (current.keyCode == KeyCode.Escape || current.keyCode == KeyCode.Tab || isReturnPressed) {
                                GUIUtility.hotControl = 0;
                                stateObject.IsListening = false;
                                
                                if (isReturnPressed) {
                                    stateObject.SaveCurrentShortcut(property);
                                }
                                
                                current.Use();
                            }
                            else {
                                // register keyboard keys
                                stateObject.Set(current.keyCode, current.modifiers);
                                current.Use();
                            }
                        }
                        else {
                            // delete current selection
                            if (current.keyCode == KeyCode.Delete || current.keyCode == KeyCode.Backspace) {
                                stateObject.Reset();
                                stateObject.SaveCurrentShortcut(property);
                                current.Use();
                            }
                            // start listening for a new shortcut
                            else if (isReturnPressed) {
                                GUIUtility.hotControl = controlID;
                                stateObject.IsListening = true;
                                stateObject.SynchronizeWithCurrentProperty(property);
                                current.Use();
                            }
                        }
                    }
                    break;
                case EventType.Repaint:
                    var content = TempContent(stateObject.KeyBindingPropertyToString(property));
                    Resources.KeyboardListener.MainLabel.Draw(rect, content, controlID, isListening);
                    break;
            }
        }
    }
}