using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public abstract class ExtendedWindow : EditorWindow, IRepaintable {
        private int _repaintRequestsCount;

        // Initialization
        public void OnEnable() {
            ExtendedEditor.CurrentView = this;
            Initialize();
        }
        protected virtual void Initialize() { }

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
    }
}
