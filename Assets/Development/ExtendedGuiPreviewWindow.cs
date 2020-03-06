using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

using SoftKata.ExtendedEditorGUI;
using UnityEngine.Profiling;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

public class ExtendedGuiPreviewWindow : ExtendedEditorWindow
{
    #region Window initialization & lifetime management
    [MenuItem("Window/Extended GUI preview")]
    static void Init() {
        GetWindow<ExtendedGuiPreviewWindow>(false, "GUI Preview").Show();
    }
    
    protected override void Initialize() {
        _verticalFaded1 = new AnimBool(false);
        _verticalFaded1.valueChanged.AddListener(Repaint);
        _verticalFaded2 = new AnimBool(false);
        _verticalFaded2.valueChanged.AddListener(Repaint);
        _verticalFaded3 = new AnimBool(false);
        _verticalFaded3.valueChanged.AddListener(Repaint);
        _verticalFaded4 = new AnimBool(false);
        _verticalFaded4.valueChanged.AddListener(Repaint);

        var icon_off = Utility.LoadAssetAtPathAndAssert<Texture>("Assets/Development/Textures/monitor_off.png");
        var icon_on = Utility.LoadAssetAtPathAndAssert<Texture>("Assets/Development/Textures/monitor_on.png");

        var monitor_string = "Monitor";
        var text_string = "Text";
        var graph_string = "Graph";
        
        var monitor = new GUIContent(monitor_string, icon_off, monitor_string);
        var text = new GUIContent(text_string, icon_off, text_string);
        var graph = new GUIContent(graph_string, icon_off, graph_string);
        var monitor_off = new GUIContent(monitor_string, icon_on, monitor_string);
        var text_off = new GUIContent(text_string, icon_on, text_string);
        var graph_off = new GUIContent(graph_string, icon_on, graph_string);
        _testToggleArrayGUIContent = new[] {monitor_off, text_off, graph_off, monitor, text, graph};
    }
    #endregion

    private bool _alwaysRepaint;

