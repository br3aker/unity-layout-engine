using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalSeparator : VerticalLayoutGroupBase {
            private float _separatorWidth;

            private Color _activeSeparatorColor;
            private Color _disabledSeparatorColor;

            public VerticalSeparator(GUIStyle style) : base(false, style) {
                _separatorWidth = style.border.left;
                
                _activeSeparatorColor = style.onNormal.textColor;
                _disabledSeparatorColor = style.normal.textColor;

                MaxAllowedWidth -= _separatorWidth;
            }
            
            protected override void CalculateLayoutData() {
                TotalRequestedWidth += _separatorWidth;
                NextEntryPosition.x += _separatorWidth;
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                // No need to check if current event is Repaint - EditorGUI.DrawRect checks it internally
                if (!IsGroupValid) return;
                
                var separatorLineRect = new Rect(FullContainerRect.x - Padding.left, FullContainerRect.y - Padding.top, _separatorWidth, TotalRequestedHeight - Margin.vertical);
                EditorGUI.DrawRect(separatorLineRect, GUI.enabled ? _activeSeparatorColor : _disabledSeparatorColor);
            } 
        }
        
        public static bool BeginVerticalSeparatorGroup(GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalSeparator(style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalSeparatorGroup() {
            return BeginVerticalSeparatorGroup(ExtendedEditorGUI.Resources.LayoutGroup.VerticalSeparatorGroup);
        }

        public static void EndVerticalSeparatorGroup() {
            EndLayoutGroup();
        }
    }
}