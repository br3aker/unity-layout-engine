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
        Repaint();
    }

    private int _validatedRects;
    
    private void OnGUI() {
        layoutDebugData = LayoutEngine.GetEngineGroupHierarchyData();
        _validatedRects = 0;
        
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
//            VerticalHierarchyGroupTreeTest();
//            VerticalHierarchyWithSeparatorTest();    // passed
//            FixedHorizontalGroupTest();    // passed
//            FixedHorizontalGroupVerticalChildrenTest();    // passed
//            FixedHorizontalGroupComplexInternalsTest();    // passed
            ScrollGroupTest();    // passed
//            RectIntersectionTest();
        }

        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"Validated rects: {_validatedRects}");
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
        var foldoutHeaderRect = LayoutEngine.RequestLayoutRect(16, 150);
        if (foldoutHeaderRect.IsValid()) {
            _verticalFaded1.target = EditorGUI.Foldout(foldoutHeaderRect,_verticalFaded1.target, "Vertical foldout example", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                var rect = LayoutEngine.RequestLayoutRect(16, 150);
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
        var foldoutRect0 = LayoutEngine.RequestLayoutRect(16, 150);
        if (foldoutRect0.IsValid()) {
            _verticalFaded1.target =
                EditorGUI.Foldout(foldoutRect0, _verticalFaded1.target, "Nested foldout example", true);
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
            HorizontalGroupTest();
            var foldoutRect1 = LayoutEngine.RequestLayoutRect(16, 150);
            if (foldoutRect1.IsValid()) {
                _verticalFaded2.target = EditorGUI.Foldout(foldoutRect1, _verticalFaded2.target, "Oh my", true);
            }
            if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded)) {
                var foldoutRect2 = LayoutEngine.RequestLayoutRect(16, 150);
                if (foldoutRect2.IsValid()) {
                    _verticalFaded3.target = EditorGUI.Foldout(foldoutRect2, _verticalFaded3.target,
                        "Vertical foldout example", true);
                }

                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded3.faded)) {
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
        if (LayoutEngine.BeginVerticalHierarchyGroup()) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                EditorGUI.TextField(LayoutEngine.RequestLayoutRect(16), "Nested label");
            }
        }
        LayoutEngine.EndVerticalHierarchyGroup();
    }

    private void _HierarchyTestHelperLevel1(AnimBool folded) {
        var headerRect = LayoutEngine.RequestLayoutRect(16, 150);
        if (headerRect.IsValid()) {
            folded.target = EditorGUI.Foldout(headerRect,folded.target, "Root", true);
            _validatedRects++;
        }
        if (LayoutEngine.BeginVerticalFadeGroup(folded.faded, true)) {
            if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                for (int i = 0; i < 3; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.LabelField(rect, "Label");
                        _validatedRects++;
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
            _validatedRects++;
        }
        if (LayoutEngine.BeginVerticalFadeGroup(folded1.faded, true)) {
            if (LayoutEngine.BeginVerticalHierarchyGroup()) {
                for (int i = 0; i < 3; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.LabelField(rect, "Label");
                        _validatedRects++;
                    }
                }
                _HierarchyTestHelperLevel1(folded2);
                var rect1 = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect1.IsValid()) {
                    EditorGUI.LabelField(rect1, "Label");
                    _validatedRects++;
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
            _validatedRects++;
        }
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded, true)) {
            if(LayoutEngine.BeginVerticalHierarchyGroup()) {
                var rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                    _validatedRects++;
                }
                rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                    _validatedRects++;
                }
                _HierarchyTestHelperLevel2(_verticalFaded3, _verticalFaded4);
                rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                    _validatedRects++;
                }
                rect = LayoutEngine.RequestLayoutRect(16, 150);
                if (rect.IsValid()) {
                    EditorGUI.LabelField(rect, "Label");
                    _validatedRects++;
                }
                _HierarchyTestHelperLevel1(_verticalFaded2);
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

