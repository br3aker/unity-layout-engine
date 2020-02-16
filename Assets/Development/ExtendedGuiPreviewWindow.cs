using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

using SoftKata.ExtendedEditorGUI;
using UnityEditor.Experimental.TerrainAPI;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

public class ExtendedGuiPreviewWindow : EditorWindow
{
    #region Window initialization & lifetime management
    [MenuItem("Window/Extended GUI preview")]
    static void Init() {
        GetWindow<ExtendedGuiPreviewWindow>(false, "GUI Preview").Show();
    }
    
    private void OnEnable() {
        RegisterUsage();
        
        _verticalFaded1 = new AnimBool(false);
        _verticalFaded1.valueChanged.AddListener(Repaint);
        _verticalFaded2 = new AnimBool(false);
        _verticalFaded2.valueChanged.AddListener(Repaint);
        _verticalFaded3 = new AnimBool(false);
        _verticalFaded3.valueChanged.AddListener(Repaint);
        _verticalFaded4 = new AnimBool(false);
        _verticalFaded4.valueChanged.AddListener(Repaint);

        _testIconSet = Utility.LoadAssetAtPathAndAssert<Texture>("Assets/Development/Textures/icon_set.png");

        var monitor_string = "Monitor";
        var text_string = "Text";
        var graph_string = "Graph";
        
        var monitor = new GUIContent(monitor_string, monitor_string);
        var text = new GUIContent(text_string, text_string);
        var graph = new GUIContent(graph_string, graph_string);

        _testToggleArrayContent = new ToggleArrayGUIContent(new []{monitor, text, graph}, _testIconSet);
    }
    private void OnDestroy() {
        UnregisterUsage();
    }
    private void OnLostFocus() {
        LayoutEngine.ResetEngine();
    }

    #endregion

    
    private void Update() {
        Repaint();
    }

    private void OnGUI() {
        if (Event.current.type == EventType.Used) return;
        
        var verticalCount = EditorGUILayout.IntField("Vertical count: ", _verticalElementsCount);
        var horizontalCount = EditorGUILayout.IntField("Horizontal count: ", _horizontalElementsCount);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        {
            TestingMethod();
//            PerformanceTestingScrollGroup();
//            VerticalGroupTest();    // passed
//            VerticalGroupsPlainTest();    // passed
//            VerticalGroupsIfCheckTest();    // passed
//            VerticalUnityNativeTest();    // utility
//            HorizontalGroupTest();    // passed
//            HorizontalGroupNestedVerticalGroupsTest();
//            UnityImplementationScrollTest();    // utility
//            VerticalFadeGroupTest();    // passed
//            VerticalFadeGroupHorizontalChildGroupTest();    // passed
//            NestedFadeGroupsTest();    // passed
//            VerticalSeparatorGroupTest();    // passed
//            VerticalHierarchyGroupTest();    // passed
//            VerticalHierarchyGroupTreeTest();
//            VerticalHierarchyWithSeparatorTest();    // passed
//            FixedHorizontalGroupTest();    // passed
//            FixedHorizontalGroupVerticalChildrenTest();    // passed
//            FixedHorizontalGroupComplexInternalsTest();    // passed
//            ScrollGroupTest();    // passed
//            HorizontalGroupNestedScroll();
        }

        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"Hot control id: {GUIUtility.hotControl}");
        EditorGUILayout.LabelField($"Keyboard control id: {GUIUtility.keyboardControl}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"View width: {EditorGUIUtility.currentViewWidth}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"Leaked groups count: {LayoutEngine.GroupCount()}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);

        if (Event.current.type != EventType.Layout) {
            _verticalElementsCount = verticalCount;
            _horizontalElementsCount = horizontalCount;
        }
    }

    private bool _guiDisabled = false;

    private int _testInteger = 10;
    private string _testError = "Must be > 0";

    private bool _testBool = false;
    
    private Color _testColor = Color.black;

    private string _testText1;
    private string _testText2;

    private KeyboardShortcut _testShortcut1;

    private Texture _testIconSet;
    private int _testToggleArray = 0;
    private ToggleArrayGUIContent _testToggleArrayContent;

