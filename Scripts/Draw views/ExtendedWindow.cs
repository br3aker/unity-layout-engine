using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public abstract class ExtendedWindow : EditorWindow, IRepaintable {
        private int _repaintRequestsCount;

        private LayoutGroup LayoutRoot;

        public event Action OnHeaderDraw;
        public event Action OnFooterDraw;

        // Initialization
        public void OnEnable() {
            ExtendedEditor.CurrentView = this;

            LayoutRoot = new VerticalGroup(ignoreConstaints: true);
            Initialize();
        }
        protected virtual void Initialize() { }

        // Drawing
        public void OnGUI() {
            if (Layout.BeginRootScope(LayoutRoot)) {
                OnHeaderDraw?.Invoke();
                DrawContent();
                OnFooterDraw?.Invoke();
                Layout.EndRootScope();
            }
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
    }
}
