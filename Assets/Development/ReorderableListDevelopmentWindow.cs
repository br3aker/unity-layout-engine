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

        private void OnEnable() {
            if (_alwaysRepaint) {
                EditorApplication.update += Repaint;
            }
            
            _cardExpandedAnim = new AnimBool(false, Repaint);
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

            ExtendedEditorGUI.StaticCard(new GUIContent("Static card"), CardContentDrawer);
            
            ExtendedEditorGUI.ExpandableCard(new GUIContent("Expandable card"), CardContentDrawer, _cardExpandedAnim);
        }
        
        private AnimBool _cardExpandedAnim;

        private static void CardContentDrawer() {
            if (LayoutEngine.GetRect(18f, -1, out var contentRect1)) {
                EditorGUI.LabelField(contentRect1, "Label with some text");
            }

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

            LayoutEngine.EndBeginTreeView();
        }
    }
}