    protected override void IMGUI() {
        if (LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var labelRect)) {
            EditorGUI.LabelField(labelRect,$"Always repaint: [{_alwaysRepaint}] | [{Mathf.Sin((float)EditorApplication.timeSinceStartup)}]");
        }
        if (LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var buttonRect)) {
            if (GUI.Button(buttonRect, _alwaysRepaint ? "Always update" : "Update on action")) {
                _alwaysRepaint = !_alwaysRepaint;
                if (_alwaysRepaint) {
                    EditorApplication.update += Repaint;
                }
                else {
                    EditorApplication.update -= Repaint;
                }
            }
        }

        var verticalCount = EditorGUI.IntField(LayoutEngine.GetRect(18), "Vertical count: ", _verticalElementsCount);
        var horizontalCount = EditorGUI.IntField(LayoutEngine.GetRect(18), "Horizontal count: ", _horizontalElementsCount);
        
        EditorGUI.DrawRect(LayoutEngine.GetRect(1), Color.gray);
        EditorGUI.LabelField(LayoutEngine.GetRect(18), $"Hot control id: {GUIUtility.hotControl}");
        EditorGUI.LabelField(LayoutEngine.GetRect(18), $"Keyboard control id: {GUIUtility.keyboardControl}");
        EditorGUI.DrawRect(LayoutEngine.GetRect(1), Color.gray);
        EditorGUI.LabelField(LayoutEngine.GetRect(18), $"View width: {EditorGUIUtility.currentViewWidth}");
        EditorGUI.DrawRect(LayoutEngine.GetRect(1), Color.gray);
        EditorGUI.LabelField(LayoutEngine.GetRect(18), $"Scroll position: ({_hybridScrollPos.x}, {_hybridScrollPos.y})");
        EditorGUI.DrawRect(LayoutEngine.GetRect(1), Color.gray);
        EditorGUI.LabelField(LayoutEngine.GetRect(18), $"Group count: {LayoutEngine.GetGroupQueueSize()}");
        EditorGUI.DrawRect(LayoutEngine.GetRect(1), Color.gray);
        
        {
            // TestingMethod();

            // Profiler.BeginSample("Performance testing group");
            // PerformanceTestingScrollGroup();
            // Profiler.EndSample();

            // VerticalGroupTest();    // passed
            // VerticalGroupsPlainTest();    // passed
            // VerticalGroupsIfCheckTest();    // passed
            // VerticalUnityNativeTest();    // utility
            // HorizontalGroupTest();    // passed
            // HorizontalGroupNestedVerticalGroupsTest();
            // UnityImplementationScrollTest();    // utility
            // VerticalFadeGroupTest();    // passed
            // VerticalFadeGroupHorizontalChildGroupTest();    // passed
            // NestedFadeGroupsTest();    // passed
            // VerticalSeparatorGroupTest();    // passed
            // VerticalHierarchyGroupTest();    // passed
            // VerticalHierarchyGroupTreeTest();
            // VerticalHierarchyWithSeparatorTest();    // passed
            FixedHorizontalGroupTest();    // passed
            // FixedHorizontalGroupVerticalChildrenTest();    // passed
            // FixedHorizontalGroupComplexInternalsTest();    // passed
            // ScrollGroupTest();    // passed
            // HorizontalGroupNestedScroll();
        }

        if (Event.current.type != EventType.Layout) {
            _verticalElementsCount = verticalCount;
            _horizontalElementsCount = horizontalCount;
        }
    }

    private bool _guiDisabled = false;

    private int _testInteger = 10;
    
    private Color _testColor = Color.black;

    private string _testText1;
    private string _testText2;

    private KeyboardShortcut _testShortcut1;

    private Texture _testIconSet;
    private int _testToggleArray = 0;

    private GUIContent[] _testToggleArrayGUIContent;

    private void TestingMethod() {
        _guiDisabled = EditorGUI.Toggle(LayoutEngine.GetRect(18), "GUI.enabled", _guiDisabled);
        
        EditorGUI.BeginDisabledGroup(_guiDisabled);

        if (LayoutEngine.BeginVerticalGroup()) {
            _testInteger = EditorGUI.IntField(LayoutEngine.GetRect(LabelHeight), _testInteger);

            _testInteger = IntDelayedField(LayoutEngine.GetRect(LabelHeight), _testInteger, "postfix");

            _testColor = EditorGUI.ColorField(LayoutEngine.GetRect(LabelHeight), _testColor);

            _testShortcut1 = KeyboardShortcutField(LayoutEngine.GetRect(LabelHeight), _testShortcut1);

            _testToggleArray = ToggleArray(LayoutEngine.GetRect(LabelHeight), _testToggleArray, _testToggleArrayGUIContent);

            GUI.Toolbar(LayoutEngine.GetRect(LabelHeight), 0, new[] {"First", "Second"});
        }
        LayoutEngine.EndVerticalGroup();

        EditorGUI.EndDisabledGroup();
    }

    private int _verticalElementsCount = 14;
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
        using (var scope = new LayoutEngine.VerticalScope()) {
            if (!scope.Valid) return;
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.GetRect(16, LayoutEngine.AutoWidth, out Rect rect)) {
                    EditorGUI.TextField(rect, "Text");
                }
            }
        }
    }
    private void VerticalGroupsPlainTest() {
        for (int i = 0; i < _verticalElementsCount; i++) {
            if (LayoutEngine.BeginVerticalGroup()) {
                var rect = LayoutEngine.GetRect(16, 150);
                EditorGUI.TextField(rect, "");
            }
            LayoutEngine.EndVerticalGroup();
        }
    }
    private void VerticalGroupsIfCheckTest() {
        for (int i = 0; i < _verticalElementsCount; i++) {
            if (LayoutEngine.BeginVerticalGroup()) {
                var rect = LayoutEngine.GetRect(16, 150);
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
                var rect = LayoutEngine.GetRect(16, -1);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, "");
                }
            }
        }
        LayoutEngine.EndHorizontalGroup();
    }

    private void HorizontalGroupNestedVerticalGroupsTest() {
        if (LayoutEngine.BeginHorizontalGroup()) {
            for (int j = 0; j < _horizontalElementsCount; j++) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.GetRect(16, 150);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "Horizontal/Vertical");
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
        var foldoutHeaderRect = LayoutEngine.GetRect(16, 250);
        if (foldoutHeaderRect.IsValid()) {
            _verticalFaded1.target = EditorGUI.Foldout(foldoutHeaderRect,_verticalFaded1.target, foldoutHeaderRect.ToString(), true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = LayoutEngine.GetRect(16, 300);
                if (rect.IsValid()) {
                    EditorGUI.TextField(rect, rect.ToString());
                }
            }
        }
        LayoutEngine.EndVerticalFadeGroup(); 
    }
    
    private void VerticalFadeGroupHorizontalChildGroupTest() {
        _verticalFaded1.target = EditorGUI.Foldout(LayoutEngine.GetRect(18), _verticalFaded1.target, "Vertical foldout example", true);
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            if (LayoutEngine.BeginHorizontalGroup()) {
                for (int i = 0; i < _horizontalElementsCount; i++) {
                    var rect = LayoutEngine.GetRect(16, -1);
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
        var foldoutRect0 = LayoutEngine.GetRect(16, -1);
        if (foldoutRect0.IsValid()) {
            _verticalFaded1.target =
                EditorGUI.Foldout(foldoutRect0, _verticalFaded1.target, "Nested foldout example", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            HorizontalGroupTest();
            var foldoutRect1 = LayoutEngine.GetRect(16, -1);
            if (foldoutRect1.IsValid()) {
                _verticalFaded2.target = EditorGUI.Foldout(foldoutRect1, _verticalFaded2.target, "Oh my", true);
            }
            if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                var foldoutRect2 = LayoutEngine.GetRect(16, -1);
                if (foldoutRect2.IsValid()) {
                    _verticalFaded3.target = EditorGUI.Foldout(foldoutRect2, _verticalFaded3.target,
                        "Vertical foldout example", true);
                }

                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded3.faded)) {
                    var contentRect = LayoutEngine.GetRect(200, -1);
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
        if (LayoutEngine.BeginVerticalGroup()) {
            if (LayoutEngine.BeginVerticalGroup()) {
                for (int i = 0; i < _verticalElementsCount; i++) {
                    var rect = LayoutEngine.GetRect(16);
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
        if (LayoutEngine.BeginTreeViewGroup()) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                EditorGUI.TextField(LayoutEngine.GetRect(16, -1), "Nested label");
            }
        }
        LayoutEngine.EndTreeView();
    }

    private void _HierarchyTestHelperLevel1(AnimBool folded) {
        if (LayoutEngine.GetRect(16, 150, out var headerRect)) {
            folded.target = EditorGUI.Foldout(headerRect,folded.target, "Root", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(folded.faded, Constraints.DiscardMargin)) {
            if (LayoutEngine.BeginTreeViewGroup()) {
                for (int i = 0; i < 3; i++) {
                    if (LayoutEngine.GetRect(16, 150, out var rect)) {
                        EditorGUI.LabelField(rect, "Label");
                    }
                }
            }
            LayoutEngine.EndTreeView();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void _HierarchyTestHelperLevel2(AnimBool folded1, AnimBool folded2) {
        var headerRect = LayoutEngine.GetRect(16, 150);
        if (headerRect.IsValid()) {
            folded1.target = EditorGUI.Foldout(headerRect,folded1.target, "Root", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(folded1.faded, Constraints.DiscardMargin)) {
            if (LayoutEngine.BeginTreeViewGroup()) {
                for (int i = 0; i < 3; i++) {
                    var rect = LayoutEngine.GetRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.LabelField(rect, "Label");
                    }
                }
                _HierarchyTestHelperLevel1(folded2);
                var rect1 = LayoutEngine.GetRect(16, 150);
                if (rect1.IsValid()) {
                    EditorGUI.LabelField(rect1, "Label");
                }
            }
            LayoutEngine.EndTreeView();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalHierarchyGroupTreeTest() {
        var rootHeaderRect = LayoutEngine.GetRect(16, 150);
        if (rootHeaderRect.IsValid()) {
            _verticalFaded1.target = EditorGUI.Foldout(rootHeaderRect,_verticalFaded1.target, "Root", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded, Constraints.DiscardMargin)) {
            if(LayoutEngine.BeginTreeViewGroup()) {
                var rect = LayoutEngine.GetRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                rect = LayoutEngine.GetRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                _HierarchyTestHelperLevel2(_verticalFaded3, _verticalFaded4);
                rect = LayoutEngine.GetRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                rect = LayoutEngine.GetRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                }
                _HierarchyTestHelperLevel1(_verticalFaded2);
            }
            LayoutEngine.EndTreeView();
        }
        LayoutEngine.EndVerticalFadeGroup();
    }
    private void VerticalHierarchyWithSeparatorTest() {
        if (LayoutEngine.BeginVerticalGroup()) {
            VerticalHierarchyGroupTest();
        }
        LayoutEngine.EndVerticalGroup();
    }

    private void FixedHorizontalGroupTest() {
        if (LayoutEngine.BeginFlexibleHorizontalGroup(LayoutEngine.AutoWidth)) {
            if (LayoutEngine.GetRect(16, 100, out var fixedRect)) {
                EditorGUI.DrawRect(fixedRect, Color.black);
                EditorGUI.LabelField(fixedRect, fixedRect.width.ToString());
            }

            if (LayoutEngine.GetRect(32, LayoutEngine.AutoWidth, out var flexibleRect)) {
                EditorGUI.DrawRect(flexibleRect, Color.black);
                EditorGUI.LabelField(flexibleRect, flexibleRect.width.ToString());
            }

            if (LayoutEngine.GetRect(32, LayoutEngine.AutoWidth, out var fractionRect)) {
                EditorGUI.DrawRect(fractionRect, Color.black);
                EditorGUI.LabelField(fractionRect, fractionRect.width.ToString());
            }
        }
        LayoutEngine.EndFlexibleHorizontalGroup();
    }
    private void FixedHorizontalGroupVerticalChildrenTest() {
        if (LayoutEngine.BeginFlexibleHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            for (int i = 0; i < _horizontalElementsCount; i++) {
                VerticalHierarchyWithSeparatorTest();
            }
        }
        LayoutEngine.EndFlexibleHorizontalGroup();
    }
    private void FixedHorizontalGroupComplexInternalsTest() {
        if (LayoutEngine.BeginFlexibleHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
            // Vertical group
            if (LayoutEngine.BeginTreeViewGroup()) {
                for (int i = 0; i < _verticalElementsCount; i++) {
                    var rect = LayoutEngine.GetRect(16, -1);
                    if (rect.IsValid()) {
                        EditorGUI.IntField(rect, i);
                    }
                }
            }
            LayoutEngine.EndTreeView();
             
            // Hierarchy
            if (LayoutEngine.BeginVerticalGroup()) {
                var nestedFoldoutRect = LayoutEngine.GetRect(16, -1);
                if (nestedFoldoutRect.IsValid()) {
                    _verticalFaded2.target = EditorGUI.Foldout(nestedFoldoutRect, _verticalFaded2.target, "Nested fade group", true);
                }
                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.GetRect(16, -1);
                        if (rect.IsValid()) {
                            EditorGUI.IntField(rect, i);
                        }
                    }
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        LayoutEngine.EndFlexibleHorizontalGroup();
    }

    private Vector2 _hybridScrollPos;
    
    private void ScrollGroupTest() {
        if (LayoutEngine.BeginScrollGroup(new Vector2(-1, 640), _hybridScrollPos)) {
            HorizontalGroupNestedVerticalGroupsTest();
            
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup(Constraints.DiscardMargin)) {
                    if (Event.current.type == EventType.Layout) {
                        LayoutEngine.RegisterArray(_horizontalElementsCount, 16, 150);
                    }
                    else {
                        for (int j = 0; j < _horizontalElementsCount; j++) {
                            var rect = LayoutEngine.GetRect(16, 150);
                            if (rect.IsValid()) {
                                EditorGUI.TextField(rect, "Text");
                            }
                        }
                    }
                }
                LayoutEngine.EndHorizontalGroup();
            }

            if (LayoutEngine.BeginVerticalGroup()) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.GetRect(16, -1);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalGroup();

            if (LayoutEngine.BeginVerticalGroup()) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.GetRect(16, 500);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalGroup();

            if (LayoutEngine.BeginHorizontalGroup(Constraints.DiscardMargin)) {
                for (int j = 0; j < 8; j++) {
                    if (LayoutEngine.BeginVerticalGroup()) {
                        for (int i = 0; i < 3; i++) {
                            var rect = LayoutEngine.GetRect(16, 150);
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

            if (LayoutEngine.BeginVerticalGroup()) {
                for (int i = 0; i < 16; i++) {
                    var rect = LayoutEngine.GetRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "Text");
                    }
                }
            }
            LayoutEngine.EndVerticalGroup();
            
            if (LayoutEngine.BeginVerticalGroup()) {
                for (int i = 0; i < 10; i++) {
                    var rect = LayoutEngine.GetRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "");
                    }
                }
            }
            LayoutEngine.EndVerticalGroup();
            
            VerticalHierarchyGroupTreeTest();
        }
        _hybridScrollPos = LayoutEngine.EndScrollGroup();
    }

    private void PerformanceTestingScrollGroup() {
        if (LayoutEngine.BeginScrollGroup(new Vector2(-1, 640), _hybridScrollPos)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup(Constraints.DiscardMargin)) {
                    if (Event.current.type == EventType.Layout) {
                        LayoutEngine.RegisterArray(_horizontalElementsCount, 16, 150);
                    }
                    else {
                        for (int j = 0; j < _horizontalElementsCount; j++) {
                            if (LayoutEngine.GetRect(16, 150, out Rect rect)) {
                                IntDelayedField(rect, 123, "postfix");
                            }
                        }
                    }
                }
                LayoutEngine.EndHorizontalGroup();
            }
        }
        _hybridScrollPos = LayoutEngine.EndScrollGroup();
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
