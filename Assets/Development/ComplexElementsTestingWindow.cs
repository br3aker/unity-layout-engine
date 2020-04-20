using System;
using System.Collections.Generic;
using System.Linq;
using SoftKata.ExtendedEditorGUI;
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

        private bool _alwaysRepaint;

        private Tabs _tabsDrawer;

        private ListView<int, TypeStringLabelElement> _arrayDrawer;

        private ScrollViewTest _scrollViewTest;
        private ScrollViewExpander _scrollViewExpander;
        private FlexibleHorizontalGroupTest _flexibleHorizontalGroupTest;
        private TreeViewGroupTest _treeViewGroupTest;

        protected override void Initialize() {
            if (_alwaysRepaint) {
                EditorApplication.update += Repaint;
            }
            else {
                EditorApplication.update -= Repaint;
            }

            // List
            var listSize = 16;
            var numbersList = new List<int>(listSize);
            for(int i = 0; i < listSize; i++) {
                numbersList.Add(i);
            }

            Action<int, IAbsoluteDrawableElement, bool> bind = (data, drawable, selected) => {
                var stringLabel = drawable as TypeStringLabelElement;
                stringLabel.Type = data.GetType().Name;
                stringLabel.Value = data.ToString();
                stringLabel.Selected = selected;
            };
            _arrayDrawer = new ListView<int, TypeStringLabelElement>(numbersList, 350, 40, bind) {
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

                // Selection
                OnElementSelected = (index, drawer) => {
                    if(drawer != null) {
                        (drawer as TypeStringLabelElement).Selected = true;
                    }
                },
                OnElementDeselected = (index, drawer) => {
                    if(drawer != null) {
                        (drawer as TypeStringLabelElement).Selected = false;
                    }
                },
                OnElementDoubleClick = (index, value) => {
                    Debug.Log($"Double click on {index} at {value}");
                },

                // Reordering
                ReorderableElementAlpha = 0.65f,
                OnElementsReorder = (oldIndex, newIndex) => {
                    Debug.Log($"Swapped {oldIndex} index with {newIndex} index");
                }
            };


            // Tabs
            var tabHeaders = new[] {
                new GUIContent("Tab 1"),
                new GUIContent("Tab 2"),
                new GUIContent("Tab 3")
            };
            var tabsContents = new IDrawableElement[] {
                new StringLabelElement("Tab content #1"),
                new StringLabelElement("Tab content #2"),
                _arrayDrawer
            };
            _tabsDrawer = new Tabs(0, tabHeaders, tabsContents, new Color(0.06f, 0.51f, 0.75f));


            // Scroll view for general groups testing
            _scrollViewTest = new ScrollViewTest(2500);
            _scrollViewExpander = new ScrollViewExpander();
            _flexibleHorizontalGroupTest = new FlexibleHorizontalGroupTest();
            _treeViewGroupTest = new TreeViewGroupTest();
        }

        protected override void IMGUI() {
            DrawServiceInfo();

            // _tabsDrawer.OnGUI();

            Profiler.BeginSample("ListView test");
            _arrayDrawer.OnGUI();
            Profiler.EndSample();

            
            // Profiler.BeginSample("Scroll group");
            // _scrollViewTest.OnGUI();
            // Profiler.EndSample();

            // _scrollViewExpander.OnGUI();

            // _flexibleHorizontalGroupTest.OnGUI();
            
            // _treeViewGroupTest.OnGUI();
        }

        private void DrawServiceInfo() {
            if (Layout.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var labelRect)) {
                EditorGUI.LabelField(labelRect,
                    $"Always repaint: [{_alwaysRepaint}] | [{Mathf.Sin((float) EditorApplication.timeSinceStartup)}]");
            }
            if (Layout.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var updateButtonRect)) {
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
                if(Layout.GetRect(40, -1, out var rect)) {
                    if(Selected) {
                        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));
                    }
                    else {
                        EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
                    }
                    
                    EditorGUI.LabelField(rect, Content);
                }
            }
            public void OnGUI(Rect rect) {
                EditorGUI.DrawRect(rect, Color.grey);
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
                _scrollGroup = new ScrollGroup(new Vector2(-1, 640), Vector2.zero, false);

                _nestedHorizontalGroups = new LayoutGroup[nestedGroupCount];
                for(int i = 0; i < nestedGroupCount; i++) {
                    _nestedHorizontalGroups[i] = new HorizontalGroup();
                }
            }

            public void OnGUI() {
                _fadeGroup.Expanded = EditorGUI.Foldout(Layout.GetRect(16), _fadeGroup.Expanded, "Fade group");
                if(_fadeGroup.Visible && Layout.BeginLayoutGroup(_fadeGroup)) {
                    if(Layout.BeginLayoutGroup(_scrollGroup)) {
                        for(int i = 0; i < _nestedHorizontalGroups.Length; i++) {
                            if(Layout.BeginLayoutGroup(_nestedHorizontalGroups[i])) {
                                for(int j = 0; j < _horizontalEntriesCount; j++) {
                                    if(Layout.GetRect(30f, -1, out var rect)) {
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
                _scrollGroup = new ScrollGroup(new Vector2(-1, 400), Vector2.zero);
            }

            public void OnGUI() {
                _contentSize.x = EditorGUI.IntField(Layout.GetRect(16), "X size", _contentSize.x);
                _contentSize.y = EditorGUI.IntField(Layout.GetRect(16), "Y size", _contentSize.y);

                if(Layout.BeginLayoutGroup(_scrollGroup)) {
                    if(Layout.GetRect(_contentSize.y, _contentSize.x, out var rect)) {
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
                _flexibleHorizontalGroup = new FlexibleHorizontalGroup(Layout.AutoWidth);
            }
            
            public void OnGUI() {
                if(Layout.BeginLayoutGroup(_flexibleHorizontalGroup)) {
                    var fixedRect = Layout.GetRect(40, 55);
                    EditorGUI.DrawRect(fixedRect, Color.black);
                    EditorGUI.LabelField(fixedRect, fixedRect.width.ToString());

                    for(int i = 0; i < 3; i++) {
                        var rect = Layout.GetRect(40, -1);
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
                        var rect = _treeViewGroup.GetLeafRect(40, -1);
                        EditorGUI.DrawRect(rect, Color.black);
                        EditorGUI.LabelField(rect, rect.width.ToString());
                    }

                    if(Layout.BeginLayoutGroup(_treeViewChildGroup)) {
                        for(int i = 0; i < 3; i++) {
                            var rect = _treeViewChildGroup.GetLeafRect(40, -1);
                            EditorGUI.DrawRect(rect, Color.black);
                            EditorGUI.LabelField(rect, rect.width.ToString());
                        }
                    }
                    Layout.EndLayoutGroup();
                    _treeViewGroup.DrawConnection(_treeViewChildGroup.ContentRect);
                    

                    for(int i = 0; i < 3; i++) {
                        var rect =  _treeViewGroup.GetLeafRect(40, -1);
                        EditorGUI.DrawRect(rect, Color.black);
                        EditorGUI.LabelField(rect, rect.width.ToString());
                    }
                }
                Layout.EndLayoutGroup();
            }
        }
    }
}