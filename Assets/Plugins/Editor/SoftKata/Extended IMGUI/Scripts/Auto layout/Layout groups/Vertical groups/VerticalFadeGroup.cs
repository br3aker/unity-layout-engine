using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        // TODO [optimization]: if faded equals to zero, this group behaves like a normal one
        internal class VerticalFadeGroup : VerticalClippingGroup {
            private float _faded;

            public VerticalFadeGroup(float faded, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                _faded = faded;
            }

            protected override void PreLayoutRequest() {
                TotalRequestedHeight *= _faded;
            }
        }

        public static bool BeginVerticalFadeGroup(float faded, GroupModifier modifier, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalFadeGroup(faded, modifier, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalFadeGroup(float faded, GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalFadeGroup(faded, modifier, ExtendedEditorGUI.Resources.LayoutGroup.VerticalFadeGroup);
        }

        public static void EndVerticalFadeGroup() {
            EndLayoutGroup<VerticalFadeGroup>();
        }
    }
}