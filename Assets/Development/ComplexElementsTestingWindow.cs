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

        private FoldableCard _cardWithArray;

        protected override void Initialize() {
            if (_alwaysRepaint) {
                EditorApplication.update += Repaint;
            }
            else {
                EditorApplication.update -= Repaint;
            }

            var tabHeaders = new[] {
                new GUIContent("Tab 1"),
                new GUIContent("Tab 2"),
                new GUIContent("Tab 3")
            };

            var cards = new IDrawableElement[] {
                new FoldableCard(new GUIContent("Card for tab 1"), new DelegateElement(DrawTab), false),
                new FoldableCard(new GUIContent("Card for tab 2"), new DelegateElement(DrawTab), false),
                new Card(new GUIContent("Card for tab 3"), new DelegateElement(DrawTab))
            };
            
            _tabsDrawer = new Tabs(0, tabHeaders, cards, new Color(0.06f, 0.51f, 0.75f));

            var listSize = 16;
            var numbersList = new List<int>(listSize);
            for(int i = 0; i < listSize; i++) {
                numbersList.Add(i);
            }

            Action<int, IDrawableElement, bool> bind = (data, drawable, selected) => {
                var stringLabel = drawable as StringLabelElement;
                stringLabel.Content = data.ToString();
                stringLabel.Selected = selected;
            };
            var fixedListView = new ListView<int, StringLabelElement>(numbersList, 350, 40, bind) {
                // Drag & drop
                ValidateDragData = () => {
                    return DragAndDropVisualMode.Link;
                },
                ExtractDragData = () => {
                    return DragAndDrop.objectReferences.Select((obj) => obj.GetInstanceID());
                },

                // Selection
                OnElementSelected = (index, drawer) => {
                    if(drawer != null) {
                        (drawer as StringLabelElement).Selected = true;
                    }
                },
                OnElementDeselected = (index, drawer) => {
                    if(drawer != null) {
                        (drawer as StringLabelElement).Selected = false;
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

            _cardWithArray = new FoldableCard(new GUIContent("Card with array contents"), fixedListView, true);
        }

        protected override void IMGUI() {
            DrawServiceInfo();

            //_tabsDrawer.OnGUI();
            Profiler.BeginSample("ListView test");
            _cardWithArray.OnGUI();
            Profiler.EndSample();
        }

        private void DrawServiceInfo() {
            if (LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var labelRect)) {
                EditorGUI.LabelField(labelRect,
                    $"Always repaint: [{_alwaysRepaint}] | [{Mathf.Sin((float) EditorApplication.timeSinceStartup)}]");
            }
            if (LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var updateButtonRect)) {
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

        private static void DrawTab() {
            if (LayoutEngine.BeginVerticalGroup(Constraints.DiscardBorder | Constraints.DiscardMargin | Constraints.DiscardPadding)) {
                if (LayoutEngine.GetRect(18f, -1, out var contentRect2)) {
                    EditorGUI.ToggleLeft(contentRect2, "Sub-header", true);
                }

                if (LayoutEngine.BeginTreeViewGroup()) {
                    for (int i = 0; i < 3; i++) {
                        if (LayoutEngine.GetRect(18f, -1, out var hierarchyRect)) {
                            EditorGUI.LabelField(hierarchyRect, $"Very long label with info #{i}");
                        }
                    }
                }
                LayoutEngine.EndTreeView();
            }
            LayoutEngine.EndVerticalGroup();
        }
    
        public class StringLabelElement : IAbsoluteDrawableElement {
            public string Content { get; set; }

            public bool Selected {get; set;}

            public StringLabelElement() : this("") { }
            public StringLabelElement(string content) {
                Content = content;
            }

            public void OnGUI() {
                if(LayoutEngine.GetRect(40, -1, out var rect)) {
                    if(Selected) {
                        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));
                    }
                    else {
                        EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
                    }
                    
                    EditorGUI.LabelField(rect, Content);
                }
            }
            public void OnGUI(Vector2 position) {
                var rect = new Rect(position, new Vector2(LayoutEngine.CurrentContentWidth, 40));
                EditorGUI.DrawRect(rect, Color.grey);
                EditorGUI.LabelField(rect, Content);
            }
        }
    }
}