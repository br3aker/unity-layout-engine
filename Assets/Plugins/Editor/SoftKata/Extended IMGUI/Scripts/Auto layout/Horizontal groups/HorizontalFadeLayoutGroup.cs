using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static bool BeginHorizontalFade(float amount) {
            return BeginHorizontalFade(amount, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
        }
        public static bool BeginHorizontalFade(float amount, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroup group;
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
            var eventType = Event.current.type;

            if (eventType == EventType.Layout) {
                TopGroup.PushLayoutRequest();
            }
            
            TopGroup.EndGroup();
            TopGroup = TopGroup.Parent;
            ActiveGroupStack.Pop();
        }

        private class HorizontalFadeLayoutGroup : HorizontalLayoutGroup {
            private readonly float _amount;

            public HorizontalFadeLayoutGroup(float amount, LayoutGroup parent, GUIStyle style) : base(parent, style) {
                _amount = amount;
            }
            
            protected override float GetContainerWidth() {
                return FullRect.width * _amount;
            }
        }
    }
}