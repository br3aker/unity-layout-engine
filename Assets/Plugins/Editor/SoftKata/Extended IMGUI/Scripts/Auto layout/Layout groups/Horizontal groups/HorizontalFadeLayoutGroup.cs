using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static bool BeginHorizontalFade(float amount) {
            return BeginHorizontalFade(amount, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalFadeGroup);
        }
        public static bool BeginHorizontalFade(float amount, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroupBase group;
            if (eventType == EventType.Layout) {
                group = new HorizontalFadeLayoutGroup(amount, TopGroup, style);
                SubscribedForLayout.Enqueue(group);
                
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;

            return amount > 0;
        }
        public static void EndHorizontalFade() {
            EndLayoutGroup();
        }

        private class HorizontalFadeLayoutGroup : HorizontalLayoutGroup {
            private readonly float _amount;

            public HorizontalFadeLayoutGroup(float amount, LayoutGroupBase parent, GUIStyle style) : base(parent, style) {
                _amount = amount;
            }
            
            protected override float GetContainerWidth() {
                return FullRect.width * _amount;
            }
        }
    }
}