﻿using UnityEditor;
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
        Repaint();
    }
    
    private void OnGUI() {
        layoutDebugData = LayoutEngine.GetEngineGroupHierarchyData();
        
        var verticalCount = EditorGUILayout.IntField("Vertical count: ", _verticalElementsCount);
        var horizontalCount = EditorGUILayout.IntField("Horizontal count: ", _horizontalElementsCount);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        {
//            VerticalGroupTest();    // passed
//            VerticalGroupsPlainTest();    // passed
//            VerticalGroupsIfCheckTest();    // passed
//            VerticalUnityNativeTest();    // utility
//            HorizontalGroupTest();    // passed
//            UnityImplementationScrollTest();    // utility
//            VerticalFadeGroupTest();    // passed
//            VerticalFadeGroupHorizontalChildGroupTest();    // passed
//            VerticalFadeGroupGridTest();    // passed
//            NestedFadeGroupsTest();    // passed
//            VerticalSeparatorGroupTest();    // passed
//            VerticalHierarchyGroupTest();    // passed
//            VerticalHierarchyWithSeparatorTest();    // passed
//            FixedHorizontalGroupTest();    // passed
//            FixedHorizontalGroupVerticalChildrenTest();    // passed
//            FixedHorizontalGroupComplexInternalsTest();    // passed
            HybridScrollGroupTest();    // passed
        }

        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"Hot control id: {EditorGUIUtility.hotControl}");
        EditorGUILayout.LabelField($"Keyboard control id: {EditorGUIUtility.keyboardControl}");
        EditorGUILayout.LabelField($"Current view width: {EditorGUIUtility.currentViewWidth}");

        DrawLayoutEngineDebugData();

        if (Event.current.type != EventType.Layout) {
            _verticalElementsCount = verticalCount;
            _horizontalElementsCount = horizontalCount;
        }
    }

    private LayoutEngine.LayoutDebugData[] layoutDebugData;
    
    private void DrawLayoutEngineDebugData() {
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"Root | Children count: {layoutDebugData.Length.ToString()}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        foreach (var groupData in layoutDebugData) {
            if (groupData.IsValid) {
                EditorGUILayout.LabelField(groupData.Data);
            }
            else {
                GUI.enabled = false;
                EditorGUILayout.LabelField(groupData.Data);
                GUI.enabled = true;
            }
        }
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
    }
    
    private int _verticalElementsCount = 16;
    private int _horizontalElementsCount = 16;

    private float _verticalScrollPosition = 0f;
    private float _horizontalScrollPosition = 0f;

    private void UnityImplementationScrollTest() {
        EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(150));
        for (int i = 0; i < _verticalElementsCount; i++) {
            EditorGUILayout.TextField("");
        }
        EditorGUILayout.EndScrollView();
    }
    private void VerticalGroupTest() {
        if (LayoutEngine.BeginVerticalGroup()) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, "Text");
                }
            }
        }
        LayoutEngine.EndVerticalGroup();
    }
    private void VerticalGroupsPlainTest() {
        for (int i = 0; i < _verticalElementsCount; i++) {
            if (LayoutEngine.BeginVerticalGroup()) {
                var rect = LayoutEngine.RequestLayoutRect(16, 150);
                EditorGUI.TextField(rect, "");
            }
            LayoutEngine.EndVerticalGroup();
        }
    }
    private void VerticalGroupsIfCheckTest() {
        for (int i = 0; i < _verticalElementsCount; i++) {
            if (LayoutEngine.BeginVerticalGroup()) {
                var rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, "");
                }
            }
            LayoutEngine.EndVerticalGroup();
        }
    }
    private void VerticalUnityNativeTest() {
        for (int i = 0; i < _verticalElementsCount; i++) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.TextField("");
            EditorGUILayout.EndVertical();
        }
    }
    private void HorizontalGroupTest() {
        if (LayoutEngine.BeginHorizontalGroup(false)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, "");
                }
            }
        }
        LayoutEngine.EndHorizontalGroup();
    }
    private void VH_GridGroupTest() {
        if(LayoutEngine.BeginVerticalGroup())
        {
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup()) {
                    for (int j = 0; j < _horizontalElementsCount; j++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i + j, "prefix", null);
                        }
                    }
                }
                LayoutEngine.EndHorizontalGroup();
            }
        }
        LayoutEngine.EndVerticalGroup();
    }
    private void HV_GridGroupTest() {
        if(LayoutEngine.BeginHorizontalGroup()) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int j = 0; j < _verticalElementsCount; j++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i + j, "prefix", null);
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
        }
        LayoutEngine.EndHorizontalGroup();
    }
    
    private AnimBool _verticalFaded1;
    private AnimBool _verticalFaded2;
    private AnimBool _verticalFaded3;
    private AnimBool _verticalFaded4;

    private void VerticalFadeGroupTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16);
                if (rect.IsValid()) {
                    ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                }
            }
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalFadeGroupHorizontalChildGroupTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            HorizontalGroupTest();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalFadeGroupGridTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            VH_GridGroupTest();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void NestedFadeGroupsTest() {
        _verticalFaded1.target = EditorGUI.Foldout(LayoutEngine.RequestLayoutRect(16), _verticalFaded1.target, "Vertical foldout example", true);
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            HorizontalGroupTest();
            var foldoutRect1 = LayoutEngine.RequestLayoutRect(16, 150);
            if (foldoutRect1.IsValid()) {
                _verticalFaded2.target = EditorGUI.Foldout(foldoutRect1, _verticalFaded2.target, "Oh my", true);
            }
            if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                var foldoutRect2 = LayoutEngine.RequestLayoutRect(16, 150);
                if (foldoutRect2.IsValid()) {
                    _verticalFaded3.target = EditorGUI.Foldout(foldoutRect2, _verticalFaded3.target, "Vertical foldout example", true);
                }
                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded3.faded)) {
                    var foldoutRect3 = LayoutEngine.RequestLayoutRect(16, 150);
                    if (foldoutRect3.IsValid()) {
                        _verticalFaded4.target = EditorGUI.Foldout(foldoutRect3, _verticalFaded4.target, "Oh my", true);
                    }
                    if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded4.faded)) {
                        var contentRect = LayoutEngine.RequestLayoutRect(200);
                        if (contentRect.IsValid()) {
                            EditorGUI.DrawRect(contentRect, Color.magenta);
                            EditorGUI.LabelField(contentRect, contentRect.ToString());
                        }
                    }
                    LayoutEngine.EndVerticalFadeGroup();
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalFadeGroup();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }

    private void VerticalSeparatorGroupTest() {
        if (LayoutEngine.BeginVerticalSeparatorGroup()) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, "");
                }
            }
        }
        LayoutEngine.EndVerticalSeparatorGroup();
    }
    private void VerticalHierarchyGroupTest() {
        var nestedFoldoutRect = LayoutEngine.RequestLayoutRect(16);
        if (nestedFoldoutRect.IsValid()) {
            _verticalFaded2.target = EditorGUI.Foldout(nestedFoldoutRect, _verticalFaded2.target, "Nested fade group", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded, true)) {
            if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                for (int i = 0; i < _verticalElementsCount; i++) {
                    EditorGUI.TextField(LayoutEngine.RequestLayoutRect(16), "Nested label");
                }
            }
            LayoutEngine.EndVerticalHierarchyGroup();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalHierarchyWithSeparatorTest() {
        if (LayoutEngine.BeginVerticalSeparatorGroup()) {
            VerticalHierarchyGroupTest();
        }
        LayoutEngine.EndVerticalSeparatorGroup();
    }

    private void FixedHorizontalGroupTest() {
        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16);
                if (rect.IsValid()) {
                    EditorGUI.DrawRect(rect, Color.black);
                    EditorGUI.LabelField(rect, rect.width.ToString());
//                    EditorGUI.TextField(rect, "Text field");
                }
            }
        }
        LayoutEngine.EndRestrictedHorizontalGroup();
    }
    
    private void FixedHorizontalGroupVerticalChildrenTest() {
        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                if (LayoutEngine.BeginVerticalGroup(true)) {
                    var rect = LayoutEngine.RequestLayoutRect(16);
                    if (rect.IsValid()) {
                        EditorGUI.DrawRect(rect, Color.black);
                        EditorGUI.LabelField(rect, rect.width.ToString());
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
        }
        LayoutEngine.EndRestrictedHorizontalGroup();
    }

    private void FixedHorizontalGroupComplexInternalsTest() {
        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            if (LayoutEngine.BeginVerticalGroup()) {
                var headerRect = LayoutEngine.RequestLayoutRect(16);
                _verticalFaded1.target = EditorGUI.Foldout(headerRect, _verticalFaded1.target,"Vertical foldout example", true);
                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16);
                        if (rect.IsValid()) {
                            ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
                        }
                    }
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalGroup();

            // Hierarchy
            if (LayoutEngine.BeginVerticalGroup(true)) {
                var nestedFoldoutRect = LayoutEngine.RequestLayoutRect(16);
                if (nestedFoldoutRect.IsValid()) {
                    _verticalFaded2.target = EditorGUI.Foldout(nestedFoldoutRect, _verticalFaded2.target, "Nested fade group", true);
                }
                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded, true)) {
                    if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                        for (int i = 0; i < _verticalElementsCount; i++) {
                            EditorGUI.TextField(LayoutEngine.RequestLayoutRect(16), "Nested label");
                        }
                    }
                    LayoutEngine.EndVerticalHierarchyGroup();
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        LayoutEngine.EndRestrictedHorizontalGroup();
    }

    private Vector2 _hybridScrollPos;
    
    private void HybridScrollGroupTest() {
        var scrollX = EditorGUILayout.Slider(_hybridScrollPos.x, 0f, 1f);
        var scrollY = EditorGUILayout.Slider(_hybridScrollPos.y, 0f, 1f);
        int counter = 0;
        if (LayoutEngine.BeginHybridScrollGroup(400, 400, _hybridScrollPos)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup(true)) {
                    for (int j = 0; j < _horizontalElementsCount; j++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            counter++;
                            EditorGUI.TextField(rect, "Text");
                        }
                    }
                }
                LayoutEngine.EndHorizontalGroup();
            }
            
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                    for (int j = 0; j < _horizontalElementsCount; j++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 500);
                        if (rect.IsValid()) {
                            counter++;
                            EditorGUI.TextField(rect, "Text");
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
        }
        _hybridScrollPos = LayoutEngine.EndHybridScrollGroup();
        EditorGUILayout.LabelField($"Validated rects: {counter}");

        if (Event.current.type != EventType.Layout) {
            _hybridScrollPos.x = scrollX;
            _hybridScrollPos.y = scrollY;
        }
    }
}
