using System;
using SoftKata.ExtendedEditorGUI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Development {
    public class ReorderableListDevelopmentWindow : ExtendedEditorWindow {
        #region Window initialization & lifetime management
        [MenuItem("Window/Reorderable list")]
        static void Init() {
            GetWindow<ReorderableListDevelopmentWindow>(false, "Reorderable list").Show();
        }
        #endregion

        private bool _alwaysRepaint;

        private int _selectedTab;
        private GUIContent[] _tabHeaders;
        private Action[] _tabContentDrawers;
        private ExtendedEditorGUI.ScrollableTabsHolder tabsDrawer;

        private void OnEnable() {
            if (_alwaysRepaint) {
                EditorApplication.update += Repaint;
            }

            _tabHeaders = new[] {
                new GUIContent("Tab 1"),
                new GUIContent("Tab 2"),
                new GUIContent("Tab 3")
            };

            _tabContentDrawers =  new Action[] {
                () => ExtendedEditorGUI.StaticCard(new GUIContent("Tab 1"), DrawTab),
                () => ExtendedEditorGUI.StaticCard(new GUIContent("Tab 2"), DrawTab),
                () => ExtendedEditorGUI.StaticCard(new GUIContent("Tab 3"), DrawTab)
            };
            
            tabsDrawer = new ExtendedEditorGUI.ScrollableTabsHolder(_selectedTab, _tabHeaders, _tabContentDrawers, new AnimFloat(_selectedTab, Repaint), new Color(0.06f, 0.51f, 0.75f));
        }

        protected override void IMGUI() {
            if (LayoutEngine.GetRect(ExtendedEditorGUI.LabelHeight, -1, out var labelRect)) {
                EditorGUI.LabelField(labelRect,
                    $"Always repaint: [{_alwaysRepaint}] | [{Mathf.Sin((float) EditorApplication.timeSinceStartup)}]");
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


            _selectedTab = tabsDrawer.DoScrollableTabs();
        }
        
        private AnimBool _cardExpandedAnim;

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