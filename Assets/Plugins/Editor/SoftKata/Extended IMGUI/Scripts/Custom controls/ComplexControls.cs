using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        public static void StaticCard(GUIContent header, Action contentDrawer, ComplexControlsResources.CardElementData styleData) {
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.None, styleData.RootStyle)) {
                // Background
                EditorGUI.DrawRect(LayoutEngine.CurrentGroup.GetContentRect(), styleData.BackgroundColor);

                // Header
                if (LayoutEngine.GetRect(18f, -1, out var headerRect)) {
                    EditorGUI.LabelField(headerRect, header, styleData.HeaderStyle);
                }
            
                // Separator
                if (LayoutEngine.GetRect(1f, -1, out var separatorRect)) {
                    ExtendedEditorGUI.DrawSeparator(separatorRect);
                }
            
                // Content
                if (LayoutEngine.BeginVerticalGroup(GroupModifier.DiscardMargin, styleData.ContentStyle)) {
                    contentDrawer?.Invoke();
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        public static void StaticCard(GUIContent header, Action contentDrawer) {
            StaticCard(header, contentDrawer, ExtendedEditorGUI.ComplexControlsResources.StaticCard);
        }
        
        public static void ExpandableCard(GUIContent header, Action contentDrawer, AnimBool expanded, ComplexControlsResources.CardElementData styleData) {
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.None, styleData.RootStyle)) {
                // Background
                EditorGUI.DrawRect(LayoutEngine.CurrentGroup.GetContentRect(), styleData.BackgroundColor);
                
                // Header
                if (LayoutEngine.GetRect(18f, -1, out var headerRect)) {
                    expanded.target = EditorGUI.Foldout(headerRect, expanded.target, header, true, styleData.HeaderStyle);
                }
                
                // Separator
                if (expanded.faded > 0.01f && LayoutEngine.GetRect(1f, -1, out var separatorRect)) {
                    ExtendedEditorGUI.DrawSeparator(separatorRect);
                }
                
                if (LayoutEngine.BeginVerticalFadeGroup(expanded.faded, GroupModifier.DiscardMargin, styleData.ContentStyle)) {
                    // Content
                    contentDrawer?.Invoke();
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        public static void ExpandableCard(GUIContent header, Action contentDrawer, AnimBool expanded) {
            ExpandableCard(header, contentDrawer, expanded, ExtendedEditorGUI.ComplexControlsResources.ExpandableCard);
        }
    }
}