    private void TestingMethod() {
        Rect rect;
        
        _guiDisabled = EditorGUILayout.Toggle("GUI.enabled", _guiDisabled);
        
        EditorGUI.BeginDisabledGroup(_guiDisabled);
        LayoutEngine.BeginVerticalGroup(); {
            rect = LayoutEngine.RequestLayoutRect(LabelHeight);
            _testInteger = IntDelayedField(rect, _testInteger, "postfix", _testInteger > 0 ? null : _testError);

            rect = LayoutEngine.RequestLayoutRect(LabelHeight);
            _testBool = UnderlineFoldout(rect, _testBool, "Foldout");
            
            rect = LayoutEngine.RequestLayoutRect(LabelHeight);
            _testColor = EditorGUI.ColorField(rect, _testColor);

            rect = LayoutEngine.RequestLayoutRect(LabelHeight, ShortcutRecorderWidth);
            _testShortcut1 = KeyboardShortcutField(rect, _testShortcut1);

            rect = LayoutEngine.RequestLayoutRect(ToggleArrayHeight);
            _testToggleArray = ToggleArray(rect, _testToggleArray, _testToggleArrayContent);
            
            rect = LayoutEngine.RequestLayoutRect(ToggleArrayHeight);
            _testToggleArray = ToggleArray(rect, _testToggleArray, _testToggleArrayContent.guiContent, _testToggleArrayContent.MaxTabWidth);

            rect = LayoutEngine.RequestLayoutRect(LabelHeight);
            GUI.Toolbar(rect, 0, new[] {"First", "Second"});
        }
        LayoutEngine.EndVerticalGroup();

        EditorGUI.EndDisabledGroup();

//        rect = LayoutEngine.RequestLayoutRect(ToggleArrayIconHeight);
//        for (int i = 0; i < _verticalElementsCount; i++) {
////            _testToggleArray = ToggleArraySingleLoop(rect, _testToggleArray, _testIconSet, 3);
//            _testToggleArray = ToggleArraySeparateLoops(rect, _testToggleArray, _testToggleArrayData);
//        }
    }

    private int _verticalElementsCount = 16;
    private int _horizontalElementsCount = 16;

