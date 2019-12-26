using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

using SoftKata.ExtendedEditorGUI;
using UnityEngine.Profiling;

public class ExtendedGuiPreviewWindow : EditorWindow
{
    #region Window initialization & lifetime management
    [MenuItem("Window/Extended GUI preview")]
    static void Init()
    {
        ExtendedGuiPreviewWindow window = GetWindow<ExtendedGuiPreviewWindow>();
        window.titleContent = new GUIContent("GUI preview");
        window.Show();
    }
    
    private void OnEnable() {
        ExtendedEditorGUI.RegisterUsage();

        _folded = new AnimBool(true);
    }
    private void OnDestroy() {
        ExtendedEditorGUI.UnregisterUsage();
    }
    #endregion

    
    private void Update() {
        Repaint();
    }
    
    private void OnGUI() {
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        VerticalFadeScopeTest();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
    }


    private float _testVerticalScrollPosition;
    
    private float _testHorizontalScrollPosition;
    

    private AnimBool _folded;

    private void VerticalFadeScopeTest() {
        _folded.target = EditorGUI.Foldout(AutoLayout.RequestLayoutRect(16), _folded.target, _folded.faded.ToString(), true);

        if (AutoLayout.BeginHorizontalFade(_folded.faded)) {
            AutoLayout.BeginVerticalGroup();
            {
                AutoLayout.BeginVerticalScroll(250, _testVerticalScrollPosition);
                {
                    for (int i = 0; i < 100; i++) {
                        var rect = AutoLayout.RequestLayoutRect(30);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                        }
                    }
                }
                _testVerticalScrollPosition = AutoLayout.EndVerticalScroll();
            }
            AutoLayout.EndVerticalGroup();

            AutoLayout.BeginVerticalGroup();
            {
                AutoLayout.BeginHorizontalScroll(100, _testHorizontalScrollPosition);
                {
                    for (int i = 0; i < 25; i++) {
                        AutoLayout.BeginVerticalGroup();
                        for (int j = 0; j < 4; j++) {
                            var rect = AutoLayout.RequestLayoutRect(16);
                            if (rect.IsValid()) {
                                ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                            }
                        }

                        AutoLayout.EndVerticalGroup();
                    }
                }
                _testHorizontalScrollPosition = AutoLayout.EndHorizontalScroll();
            }
            AutoLayout.EndVerticalGroup();
            AutoLayout.EndHorizontalFade();
        }
    }
}
