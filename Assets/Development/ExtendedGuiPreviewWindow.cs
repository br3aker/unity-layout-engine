 using System;
 using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

using SoftKata.ExtendedEditorGUI;
 using TMPro;
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
        
        _verticalFaded1 = new AnimBool(false);
        _verticalFaded1.valueChanged.AddListener(Repaint);
        _verticalFaded2 = new AnimBool(false);
        _verticalFaded2.valueChanged.AddListener(Repaint);
    }
    private void OnDestroy() {
        ExtendedEditorGUI.UnregisterUsage();
    }
    #endregion

    
    private void Update() {
        //Repaint();
    }
    
    private void OnGUI() {
        EditorGUILayout.LabelField($"Layout groups count: {AutoLayout.GetTotalGroupCount().ToString()}");
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
//            NestedFadeGroupsTest();
        }
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        if (Event.current.type != EventType.Layout) {
            _verticalElementsCount = verticalCount;
            _horizontalElementsCount = horizontalCount;

            _verticalScrollPosition = verticalScroll;
            _horizontalScrollPosition = horizontalScroll;
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
        else {
            AutoLayout.ScrapGroups(_verticalElementsCount);
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
        else {
            AutoLayout.ScrapGroups(_horizontalElementsCount);
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
        AutoLayout.BeginVerticalScrollGroup(150, _verticalScrollPosition);
        for (int i = 0; i < _verticalElementsCount; i++) {
            HorizontalGroupTest();
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
        AutoLayout.BeginHorizontalScrollGroup(EditorGUIUtility.currentViewWidth, _horizontalScrollPosition);
        for (int i = 0; i < _horizontalElementsCount; i++) {
            VerticalGroupTest();
        }
        _horizontalScrollPosition = AutoLayout.EndHorizontalScrollGroup();
    }
    
    private void ScrollGridTest() {
        int validVerticalGroupsCount = 0;
        if (AutoLayout.BeginHorizontalScrollGroup(EditorGUIUtility.currentViewWidth, _horizontalScrollPosition)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                if (AutoLayout.BeginVerticalScrollGroup(250, _verticalScrollPosition)) {
                    validVerticalGroupsCount++;
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
        else {
            AutoLayout.ScrapGroups(_horizontalElementsCount);
        }
        _horizontalScrollPosition = AutoLayout.EndHorizontalScrollGroup();

        EditorGUI.LabelField(AutoLayout.RequestLayoutRect(16), $"Valid vertical groups: {validVerticalGroupsCount}");
    }

    private AnimBool _verticalFaded1;
    private AnimBool _verticalFaded2;

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
            EditorGUI.DrawRect(AutoLayout.RequestLayoutRect(200), Color.magenta);
            var nestedFoldoutRect = AutoLayout.RequestLayoutRect(16);
            if (nestedFoldoutRect.IsValid()) {
                _verticalFaded2.target = EditorGUI.Foldout(nestedFoldoutRect, _verticalFaded2.target, "Oh my", true);
            }
            if (AutoLayout.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                var contentRect = AutoLayout.RequestLayoutRect(200);
                if (contentRect.IsValid()) {
                    EditorGUI.DrawRect(contentRect, Color.magenta);
                }
            }
            AutoLayout.EndVerticalFadeGroup();
        }
        AutoLayout.EndVerticalFadeGroup();
    }
}
