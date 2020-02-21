using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        // Does not support nesting
        internal class VerticalHierarchyGroup : VerticalGroup {
            private float _connectionLineWidth;
            private float _connectionLineLength;
            
            private Color _connectionLineColor;
            
            private float _lastEntryHeight;
            private float _lastEntryY;

            public VerticalHierarchyGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                _connectionLineWidth = Border.left;
                _connectionLineLength = Padding.left + Border.right;

                _connectionLineColor = style.normal.textColor;
            }

            protected override Rect GetRectInternal(float x, float y, float height, float width) {
                _lastEntryHeight = height;
                _lastEntryY = y;
                
                var horizontalLine = new Rect(
                    ContainerRect.x - Padding.left, y + height / 2,
                    _connectionLineLength, _connectionLineWidth
                );
                EditorGUI.DrawRect(horizontalLine, _connectionLineColor);
                
                return new Rect(x, y, width, height);
            }

            internal void DrawMajorConnectionType() {
                var verticalLineRect = new Rect(
                    ContainerRect.x - Padding.left, ContainerRect.y - Padding.top,
                    _connectionLineWidth,
                    (_lastEntryY - ContainerRect.y) + _lastEntryHeight
                );
                
                EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
            } 
        }
        
        public static bool BeginVerticalHierarchyGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterGroup(new VerticalHierarchyGroup(modifier, style));
            }

            return GatherGroup().IsGroupValid;
        }
        public static bool BeginVerticalHierarchyGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalHierarchyGroup(modifier, ExtendedEditorGUI.Resources.LayoutGroups.VerticalHierarchyGroup);
        }

        public static void EndVerticalHierarchyGroup() {
            var group = EndLayoutGroup<VerticalHierarchyGroup>();
            group.DrawMajorConnectionType();
        }
    }
}