using System;
using System.Collections.Generic;
using System.Linq;
using SoftKata.ExtendedEditorGUI;
using SoftKata.ExtendedEditorGUI.Animations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using static SoftKata.ExtendedEditorGUI.ExtendedEditorGUI;

namespace Development {
    public class ComplexElementsTestingWindow : ExtendedEditorWindow {
        [MenuItem("Window/Complex elements")]
        static void Init() {
            GetWindow<ComplexElementsTestingWindow>(false, "Complex elements");
        }

        // [NonSerialized]
        private bool _alwaysRepaint = false;

        private TabView _tabsDrawer;

        private ListView<int, StringLabelElement> _arrayDrawer;

        private ScrollViewTest _scrollViewTest;
        private ScrollViewExpander _scrollViewExpander;
        private FlexibleHorizontalGroupTest _flexibleHorizontalGroupTest;
        private TreeViewGroupTest _treeViewGroupTest;

        private AnimationValuesTest _animationValuesTest;

        protected override void Initialize() {
            if (_alwaysRepaint) {
                EditorApplication.update += Repaint;
            }
            else {
                EditorApplication.update -= Repaint;
            }

            // Scroll view for general groups testing
            _scrollViewTest = new ScrollViewTest(2500);
            _scrollViewExpander = new ScrollViewExpander();
            _flexibleHorizontalGroupTest = new FlexibleHorizontalGroupTest();
            _treeViewGroupTest = new TreeViewGroupTest();

            // List
            var listSize = 15;
            var numbersList = new List<int>(listSize);
            for(int i = 0; i < listSize; i++) {
                numbersList.Add(i);
            }

            ListView<int, StringLabelElement>.DataDrawerBinder bind = (index, data, drawable, selected) => {
                var stringLabel = drawable as StringLabelElement;
                stringLabel.Content = data.ToString();
                stringLabel.Selected = selected;
            };
            _arrayDrawer = new ListView<int, StringLabelElement>(numbersList, 350, 40, bind) {
                // Drag & drop
                ValidateDragData = () => {
                    return DragAndDropVisualMode.Link;
                },
                AddDragDataToArray = (list) => {
                    var references = DragAndDrop.objectReferences;
                    for (int i = 0; i < references.Length; i++) {
                        list.Add(references[i].GetInstanceID());
                    }
                },

                // etc
                ReorderingTintAlpha = 0.65f
            };
            _arrayDrawer.OnElementSelected += (index, data, drawer) => {
                    if(drawer != null) {
                        (drawer as StringLabelElement).Selected = true;
                        // Debug.Log($"Selected data index {index}");
                    }
                };
            _arrayDrawer.OnElementDeselected += (index, data, drawer) => {
                    if(drawer != null) {
                        (drawer as StringLabelElement).Selected = false;
                        Debug.Log($"Deselected data index {index}");
                    }
                };
            _arrayDrawer.OnElementDoubleClick += (index, data, drawer) => {
                    Debug.Log($"Double click on {index} at {data}");
                };
            _arrayDrawer.OnElementsReorder += (oldIndex, newIndex) => {
                    Debug.Log($"Swapped {oldIndex} index with {newIndex} index");
                };


            // Tabs
            var tabHeaders = new[] {
                new GUIContent("Label"),
                new GUIContent("List"),
                new GUIContent("Scroll")
            };
            var tabsContents = new IDrawableElement[] {
                new StringLabelElement("Tab content #1"),
                _arrayDrawer,
                _scrollViewTest
            };
            _tabsDrawer = new TabView(0, tabHeaders, tabsContents, new Color(0.06f, 0.51f, 0.75f));

            _animationValuesTest = new AnimationValuesTest();
        }

        protected override void IMGUI() {
            DrawServiceInfo();

            // for(int i = 0; i < 20; i++) {
            //     var rect = Layout.GetRect(16);
            //     EditorGUI.DrawRect(rect, Color.black);
            //     EditorGUI.LabelField(rect, rect.width.ToString());
            // }

            // EditorGUI.DrawRect(Layout.GetRect(2000, 16), Color.red);

            // _tabsDrawer.OnGUI();

            _animationValuesTest.OnGUI();

            // Profiler.BeginSample("ListView test");
            // _arrayDrawer.OnGUI();
            // Profiler.EndSample();

            
            // Profiler.BeginSample("Scroll group");
            // _scrollViewTest.OnGUI();
            // Profiler.EndSample();

            // _scrollViewExpander.OnGUI();

            // _flexibleHorizontalGroupTest.OnGUI();
            
            // _treeViewGroupTest.OnGUI();
        }

