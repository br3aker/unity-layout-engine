using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static bool BeginHorizontalFade(float amount) {
            return BeginHorizontalFade(amount, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalFadeGroup);
        }
        public static bool BeginHorizontalFade(float amount, GUIStyle style) {
            if (amount > 0) {
                var eventType = Event.current.type;

                LayoutGroupBase group;
                if (eventType == EventType.Layout) {
                    group = new HorizontalFadeLayoutGroup(amount, style);
                    SubscribedForLayout.Enqueue(group);

                }
                else {
                    group = SubscribedForLayout.Dequeue();
                    group.PushLayoutRequest();
                }

                ActiveGroupStack.Push(group);
                TopGroup = group;

                return true;
            }

            return false;
        }
        public static void EndHorizontalFade() {
            EndLayoutGroup();
        }

        private class HorizontalFadeLayoutGroup : HorizontalLayoutGroup {
            private readonly float _amount;

            public HorizontalFadeLayoutGroup(float amount, GUIStyle style) : base(style) {
                _amount = amount;
            }
            
            protected override float GetContainerWidth() {
                return FullRect.width * _amount;
            }
        }
    }
}