using System;
using SoftKata.ExtendedEditorGUI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Development {
    public class ComplexElementsTestingWindow : ExtendedEditorWindow {
        #region Window initialization & lifetime management
        [MenuItem("Window/Complex elements")]
        static void Init() {
            GetWindow<ComplexElementsTestingWindow>(false, "Complex elements");
        }
        #endregion

        private bool _alwaysRepaint;

        private int _selectedTab;
        private GUIContent[] _tabHeaders;
        private Action[] _tabContentDrawers;

        private ExtendedEditorGUI.TabsElement _tabsDrawer;

        protected override void Initialize() {
            if (_alwaysRepaint) {
                EditorApplication.update += Repaint;
            }
            else{
                EditorApplication.update -= Repaint;
            }

            var tabHeaders = new[] {
                new GUIContent("Tab 1"),
                new GUIContent("Tab 2"),
                new GUIContent("Tab 3")
            };

            var cards = new[] {
                new ExtendedEditorGUI.CardElement(tabHeaders[0], DrawTab),
                new ExtendedEditorGUI.CardElement(tabHeaders[1], DrawTab),
                new ExtendedEditorGUI.CardElement(tabHeaders[2], DrawTab)
            };

            var tabContentDrawers =  new Action[] {
                cards[0].OnGUI,
                cards[1].OnGUI,
                cards[2].OnGUI
            };
            
            _tabsDrawer = new ExtendedEditorGUI.TabsElement(_selectedTab, tabHeaders, tabContentDrawers, new Color(0.06f, 0.51f, 0.75f));
        }

        protected override void IMGUI() {
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


            _selectedTab = _tabsDrawer.OnGUI();
        }

        private static void DrawTab() {
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.DiscardBorder | GroupModifier.DiscardMargin | GroupModifier.DiscardPadding)) {
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
    }
}