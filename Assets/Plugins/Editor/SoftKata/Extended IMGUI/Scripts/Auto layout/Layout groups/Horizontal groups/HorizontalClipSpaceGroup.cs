using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalClipSpaceGroup : HorizontalLayoutGroup {
            public HorizontalClipSpaceGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) { }
            
            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryPosition.x + width < 0 || NextEntryPosition.x > FullContainerRect.width) {
                    return InvalidRect;
                }
                
                return new Rect(NextEntryPosition.x, NextEntryPosition.y, width, height);
            }
        }
        
        public static bool BeginHorizontalClipSpaceGroup(bool discardMarginAndPadding, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalClipSpaceGroup(discardMarginAndPadding, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHorizontalClipSpaceGroup(bool discardMarginAndPadding = false) {
            return BeginHorizontalClipSpaceGroup(discardMarginAndPadding, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
        }

        public static void EndHorizontalClipSpaceGroup() {
            EndLayoutGroup();
        }
    }
}