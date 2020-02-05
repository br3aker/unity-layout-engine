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
//        Repaint();
    }

    private void OnGUI() {
        var verticalCount = EditorGUILayout.IntField("Vertical count: ", _verticalElementsCount);
        var horizontalCount = EditorGUILayout.IntField("Horizontal count: ", _horizontalElementsCount);
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        {
            TestingMethod();
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
        EditorGUILayout.LabelField($"Hot control id: {EditorGUIUtility.hotControl}");
        EditorGUILayout.LabelField($"Keyboard control id: {EditorGUIUtility.keyboardControl}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);
        EditorGUILayout.LabelField($"View width: {EditorGUIUtility.currentViewWidth}");
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(0f, 1f), Color.gray);

        if (Event.current.type != EventType.Layout) {
            _verticalElementsCount = verticalCount;
            _horizontalElementsCount = horizontalCount;
        }
    }

    private int _verticalElementsCount = 16;
    private int _horizontalElementsCount = 16;

    private float _verticalScrollPosition = 0f;
    private float _horizontalScrollPosition = 0f;

    private void TestingMethod() {
        
    }
    
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
                if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                    for (int i = 0; i < 3; i++) {
                        var rect = LayoutEngine.RequestLayoutRect(16, 150);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
                        }
                    }
                }
                LayoutEngine.EndVerticalSeparatorGroup();
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
        if (LayoutEngine.BeginVerticalSeparatorGroup()) {
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
        LayoutEngine.EndVerticalSeparatorGroup();
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
        if (LayoutEngine.BeginVerticalFadeGroup(folded.faded, true)) {
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
        if (LayoutEngine.BeginVerticalFadeGroup(folded1.faded, true)) {
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
        if (LayoutEngine.BeginVerticalFadeGroup(_verticalFaded1.faded, true)) {
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
        if (LayoutEngine.BeginVerticalSeparatorGroup()) {
            VerticalHierarchyGroupTest();
        }
        LayoutEngine.EndVerticalSeparatorGroup();
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
            if (LayoutEngine.BeginVerticalGroup(false)) {
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
                        var rect = LayoutEngine.RequestLayoutRect(16, -1);
                        if (rect.IsValid()) {
                            EditorGUI.TextField(rect, "");
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
                        }
                    }
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalSeparatorGroup();


            
            if (LayoutEngine.BeginHorizontalGroup(true)) {
                for (int j = 0; j < 8; j++) {
                    if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                        for (int i = 0; i < 3; i++) {
                            var rect = LayoutEngine.RequestLayoutRect(16, 150);
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
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
                    if (rect.IsValid()) {
                        EditorGUI.TextField(rect, "Text");
                    }
                }
            }
            LayoutEngine.EndVerticalSeparatorGroup();
            
            if (LayoutEngine.BeginVerticalSeparatorGroup()) {
                for (int i = 0; i < 10; i++) {
                    var rect = LayoutEngine.RequestLayoutRect(16, 150);
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
