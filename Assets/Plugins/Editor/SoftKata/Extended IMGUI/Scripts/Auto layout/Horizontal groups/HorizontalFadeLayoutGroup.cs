using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static LayoutGroupScope HorizontalFadeScope(AnimBool animBool, int indent = 1) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new HorizontalFadeLayoutGroup(animBool.faded, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
            }
            
            return new LayoutGroupScope(group, indent, eventType);
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