using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class ExtendedEditorWindow : EditorWindow {
        private Vector2 _windowScroll;
        
        public void OnGUI() {
            if(Event.current.type == EventType.Used) return;
            if(Event.current.type == EventType.Layout) LayoutEngine.ResetEngine();

            LayoutEngine.BeginScrollGroup(position.width, position.height, _windowScroll);
            IMGUI();
            _windowScroll = LayoutEngine.EndScrollGroup();
        }
        
        protected abstract void IMGUI();
    }
}