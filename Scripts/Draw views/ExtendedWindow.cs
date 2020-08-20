using System;
using UnityEditor;

namespace SoftKata.UnityEditor {
    public abstract class ExtendedWindow : EditorWindow, IRepaintable {
        [NonSerialized]
        private int _repaintRequestsCount;

        public void OnEnable() {
            ExtendedEditor.CurrentView = this;
            Initialize();
        }
        protected abstract void Initialize();

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