        protected override WindowHeaderBar CreateHeader() {
            return new WindowHeaderBar(
                null,
                new IDrawableElement[] {
                    new WindowHeaderSearchBar(),
                    new Button(EditorGUIUtility.IconContent("d__Help"), ExtendedEditorGUI.Resources.WindowHeader.ButtonStyle, () => Debug.Log("Button #1 pressed")),
                    new Button(EditorGUIUtility.IconContent("d_Preset.Context"), ExtendedEditorGUI.Resources.WindowHeader.ButtonStyle, () => Debug.Log("Button #2 pressed")),
                    new Button(EditorGUIUtility.IconContent("d__Menu"), ExtendedEditorGUI.Resources.WindowHeader.ButtonStyle, () => Debug.Log("Overflow menu pressed"))
                }
            );
        }

        private void DrawServiceInfo() {
            if (Layout.GetRect(ExtendedEditorGUI.LabelHeight, out var labelRect)) {
                EditorGUI.LabelField(labelRect,
                    $"Always repaint: [{_alwaysRepaint}] | [{Mathf.Sin((float) EditorApplication.timeSinceStartup)}]");
            }
            if (Layout.GetRect(ExtendedEditorGUI.LabelHeight, out var updateButtonRect)) {
                if (GUI.Button(updateButtonRect, _alwaysRepaint ? "Always update" : "Update on action")) {
                    _alwaysRepaint = !_alwaysRepaint;
                    if (_alwaysRepaint) {
                        EditorApplication.update += Repaint;
                    }
                    else {
                        EditorApplication.update -= Repaint;
                    }
                }
            }
        }

        public class StringLabelElement : IDrawableElement, IAbsoluteDrawableElement {
            public string Content { get; set; }

            public bool Selected {get; set;}

            public StringLabelElement() : this("") { }
            public StringLabelElement(string content) {
                Content = content;
            }

            public void OnGUI() {
                if(Layout.GetRect(40, out var rect)) {
                    if(Selected) {
                        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));
                    }
                    else {
                        EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
                    }
                    
