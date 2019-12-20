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

        _debugColors = new Color[1000];
        for (int i = 0; i < _debugColors.Length; i++) {
            _debugColors[i] = Random.ColorHSV();
        }
    }
    private void OnDestroy() {
        ExtendedEditorGUI.UnregisterUsage();
    }
    
    private void Update() {
        Repaint();
    }
    #endregion

    private void OnGUI() {
        AutoLayout.IsDebug = EditorGUILayout.Toggle("Layout engine debug ", AutoLayout.IsDebug);

        EditorGUILayout.LabelField(AutoLayout.GetEngineInfo());
        EditorGUILayout.LabelField(AutoLayout.GetCurrentLayoutGroupInfo());
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        VerticalFadeScopeTest();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
    }


    private float _testVerticalScrollPosition;
    private float _scrollGroupContainerSize = 250;
    
    private float _testHorizontalScrollPosition;
    private float _scrollGroupElementWidth = 90;
    
    private Color[] _debugColors;

    private AnimBool _folded = new AnimBool();

    private void VerticalFadeScopeTest() {
        _folded.target = EditorGUI.Foldout(AutoLayout.RequestLayoutRect(16), _folded.target, _folded.faded.ToString(), true);
        AutoLayout.BeginHorizontalFade(_folded.faded);
        {
            AutoLayout.BeginVerticalScope();
            {
                _testVerticalScrollPosition = EditorGUI.Slider(AutoLayout.RequestLayoutRect(16), _testVerticalScrollPosition, 0, 1);
                _scrollGroupContainerSize = EditorGUI.FloatField(AutoLayout.RequestLayoutRect(16), _scrollGroupContainerSize);
                AutoLayout.BeginVerticalScroll(_scrollGroupContainerSize, _testVerticalScrollPosition);
                {
                    foreach (var color in _debugColors) {
                        var rect = AutoLayout.RequestLayoutRect(30);
                        if (rect.IsValid()) {
                            EditorGUI.DrawRect(rect, color);
                        }
                    }
                }
                AutoLayout.EndVerticalScroll();
            }
            AutoLayout.EndVerticalScope();

            AutoLayout.BeginVerticalScope();
            {
                _testHorizontalScrollPosition = EditorGUI.Slider(AutoLayout.RequestLayoutRect(16),_testHorizontalScrollPosition, 0, 1);
                _scrollGroupElementWidth = EditorGUI.FloatField(AutoLayout.RequestLayoutRect(16), _scrollGroupElementWidth);
                AutoLayout.BeginHorizontalScroll(_scrollGroupElementWidth, _testHorizontalScrollPosition);
                {
                    foreach (var color in _debugColors) {
                        var rect = AutoLayout.RequestLayoutRect(80);
                        if (rect.IsValid()) {
                            EditorGUI.DrawRect(rect, color);
                        }
                    }
                }
                AutoLayout.EndHorizontalScroll();
            }
            AutoLayout.EndVerticalScope();
        }
        AutoLayout.EndHorizontalFade();
    }
}
