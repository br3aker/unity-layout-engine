using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        // TODO [optimization]: if faded equals to zero, this group behaves like a normal one
        internal class VerticalFadeGroup : VerticalClippingGroup {
            private float _faded;

            public VerticalFadeGroup(bool discardMargin, float faded, GUIStyle style) : base(discardMargin, style) {
                _faded = faded;
            }

            protected override void PreLayoutRequest() {
                TotalRequestedHeight *= _faded;
            }
        }

        public static bool BeginVerticalFadeGroup(bool discardMarginAndPadding, float faded, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalFadeGroup(discardMarginAndPadding, faded, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalFadeGroup(float faded, bool discardMarginAndPadding = false) {
            return BeginVerticalFadeGroup(discardMarginAndPadding, faded, ExtendedEditorGUI.Resources.LayoutGroup.VerticalFadeGroup);
        }

        public static void EndVerticalFadeGroup() {
            EndLayoutGroup<VerticalFadeGroup>();
        }
    }
}