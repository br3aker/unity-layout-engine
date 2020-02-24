using SoftKata.ExtendedEditorGUI;
using UnityEditor;
using UnityEngine;

namespace Development {
    public class DummyWindow : EditorWindow
    {
        [MenuItem("Window/Dummy window")]
        static void Init() {
            GetWindow<DummyWindow>(false, "Dummy window").Show();
        }
        
        private void Update() {
            Repaint();
        }

        private Vector2 _scrollPos;
        
        private void OnGUI() {
            EditorGUILayout.LabelField($"Sin: {Mathf.Sin((float)EditorApplication.timeSinceStartup)}");
            
            if (LayoutEngine.BeginScrollGroup(new Vector2(-1, position.height - 18), _scrollPos)) {
                for (int i = 0; i < 16; i++) {
                    if (LayoutEngine.BeginHorizontalGroup(GroupModifier.DiscardMargin)) {
                        if (Event.current.type == EventType.Layout) {
                            LayoutEngine.RegisterArray(16, 16, 150);
                        }
                        else {
                            for (int j = 0; j < 16; j++) {
                                if (LayoutEngine.GetRect(16, 150, out Rect rect)) {
                                    ExtendedEditorGUI.IntDelayedField(rect, 123, "postfix", null);
                                }
                            }
                        }
                    }
                    LayoutEngine.EndHorizontalGroup();
                }
            }
            _scrollPos = LayoutEngine.EndScrollGroup();
        }
    }
}