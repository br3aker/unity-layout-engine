using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalSeparator : VerticalGroup {
            private float _separatorWidth;

            private Color _activeSeparatorColor;
            private Color _disabledSeparatorColor;

            public VerticalSeparator(GUIStyle style) : base(false, style) {
                _separatorWidth = style.border.left;
                
                _activeSeparatorColor = style.onNormal.textColor;
                _disabledSeparatorColor = style.normal.textColor;

                AutomaticEntryWidth -= _separatorWidth;
            }
            
            protected override void PreLayoutRequest() {
//                ServiceWidth += _separatorWidth;
//                NextEntryPosition.x += _separatorWidth;
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                var separatorLineRect = new Rect(ContentRect.x - Padding.left, ContentRect.y - Padding.top, _separatorWidth, TotalRequestedHeight - Margin.vertical);
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
            EndLayoutGroup<VerticalSeparator>();
        }
    }
}