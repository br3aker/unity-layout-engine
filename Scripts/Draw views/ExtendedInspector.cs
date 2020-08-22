using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public abstract class ExtendedInspector : Editor, IRepaintable {
        private int _repaintRequestsCount;

        // Initialization
        public void OnEnable() {
            ExtendedEditor.CurrentView = this;
            Initialize();
        }
        protected virtual void Initialize() { }

        // Actual GUI
        public override void OnInspectorGUI() {
            DrawContent();

            ExtendedEditor.DrawElevationShadow(Vector2.zero);
        }
        protected abstract void DrawContent();

        // Repaint requests
        public void RegisterRepaintRequest() {
            if(0 == _repaintRequestsCount++) {
                EditorApplication.update += Repaint;
            }
        }
        public void UnregisterRepaintRequest() {
            if(0 == --_repaintRequestsCount) {
                EditorApplication.update -= Repaint;
            }
        }
    
        // We can control margins via layout group and its' GUIStyle settings
        public override bool UseDefaultMargins() => false;
    }
}