                    EditorGUI.LabelField(rect, $"{Content} | {rect.width}");
                }
            }
            public void OnGUI(Rect rect) {                   
                if(Selected) {
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));
                }
                else {
                    EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
                }
                
                EditorGUI.LabelField(rect, Content);
            }
        }

        public class TypeStringLabelElement : IAbsoluteDrawableElement {
            public string Type {get; set;}
            public string Value {get; set;}

            public bool Selected {get; set;}

            public void OnGUI(Rect rect) {
                if(Selected) {
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));
                }
                else {
                    EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
                }

                var typeRect = new Rect(rect.position, new Vector2(rect.width, 18));
                var valueRect = new Rect(new Vector2(typeRect.x, typeRect.y + 22), typeRect.size);
                EditorGUI.LabelField(typeRect, $"Current: {rect.width} | Visible {EditorGUIUtility.currentViewWidth}");
                EditorGUI.LabelField(valueRect, Value);
            }
        }

        public class ScrollViewTest : IDrawableElement {
            private VerticalFadeGroup _fadeGroup;
            private ScrollGroup _scrollGroup;
            private LayoutGroup[] _nestedHorizontalGroups;

            private int _horizontalEntriesCount = 50;

            public ScrollViewTest(int nestedGroupCount) {
                _fadeGroup = new VerticalFadeGroup(true);
                _scrollGroup = new ScrollGroup(new Vector2(-1, 640), false);

                _nestedHorizontalGroups = new LayoutGroup[nestedGroupCount];
                for(int i = 0; i < nestedGroupCount; i++) {
                    _nestedHorizontalGroups[i] = new HorizontalGroup();
                }
            }

            public void OnGUI() {
                // _fadeGroup.Expanded = EditorGUI.Foldout(Layout.GetRect(16), _fadeGroup.Expanded, "Fade group");
                if(_fadeGroup.Visible && Layout.BeginLayoutGroup(_fadeGroup)) {
                    if(Layout.BeginLayoutGroup(_scrollGroup)) {
                        for(int i = 0; i < _nestedHorizontalGroups.Length; i++) {
                            var group = _nestedHorizontalGroups[i];
                            if(Layout.BeginLayoutGroup(group)) {
                                var width = group.AutomaticWidth;
                                for(int j = 0; j < _horizontalEntriesCount; j++) {
                                    if(group.GetRect(width, 30, out var rect)) {
                                        EditorGUI.DrawRect(rect, Color.black);
                                    }
                                }
                                Layout.EndLayoutGroup();
                            }
                        }
                        Layout.EndLayoutGroup();
                    }

                    Layout.EndLayoutGroup();
                }
            }
        }

        public class ScrollViewExpander : IDrawableElement {
            private ScrollGroup _scrollGroup;
            private Vector2Int _contentSize = new Vector2Int(200, 400);

            public ScrollViewExpander() {
                _scrollGroup = new ScrollGroup(new Vector2(-1, 400));
            }

            public void OnGUI() {
                _contentSize.x = EditorGUI.IntField(Layout.GetRect(16), "X size", _contentSize.x);
                _contentSize.y = EditorGUI.IntField(Layout.GetRect(16), "Y size", _contentSize.y);

                if(Layout.BeginLayoutGroup(_scrollGroup)) {
                    if(Layout.GetRect(_contentSize.x, _contentSize.y, out var rect)) {
                        EditorGUI.DrawRect(rect, Color.red);
                        EditorGUI.LabelField(rect, $"{rect.width} x {rect.height}");
                    }
                }
                Layout.EndLayoutGroup();
            }
        }

        public class FlexibleHorizontalGroupTest : IDrawableElement {
            private LayoutGroup _flexibleHorizontalGroup;

            public FlexibleHorizontalGroupTest() {
                _flexibleHorizontalGroup = new FlexibleHorizontalGroup();
            }
            
            public void OnGUI() {
                if(Layout.BeginLayoutGroup(_flexibleHorizontalGroup)) {
                    Layout.GetRect(40, 55, out var fixedRect);
                    EditorGUI.DrawRect(fixedRect, Color.black);
                    EditorGUI.LabelField(fixedRect, fixedRect.width.ToString());

                    for(int i = 0; i < 3; i++) {
                        var rect = Layout.GetRect(40);
                        EditorGUI.DrawRect(rect, Color.black);
                        EditorGUI.LabelField(rect, rect.width.ToString());
                    }
                }
                Layout.EndLayoutGroup();
            }
        }
    
        public class TreeViewGroupTest : IDrawableElement {
            private TreeViewGroup _treeViewGroup;
            private TreeViewGroup _treeViewChildGroup;

            public TreeViewGroupTest() {
                _treeViewGroup = new TreeViewGroup();
                _treeViewChildGroup = new TreeViewGroup();
            }
            
            public void OnGUI() {
                if(Layout.BeginLayoutGroup(_treeViewGroup)) {
                    for(int i = 0; i < 3; i++) {
                        var rect = _treeViewGroup.GetLeafRect(40);
                        EditorGUI.DrawRect(rect, Color.black);
                        EditorGUI.LabelField(rect, rect.width.ToString());
                    }

                    if(Layout.BeginLayoutGroup(_treeViewChildGroup)) {
                        for(int i = 0; i < 3; i++) {
                            var rect = _treeViewChildGroup.GetLeafRect(40);
                            EditorGUI.DrawRect(rect, Color.black);
                            EditorGUI.LabelField(rect, rect.width.ToString());
                        }
                        Layout.EndLayoutGroup();
                    }

                    for(int i = 0; i < 3; i++) {
                        var rect =  _treeViewGroup.GetLeafRect(40);
                        EditorGUI.DrawRect(rect, Color.black);
                        EditorGUI.LabelField(rect, rect.width.ToString());
                    }
                    Layout.EndLayoutGroup();
                }
            }
        }

        public class AnimationValuesTest : IDrawableElement {
            private ScrollGroup updateSubsContainer;
            private ScrollGroup tweensContainer;

            private float _animTarget;

            private FloatTween _floatTween;

            public AnimationValuesTest() {
                updateSubsContainer = new ScrollGroup(new Vector2(-1, 400), false);
                tweensContainer = new ScrollGroup(new Vector2(-1, 400), false);

                _floatTween = new FloatTween(0);
                _floatTween.OnBegin += () => Debug.Log("Tween start");
                _floatTween.OnFinish += () => Debug.Log("Tween end");
                _floatTween.OnUpdate += CurrentViewRepaint;
            }

            public void OnGUI() {
                if(Layout.BeginLayoutGroup(updateSubsContainer)) {
                    var subs = EditorApplication.update.GetInvocationList();
                    EditorGUI.LabelField(Layout.GetRect(16), $"Sub count: {subs.Length}");
                    for(int i = 0; i < subs.Length; i++) {
                        var subMethod = subs[i].Method;
                        EditorGUI.LabelField(Layout.GetRect(16), $"{subMethod.DeclaringType}::{subMethod.Name}( )");
                    }

                    updateSubsContainer.MarkLayoutDirty();
                    Layout.EndLayoutGroup();
                }

                _animTarget = EditorGUI.FloatField(Layout.GetRect(16), "Target", _animTarget);

                if(GUI.Button(Layout.GetRect(16), "Launch")) {
                    _floatTween.Target = _animTarget;
                }
                
                if(Layout.BeginLayoutGroup(tweensContainer)) {
                    _floatTween.DrawDebugInfo();

                    Layout.EndLayoutGroup();
                }
            }
        }
    }
}