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

            LayoutGroup group;
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
            var eventType = Event.current.type;

            if (eventType == EventType.Layout) {
                TopGroup.PushLayoutRequest();
            }
            
            TopGroup.EndGroup();
            TopGroup = TopGroup.Parent;
            ActiveGroupStack.Pop();
        }

        private class VerticalFadeLayoutGroup : VerticalLayoutGroup {
            private readonly float _amount;

            public VerticalFadeLayoutGroup(float amount, LayoutGroup parent, GUIStyle style) : base(parent, style) {
                _amount = amount;
            }

            protected override float GetContainerHeight() {
                return LayoutData.TotalHeight * _amount;
            }
        }
    }
}