//    private void FixedHorizontalGroupTest() {
//        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
//            for (int i = 0; i < _horizontalElementsCount; i++) {
//                var rect = LayoutEngine.RequestLayoutRect(16);
//                if (rect.IsValid()) {
//                    EditorGUI.DrawRect(rect, Color.black);
//                    EditorGUI.LabelField(rect, rect.width.ToString());
////                    EditorGUI.TextField(rect, "Text field");
//                }
//            }
//        }
//        LayoutEngine.EndRestrictedHorizontalGroup();
//    }
//    
//    private void FixedHorizontalGroupVerticalChildrenTest() {
//        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
//            for (int i = 0; i < _horizontalElementsCount; i++) {
//                if (LayoutEngine.BeginVerticalGroup(true)) {
//                    var rect = LayoutEngine.RequestLayoutRect(16);
//                    if (rect.IsValid()) {
//                        EditorGUI.DrawRect(rect, Color.black);
//                        EditorGUI.LabelField(rect, rect.width.ToString());
//                    }
//                }
//                LayoutEngine.EndVerticalGroup();
//            }
//        }
//        LayoutEngine.EndRestrictedHorizontalGroup();
//    }
//
//    private void FixedHorizontalGroupComplexInternalsTest() {
//        if (LayoutEngine.BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth)) {
//            if (LayoutEngine.BeginVerticalGroup()) {
//                var headerRect = LayoutEngine.RequestLayoutRect(16);
//                _verticalFaded1.target = EditorGUI.Foldout(headerRect, _verticalFaded1.target,"Vertical foldout example", true);
//                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded)) {
//                    for (int i = 0; i < _verticalElementsCount; i++) {
//                        var rect = LayoutEngine.RequestLayoutRect(16);
//                        if (rect.IsValid()) {
//                            ExtendedEditorGUI.IntPostfixInputField(rect, i, "prefix", null);
//                        }
//                    }
//                }
//                LayoutEngine.EndVerticalFadeGroup();
//            }
//            LayoutEngine.EndVerticalGroup();
//
//            // Hierarchy
//            if (LayoutEngine.BeginVerticalGroup(true)) {
//                var nestedFoldoutRect = LayoutEngine.RequestLayoutRect(16);
//                if (nestedFoldoutRect.IsValid()) {
//                    _verticalFaded2.target = EditorGUI.Foldout(nestedFoldoutRect, _verticalFaded2.target, "Nested fade group", true);
//                }
//                if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded2.faded, true)) {
//                    if (LayoutEngine.BeginVerticalHierarchyGroup()) {
//                        for (int i = 0; i < _verticalElementsCount; i++) {
//                            EditorGUI.TextField(LayoutEngine.RequestLayoutRect(16), "Nested label");
//                        }
//                    }
//                    LayoutEngine.EndVerticalHierarchyGroup();
//                }
//                LayoutEngine.EndVerticalFadeGroup();
//            }
//            LayoutEngine.EndVerticalGroup();
//        }
//        LayoutEngine.EndRestrictedHorizontalGroup();
//    }

    private Vector2 _hybridScrollPos;
    
    private void ScrollGroupTest() {
        if (LayoutEngine.BeginHybridScrollGroup(400, 640, _hybridScrollPos)) {
            for (int i = 0; i < _verticalElementsCount; i++) {
                if (LayoutEngine.BeginHorizontalGroup(true)) {
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

            if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 500);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                            _validatedRects++;
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalSeparatorGroup();
            
            if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                if (LayoutEngine.BeginVerticalGroup()) {
                    for (int i = 0; i < _verticalElementsCount; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 500);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                            _validatedRects++;
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalSeparatorGroup();


            
            if (LayoutEngine.BeginHorizontalGroup(true)) {
                for (int j = 0; j < 3; j++) {
                    if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                        for (int i = 0; i < 3; i++) {
                            var rect = LayoutEngine.RequestLayoutRect(16);
                            if (rect.IsValid()) {
                                EditorGUI.TextField(rect, "");
                            }
                        }
                    }
                    LayoutEngine.EndVerticalSeparatorGroup();
                }
            }
            LayoutEngine.EndHorizontalGroup();
            
            if (LayoutEngine.BeginVerticalGroup()) {
                NestedFadeGroupsTest();
            }
            LayoutEngine.EndVerticalGroup();

            if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                for (int i = 0; i < 16; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "Text");
                    }
                }
            }
            LayoutEngine.EndVerticalSeparatorGroup();
            
            if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                for (int i = 0; i < 10; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "");
                    }
                }
            }
            LayoutEngine.EndVerticalSeparatorGroup();
            
            VerticalHierarchyGroupTreeTest();
        }
        _hybridScrollPos = LayoutEngine.EndHybridScrollGroup();
    }

    private void RectIntersectionTest() {
        if (LayoutEngine.BeginVerticalGroup()) {
            LayoutEngine.RequestLayoutRect(600, 600);
            
            var rect1_1 = new Rect(0, 0, 275, 275);
            var rect1_2 = new Rect(225, 225, 275, 275);
            var rect1_i = Utility.RectIntersection(rect1_1, rect1_2);
            EditorGUI.DrawRect(rect1_1, Color.black);
            EditorGUI.DrawRect(rect1_2, Color.black);
            EditorGUI.DrawRect(rect1_i, Color.white);
        }
        LayoutEngine.EndVerticalGroup();
    }
}
