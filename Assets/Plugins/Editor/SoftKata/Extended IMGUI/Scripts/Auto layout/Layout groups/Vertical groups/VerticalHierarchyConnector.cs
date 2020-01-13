using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        // Does not support nesting
        internal class VerticalHierarchyGroup : VerticalLayoutGroupBase {
            private float _connectorX = 0;
            private float _connectorY = 0;
            
            private float _connectionLineWidth;
            private float _connectionLineLength;

            private float _lastEntryHeight;

            private Color _connectionLineColor;

            public VerticalHierarchyGroup(GUIStyle style) : base(style) {
                _connectionLineWidth = style.border.left;
                _connectionLineLength = Padding.left - style.border.right;

                _connectionLineColor = style.normal.textColor;
            }
            
            protected override void CalculateLayoutData() {
                TotalContainerWidth += _connectionLineWidth;
                NextEntryX += _connectionLineWidth;
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryY + height < 0 || NextEntryY > FullRect.height) {
                    return InvalidRect;
                }
                
                _lastEntryHeight = height;
                _connectorY = NextEntryY + height / 2;
                
                var horizontalLine = new Rect(
                    _connectorX, _connectorY,
                    _connectionLineLength, _connectionLineWidth
                );

                EditorGUI.DrawRect(horizontalLine, _connectionLineColor);

                return new Rect(NextEntryX, NextEntryY, width, height);
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                // No need to check if current event is Repaint - EditorGUI.DrawRect checks it internally
                if (!IsGroupValid) return;
                
                var verticalLineRect = new Rect(
                    0, 0,
                    _connectionLineWidth, TotalContainerHeight - _lastEntryHeight / 2
                );
                
                EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
            } 
        }
        
        public static bool BeginVerticalHierarchyGroup(GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalHierarchyGroup(style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
                layoutGroup.RegisterDebugData();
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalHierarchyGroup() {
            return BeginVerticalHierarchyGroup(ExtendedEditorGUI.Resources.LayoutGroup.VerticalHierarchyGroup);
        }

        public static void EndVerticalHierarchyGroup() {
            EndLayoutGroup();
        }
    }
}