using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

using SoftKata.ExtendedEditorGUI;

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
        
        _verticalFaded1 = new AnimBool(false);
        _verticalFaded1.valueChanged.AddListener(Repaint);
        _verticalFaded2 = new AnimBool(false);
        _verticalFaded2.valueChanged.AddListener(Repaint);
        _verticalFaded3 = new AnimBool(false);
        _verticalFaded3.valueChanged.AddListener(Repaint);
        _verticalFaded4 = new AnimBool(false);
        _verticalFaded4.valueChanged.AddListener(Repaint);
    }
    private void OnDestroy() {
        ExtendedEditorGUI.UnregisterUsage();
    }
    #endregion

    
    private void Update() {
        //Repaint();
    }
    
    private void OnGUI() {
        var verticalCount = EditorGUILayout.IntField("Vertical count: ", _verticalElementsCount);
        var horizontalCount = EditorGUILayout.IntField("Horizontal count: ", _horizontalElementsCount);
        var verticalScroll = EditorGUILayout.Slider("Vertical scroll: ", _verticalScrollPosition, 0f, 1f);
        var horizontalScroll = EditorGUILayout.Slider("Horizontal scroll: ", _horizontalScrollPosition, 0f, 1f);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        {
//            VerticalGroupTest();
//            HorizontalGroupTest();
//            VH_GridGroupTest();
//            HV_GridGroupTest();
//            VerticalScrollGroupTest();
//            VerticalScrollGroupHorizontalInternalsTest();
//            HorizontalScrollGroupTest();
//            HorizontalScrollGroupVerticalInternalsTest();
//            ScrollGridTest();
//            VerticalFadeGroupTest();
//            VerticalFadeGroupHorizontalChildGroupTest();
//            VerticalFadeGroupGridTest();
//            VerticalFadeGroupScrollGridTest();
            NestedFadeGroupsTest();
        }
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        PrintLayoutGroupsData();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        
        if (Event.current.type != EventType.Layout) {
            _verticalElementsCount = verticalCount;
            _horizontalElementsCount = horizontalCount;

            _verticalScrollPosition = verticalScroll;
            _horizontalScrollPosition = horizontalScroll;
            
            _layoutGroupsData = _rawLayoutGroupsData;
        }
        else {
            _groupCount = AutoLayout.GetTotalGroupCount();
            _rawLayoutGroupsData = AutoLayout.GetLayoutGroupsData();
        }
    }

    
    private AutoLayout.LayoutGroupDebugData[] _layoutGroupsData = new AutoLayout.LayoutGroupDebugData[0];
    private AutoLayout.LayoutGroupDebugData[] _rawLayoutGroupsData;
    private int _groupCount;
    private void PrintLayoutGroupsData() {
        EditorGUILayout.LabelField($"Layout groups count: {_groupCount}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        foreach (var data in _layoutGroupsData) {
            if (data.Valid) {
                var color = GUI.color;
                GUI.color = Color.green;
                EditorGUILayout.LabelField(data.Data);
                GUI.color = color;
            }
            else {
                GUI.enabled = false;
                EditorGUILayout.LabelField(data.Data);
                GUI.enabled = true;
            }
        }
    }
    
    
    private int _verticalElementsCount = 1;
    private int _horizontalElementsCount = 1;

    private float _verticalScrollPosition = 0f;
    private float _horizontalScrollPosition = 0f;


    private void VerticalGroupTest() {
        if (AutoLayout.BeginVerticalGroup()) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = AutoLayout.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                }
            }
        }
        AutoLayout.EndVerticalGroup();
    }
    private void HorizontalGroupTest() {
        if (AutoLayout.BeginHorizontalGroup()) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                var rect = AutoLayout.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                }
            }
        }
        AutoLayout.EndHorizontalGroup();
    }
    private void VH_GridGroupTest() {
        if(AutoLayout.BeginVerticalGroup())
        {
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (AutoLayout.BeginHorizontalGroup()) {
                    for (int j = 0; j < _horizontalElementsCount; j++) {
                        var rect = AutoLayout.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i + j, "prefix", null);
                        }
                    }
                }
                AutoLayout.EndHorizontalGroup();
            }
        }
        AutoLayout.EndVerticalGroup();
    }
    private void HV_GridGroupTest() {
        if(AutoLayout.BeginHorizontalGroup()) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                if (AutoLayout.BeginVerticalGroup()) {
                    for (int j = 0; j < _verticalElementsCount; j++) {
                        var rect = AutoLayout.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i + j, "prefix", null);
                        }
                    }
                }
                AutoLayout.EndVerticalGroup();
            }
        }
        AutoLayout.EndHorizontalGroup();
    }

    private void VerticalScrollGroupTest() {
        AutoLayout.BeginVerticalScrollGroup(150, _verticalScrollPosition);
        for (int i = 0; i < _verticalElementsCount; i++) {
            var rect = AutoLayout.RequestLayoutRect(16, 150);
            if (rect.IsValid()) {
                ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
            }
        }
        _verticalScrollPosition = AutoLayout.EndVerticalScrollGroup();
    }
    private void VerticalScrollGroupHorizontalInternalsTest() {
        if (AutoLayout.BeginVerticalScrollGroup(150, _verticalScrollPosition)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                HorizontalGroupTest();
            }
        }

        _verticalScrollPosition = AutoLayout.EndVerticalScrollGroup();
    }
    
    private void HorizontalScrollGroupTest() {
        AutoLayout.BeginHorizontalScrollGroup(EditorGUIUtility.currentViewWidth, _horizontalScrollPosition);
        for (int i = 0; i < _horizontalElementsCount; i++) {
            var rect = AutoLayout.RequestLayoutRect(16, 120);
            if (rect.IsValid()) {
                ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
            }
        }
        _horizontalScrollPosition = AutoLayout.EndHorizontalScrollGroup();
    }
    private void HorizontalScrollGroupVerticalInternalsTest() {
        if (AutoLayout.BeginHorizontalScrollGroup(EditorGUIUtility.currentViewWidth, _horizontalScrollPosition)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                VerticalGroupTest();
            }
        }
        _horizontalScrollPosition = AutoLayout.EndHorizontalScrollGroup();
    }
    
    private void ScrollGridTest() {
        if (AutoLayout.BeginHorizontalScrollGroup(EditorGUIUtility.currentViewWidth, _horizontalScrollPosition)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                if (AutoLayout.BeginVerticalScrollGroup(250, _verticalScrollPosition)) {
                    for (int j = 0; j < _verticalElementsCount; j++) {
                        var rect = AutoLayout.RequestLayoutRect(16, 120);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i + j, "prefix", null);
                        }
                    }
                }
                _verticalScrollPosition = AutoLayout.EndVerticalScrollGroup();
            }
        }
        _horizontalScrollPosition = AutoLayout.EndHorizontalScrollGroup();
    }

    private AnimBool _verticalFaded1;
    private AnimBool _verticalFaded2;
    private AnimBool _verticalFaded3;
    private AnimBool _verticalFaded4;

    private void VerticalFadeGroupTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = AutoLayout.RequestLayoutRect(16);
                if (rect.IsValid()) {
                    ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                }
            }
        }
        AutoLayout.EndVerticalFadeGroup();
    }
    private void VerticalFadeGroupHorizontalChildGroupTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            HorizontalGroupTest();
        }
        AutoLayout.EndVerticalFadeGroup();
    }
    private void VerticalFadeGroupGridTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            VH_GridGroupTest();
        }
        AutoLayout.EndVerticalFadeGroup();
    }
    private void VerticalFadeGroupScrollGridTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            ScrollGridTest();
        }
        AutoLayout.EndVerticalFadeGroup();
    }
    private void NestedFadeGroupsTest() {
        _verticalFaded1.target = EditorGUI.Foldout(AutoLayout.RequestLayoutRect(16), _verticalFaded1.target, "Vertical foldout example", true);
        if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            var foldoutRect1 = AutoLayout.RequestLayoutRect(16);
            if (foldoutRect1.IsValid()) {
                _verticalFaded2.target = EditorGUI.Foldout(foldoutRect1, _verticalFaded2.target, "Oh my", true);
            }
            if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                var foldoutRect2 = AutoLayout.RequestLayoutRect(16);
                if (foldoutRect2.IsValid()) {
                    _verticalFaded3.target = EditorGUI.Foldout(foldoutRect2, _verticalFaded3.target, "Vertical foldout example", true);
                }
                if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded3.faded)) {
                    var foldoutRect3 = AutoLayout.RequestLayoutRect(16);
                    if (foldoutRect3.IsValid()) {
                        _verticalFaded4.target = EditorGUI.Foldout(foldoutRect3, _verticalFaded4.target, "Oh my", true);
                    }
                    if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded4.faded)) {
                        var contentRect = AutoLayout.RequestLayoutRect(200);
                        if (contentRect.IsValid()) {
                            EditorGUI.DrawRect(contentRect, Color.magenta);
                        }
                    }
                    AutoLayout.EndVerticalFadeGroup();
                }
                AutoLayout.EndVerticalFadeGroup();
            }
            AutoLayout.EndVerticalFadeGroup();
        }
        AutoLayout.EndVerticalFadeGroup();
    }
}
