using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        // Does not support nesting
        internal class VerticalHierarchyGroup : VerticalLayoutGroupBase {
            private float _connectorLineY;
            
            private float _connectionLineWidth;
            private float _connectionLineLength;
            
            private Color _connectionLineColor;
            
            private float _lastEntryHeight;

            public VerticalHierarchyGroup(GUIStyle style) : base(false, style) {
                _connectionLineWidth = Border.left;
                _connectionLineLength = Padding.left + Border.right;

                _connectionLineColor = style.normal.textColor;
            }
            
            protected override void CalculateLayoutData() {
                NextEntryPosition.x += _connectionLineWidth;
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryPosition.y + height < FullContainerRect.y || NextEntryPosition.y > FullContainerRect.yMax) {
                    return InvalidRect;
                }

                _lastEntryHeight = height;
                _connectorLineY = NextEntryPosition.y + height / 2;
                
                var horizontalLine = new Rect(
                    FullContainerRect.x - Padding.left, _connectorLineY,
                    _connectionLineLength, _connectionLineWidth
                );

                EditorGUI.DrawRect(horizontalLine, _connectionLineColor);

                return new Rect(NextEntryPosition.x, NextEntryPosition.y, width - _connectionLineWidth, height);
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                // No need to check if current event is Repaint - EditorGUI.DrawRect checks it internally
                if (!IsGroupValid) return;
                
                var verticalLineRect = new Rect(
                    FullContainerRect.x - Padding.left, FullContainerRect.y - Padding.top,
                    _connectionLineWidth,
                    TotalRequestedHeight - _lastEntryHeight / 2
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