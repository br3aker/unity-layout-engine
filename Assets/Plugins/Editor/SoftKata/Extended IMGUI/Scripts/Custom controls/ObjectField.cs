//using UnityEditor;
//using UnityEngine;
//
//
//namespace SoftKata.ExtendedEditorGUI {
//    public static partial class ExtendedEditorGUI {
//        private static readonly int s_ComponentObjectFieldHint = nameof(s_ComponentObjectFieldHint).GetHashCode();
//
//        public static void ComponentObjectField<T>(
//            Rect rect, 
//            SerializedProperty property,
//            bool allowSceneObjects) where T: UnityEngine.Object {
//
//            var style = Resources.ObjectField.Style;
//            var objectPickerIcon = Resources.ObjectField.OpenObjectPickerIcon;
//            
//            int controlId = GUIUtility.GetControlID(s_ComponentObjectFieldHint, FocusType.Keyboard, rect);
//            Event current = Event.current;
//            EventType eventType = current.GetTypeForControl(controlId);
//            
//            UnityEngine.Object objRef = property.objectReferenceValue;
//
//            bool isObjectFieldActive = 
//                GUIUtility.keyboardControl == controlId || DragAndDrop.activeControlID == controlId;
//
//            rect.width = style.fixedWidth - style.fixedHeight;
//            var objectPickerButtonRect = new Rect(
//                rect.xMax, rect.y, rect.height, rect.height
//            );
//
//            switch (eventType) {
//                case EventType.MouseDown:
//                    if (rect.Contains(current.mousePosition)) {
//                        if (current.clickCount == 1) {
//                            if (GUIUtility.keyboardControl != controlId) {
//                                GUIUtility.keyboardControl = controlId;
//                            }
//
//                            if (objRef != null) {
//                                EditorGUIUtility.PingObject(objRef);
//                            }
//                            else {
//                                EditorGUIUtility.ShowObjectPicker<T>(objRef, allowSceneObjects, "", controlId);
//                                GUIUtility.ExitGUI();
//                            }
//                        }
//                        else if(current.clickCount == 2) {
//                            if (objRef != null) {
//                                AssetDatabase.OpenAsset(objRef);
//                            }
//                        }
//                        current.Use();
//                    }
//                    else if(objectPickerButtonRect.Contains(current.mousePosition)){
//                        EditorGUIUtility.ShowObjectPicker<T>(objRef, allowSceneObjects, "", controlId);
//                        GUIUtility.ExitGUI();
//                    }
//                    break;
//                case EventType.KeyDown:
//                    if (GUIUtility.keyboardControl == controlId) {
//                        if (current.keyCode == KeyCode.Backspace || current.keyCode == KeyCode.Delete) {
//                            property.objectReferenceValue = null;
//                            current.Use();
//                        }
//                        if (current.keyCode == KeyCode.Return) {
//                            EditorGUIUtility.ShowObjectPicker<T>(objRef, allowSceneObjects, "", controlId);
//                            current.Use();
//                            GUIUtility.ExitGUI();
//                        }
//                    }
//                    break;
//                case EventType.ExecuteCommand:
//                    if (EditorGUIUtility.GetObjectPickerControlID() == controlId) {
//                        if (current.commandName == "ObjectSelectorUpdated") {
//                            var objectReference = EditorGUIUtility.GetObjectPickerObject() as GameObject;
//                            if (objectReference == null) {
//                                property.objectReferenceValue = null;
//                            }
//                            else {
//                                property.objectReferenceValue = objectReference.GetComponent<T>();
//                            }
//
//                            current.Use();
//                        }
//                    }
//                    break;
//                case EventType.Repaint:
//                    // input field
//                    var content = objRef ? TempContent(objRef.name) : TempContent("(empty)");    
//                    style.Draw(rect, content, controlId, isObjectFieldActive);
//                    // object picker button
//                    GUI.DrawTexture(objectPickerButtonRect, objectPickerIcon);
//                    break;
//                case EventType.DragUpdated:
//                    if (DragAndDrop.activeControlID == 0 && rect.Contains(current.mousePosition)) {
//                        DragAndDrop.activeControlID = controlId;
//                        if ((DragAndDrop.objectReferences[0] as GameObject)?.GetComponent<T>()) {
//                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
//                        }
//                        else {
//                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
//                        }
//                        current.Use();
//                    }
//                    break;
//                case EventType.DragPerform:
//                    if (rect.Contains(current.mousePosition)) {
//                        if (DragAndDrop.objectReferences.Length >= 1) {
//                            var objectReference = (DragAndDrop.objectReferences[0] as GameObject).GetComponent<T>();
//                            property.objectReferenceValue = objectReference;
//                        }
//                    }
//                    break;
//            }
//        }
//    }
//}