    private void UnityImplementationScrollTest() {
        EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(640), GUILayout.Width(640));
        for (int i = 0; i < _verticalElementsCount; i++) {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < _horizontalElementsCount; j++) {
                EditorGUILayout.TextField("Text");                    
            }
            EditorGUILayout.EndHorizontal();
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
        if (LayoutEngine.BeginHorizontalGroup()) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16, -1);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, "");
                }
            }
        }
        LayoutEngine.EndHorizontalGroup();
    }

    private void HorizontalGroupNestedVerticalGroupsTest() {
        if (LayoutEngine.BeginHorizontalGroup()) {
            for (int j = 0; j < 10; j++) {
                if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                    for (int i = 0; i < 3; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
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
        var foldoutHeaderRect = LayoutEngine.RequestLayoutRect(16, 250);
        if (foldoutHeaderRect.IsValid()) {
            _verticalFaded1.target = EditorGUI.Foldout(foldoutHeaderRect,_verticalFaded1.target, foldoutHeaderRect.ToString(), true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16, 300);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, rect.ToString());
                }
            }
        }
        LayoutEngine.EndVerticalFadeGroup(); 
    }
    
    private void VerticalFadeGroupHorizontalChildGroupTest() {
        _verticalFaded1.target = EditorGUILayout.Foldout(_verticalFaded1.target, "Vertical foldout example", true);
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            if (LayoutEngine.BeginHorizontalGroup()) {
                for (int i = 0; i < _horizontalElementsCount; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, -1);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "");
                    }
                }
            }
            LayoutEngine.EndHorizontalGroup();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void NestedFadeGroupsTest() {
        var foldoutRect0 = LayoutEngine.RequestLayoutRect(16, -1);
        if (foldoutRect0.IsValid()) {
            _verticalFaded1.target =
                EditorGUI.Foldout(foldoutRect0, _verticalFaded1.target, "Nested foldout example", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            HorizontalGroupTest();
            var foldoutRect1 = LayoutEngine.RequestLayoutRect(16, -1);
            if (foldoutRect1.IsValid()) {
                _verticalFaded2.target = EditorGUI.Foldout(foldoutRect1, _verticalFaded2.target, "Oh my", true);
            }
            if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                var foldoutRect2 = LayoutEngine.RequestLayoutRect(16, -1);
                if (foldoutRect2.IsValid()) {
                    _verticalFaded3.target = EditorGUI.Foldout(foldoutRect2, _verticalFaded3.target,
                        "Vertical foldout example", true);
                }

                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded3.faded)) {
                    var contentRect = LayoutEngine.RequestLayoutRect(200, -1);
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

    private void VerticalSeparatorGroupTest() {
        if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                for (int i = 0; i < _verticalElementsCount; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "");
                    }
                }
            }
            LayoutEngine.EndVerticalGroup();
        }
        LayoutEngine.EndVerticalGroup();
    }
    private void VerticalHierarchyGroupTest() {
        if (LayoutEngine.BeginVerticalHierarchyGroup()) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                EditorGUI.TextField(LayoutEngine.RequestLayoutRect(16, -1), "Nested label");
            }
        }
        LayoutEngine.EndVerticalHierarchyGroup();
    }

    private void _HierarchyTestHelperLevel1(AnimBool folded) {
        var headerRect = LayoutEngine.RequestLayoutRect(16, 150);
        if (headerRect.IsValid()) {
            folded.target = EditorGUI.Foldout(headerRect,folded.target, "Root", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(folded.faded, GroupModifier.DiscardMargin)) {
            if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                for (int i = 0; i < 3; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.LabelField(rect, "Label");
                    }
                }
            }
            LayoutEngine.EndVerticalHierarchyGroup();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void _HierarchyTestHelperLevel2(AnimBool folded1, AnimBool folded2) {
        var headerRect = LayoutEngine.RequestLayoutRect(16, 150);
        if (headerRect.IsValid()) {
            folded1.target = EditorGUI.Foldout(headerRect,folded1.target, "Root", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(folded1.faded, GroupModifier.DiscardMargin)) {
            if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                for (int i = 0; i < 3; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.LabelField(rect, "Label");
                    }
                }
                _HierarchyTestHelperLevel1(folded2);
                var rect1 = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect1.IsValid()) {
                    EditorGUI.LabelField(rect1, "Label");
                }
            }
            LayoutEngine.EndVerticalHierarchyGroup();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalHierarchyGroupTreeTest() {
        var rootHeaderRect = LayoutEngine.RequestLayoutRect(16, 150);
        if (rootHeaderRect.IsValid()) {
            _verticalFaded1.target = EditorGUI.Foldout(rootHeaderRect,_verticalFaded1.target, "Root", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded, GroupModifier.DiscardMargin)) {
            if(LayoutEngine.BeginVerticalHierarchyGroup()) {
                var rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                _HierarchyTestHelperLevel2(_verticalFaded3, _verticalFaded4);
                rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                _HierarchyTestHelperLevel1(_verticalFaded2);
            }
            LayoutEngine.EndVerticalHierarchyGroup();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalHierarchyWithSeparatorTest() {
        if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
            VerticalHierarchyGroupTest();
        }
        LayoutEngine.EndVerticalGroup();
    }

    private void FixedHorizontalGroupTest() {
        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            var rect1 = LayoutEngine.RequestLayoutRect(16, 100);
            if (rect1.IsValid()) {
                EditorGUI.DrawRect(rect1, Color.black);
                EditorGUI.LabelField(rect1, rect1.width.ToString());
            }
            for (int i = 0; i < _horizontalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16, -1);
                if (rect.IsValid()) {
                    EditorGUI.DrawRect(rect, Color.black);
                    EditorGUI.LabelField(rect, rect.width.ToString());
                }
            }
            var rect2 = LayoutEngine.RequestLayoutRect(16, 100);
            if (rect2.IsValid()) {
                EditorGUI.DrawRect(rect2, Color.black);
                EditorGUI.LabelField(rect2, rect2.width.ToString());
            }
        }
        LayoutEngine.EndRestrictedHorizontalGroup();
    }
    private void FixedHorizontalGroupVerticalChildrenTest() {
        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                VerticalHierarchyWithSeparatorTest();
            }
        }
        LayoutEngine.EndRestrictedHorizontalGroup();
    }
    private void FixedHorizontalGroupComplexInternalsTest() {
        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            // Vertical group
            if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                for (int i = 0; i < _verticalElementsCount; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, -1);
                    if (rect.IsValid()) {
                        EditorGUI.IntField(rect, i);
                    }
                }
            }
            LayoutEngine.EndVerticalHierarchyGroup();
             
            // Hierarchy
            if (LayoutEngine.BeginVerticalGroup()) {
                var nestedFoldoutRect = LayoutEngine.RequestLayoutRect(16, -1);
                if (nestedFoldoutRect.IsValid()) {
                    _verticalFaded2.target = EditorGUI.Foldout(nestedFoldoutRect, _verticalFaded2.target, "Nested fade group", true);
                }
                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, -1);
                        if (rect.IsValid()) {
                            EditorGUI.IntField(rect, i);
                        }
                    }
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        LayoutEngine.EndRestrictedHorizontalGroup();
    }

    private Vector2 _hybridScrollPos;
    
    private void ScrollGroupTest() {
        if (LayoutEngine.BeginHybridScrollGroup(640, 640, _hybridScrollPos)) {
            HorizontalGroupNestedVerticalGroupsTest();
            
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup(GroupModifier.DiscardMargin)) {
                    if (Event.current.type == EventType.Layout) {
                        LayoutEngine.RegisterElementsArray(_horizontalElementsCount, 16, 150);
                    }
                    else {
                        for (int j = 0; j < _horizontalElementsCount; j++) {
                            var rect = LayoutEngine.RequestLayoutRect(16, 150);
                            if (rect.IsValid()) {
                                EditorGUI.TextField(rect, "Text");
                            }
                        }
                    }
                }
                LayoutEngine.EndHorizontalGroup();
            }

            if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, -1);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalGroup();

            
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 500);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalGroup();


            
            if (LayoutEngine.BeginHorizontalGroup(GroupModifier.DiscardMargin)) {
                for (int j = 0; j < 8; j++) {
                    if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                        for (int i = 0; i < 3; i++) {
                            var rect = LayoutEngine.RequestLayoutRect(16, 150);
                            if (rect.IsValid()) {
                                EditorGUI.TextField(rect, "");
                            }
                        }
                    }
                    LayoutEngine.EndVerticalGroup();
                }
            }
            LayoutEngine.EndHorizontalGroup();
            
            if (LayoutEngine.BeginVerticalGroup()) {
                NestedFadeGroupsTest();
            }
            LayoutEngine.EndVerticalGroup();

            if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                for (int i = 0; i < 16; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "Text");
                    }
                }
            }
            LayoutEngine.EndVerticalGroup();
            
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.DrawLeftSeparator)) {
                for (int i = 0; i < 10; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "");
                    }
                }
            }
            LayoutEngine.EndVerticalGroup();
            
            VerticalHierarchyGroupTreeTest();
        }
        _hybridScrollPos = LayoutEngine.EndHybridScrollGroup();
    }

    private void PerformanceTestingScrollGroup() {
        if (LayoutEngine.BeginHybridScrollGroup(640, 640, _hybridScrollPos)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup(GroupModifier.DiscardMargin)) {
                    if (Event.current.type == EventType.Layout) {
                        LayoutEngine.RegisterElementsArray(_horizontalElementsCount, 16, 150);
                    }
                    else {
                        for (int j = 0; j < _horizontalElementsCount; j++) {
                            var rect = LayoutEngine.RequestLayoutRect(16, 150);
                            if (rect.IsValid()) {
//                                EditorGUI.DelayedIntField(rect, 123);
                                IntDelayedField(rect, 123, null, null);
                            }
                        }
                    }
                }
                LayoutEngine.EndHorizontalGroup();
            }
        }
        _hybridScrollPos = LayoutEngine.EndHybridScrollGroup();
    }

    private void HorizontalGroupNestedScroll() {
//        VerticalFadeGroupTest();
        
//        if (LayoutEngine.BeginVerticalGroup()) {
//            VerticalFadeGroupTest();
//        }
//        LayoutEngine.EndVerticalGroup();

        HorizontalGroupNestedVerticalGroupsTest();

//        if (LayoutEngine.BeginRestrictedHorizontalGroup()) {
//            ScrollGroupTest();
//            ScrollGroupTest();

//            if (LayoutEngine.BeginVerticalGroup()) {
//                VerticalFadeGroupTest();
//            }
//            LayoutEngine.EndVerticalGroup();
//            
//            if (LayoutEngine.BeginVerticalGroup()) {
//                VerticalFadeGroupTest();
//            }
//            LayoutEngine.EndVerticalGroup();
//        }
//        LayoutEngine.EndHorizontalGroup();
    }
}
