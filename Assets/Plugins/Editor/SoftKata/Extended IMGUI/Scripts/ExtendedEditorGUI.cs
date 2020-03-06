using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        private static UnityAction _currentViewRepaint;

        public static UnityAction CurrentViewRepaint {
            get {
                if(_currentViewRepaint != null){
                    return _currentViewRepaint;
                }
                throw new Exception("Accessing CurrentRepaint without initialization via InitEditor(...) or InitEditorWindow(...)");
            }
        }

        public static void InitEditor(Editor editor) {
            _currentViewRepaint = editor.Repaint;
        }
        public static void InitEditorWindow(EditorWindow editorWindow) {
            _currentViewRepaint = editorWindow.Repaint;
        }
    }
}