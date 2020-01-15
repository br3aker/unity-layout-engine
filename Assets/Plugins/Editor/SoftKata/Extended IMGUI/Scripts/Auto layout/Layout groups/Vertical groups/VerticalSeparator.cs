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

                DefaultEntryWidth -= _separatorWidth;
            }
            
            protected override void CalculateLayoutData() {
                TotalContainerWidth += _separatorWidth;
                NextEntryX += _separatorWidth;
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                // No need to check if current event is Repaint - EditorGUI.DrawRect checks it internally
                if (!IsGroupValid) return;
                
                var separatorLineRect = new Rect(0, 0, _separatorWidth, TotalContainerHeight);
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