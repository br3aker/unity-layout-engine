using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static bool BeginVerticalFade(float amount) {
            return BeginVerticalFade(amount, ExtendedEditorGUI.Resources.LayoutGroup.VerticalFadeGroup);
        }
        public static bool BeginVerticalFade(float amount, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroupBase group;
            if (eventType == EventType.Layout) {
                group = new VerticalFadeLayoutGroup(amount, TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;

            return amount > 0f;
        }
        public static void EndVerticalFade() {
            EndLayoutGroup();
        }

        private class VerticalFadeLayoutGroup : VerticalLayoutGroup {
            private readonly float _amount;

            public VerticalFadeLayoutGroup(float amount, LayoutGroupBase parent, GUIStyle style) : base(parent, style) {
                _amount = amount;
            }

            protected override float GetContainerHeight() {
                return LayoutData.TotalHeight * _amount;
            }
        }
    }
}