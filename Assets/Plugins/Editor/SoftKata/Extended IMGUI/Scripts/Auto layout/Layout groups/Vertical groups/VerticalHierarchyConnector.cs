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

            public VerticalHierarchyGroup(GUIStyle style) : base(false, style) {
                _connectionLineWidth = Border.left;
                _connectionLineLength = Padding.left + Border.right;

                _connectionLineColor = style.normal.textColor;
            }

            protected override Rect GetRectInternal(float x, float y, float height, float width) {
                _lastEntryHeight = height;
                _lastEntryY = y;
                
                var horizontalLine = new Rect(
                    ContentRect.x - Padding.left, y + height / 2,
                    _connectionLineLength, _connectionLineWidth
                );
                EditorGUI.DrawRect(horizontalLine, _connectionLineColor);
                
                return new Rect(x, y, width, height);
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                var verticalLineRect = new Rect(
                    ContentRect.x - Padding.left, ContentRect.y - Padding.top,
                    _connectionLineWidth,
                    (_lastEntryY - ContentRect.y) + _lastEntryHeight
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
            EndLayoutGroup<VerticalHierarchyGroup>();
        }
    }
}