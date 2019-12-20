using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
//        public static LayoutGroupScope VerticalFadeScope(AnimBool animBool, int indent = 1) {
//            var eventType = Event.current.type;
//
//            LayoutGroup group;
//            if (eventType == EventType.Layout) {
//                group = new VerticalFadeLayoutGroup(animBool.faded, ExtendedEditorGUI.Resources.LayoutGroup.VerticalFadeGroup);
//                SubscribedForLayout.Enqueue(group);
//            }
//            else {
//                group = SubscribedForLayout.Dequeue();
//            }
//            
//            return new LayoutGroupScope(group, indent, eventType);
//        }

        public static void BeginVerticalFade(float amount, GUIStyle style) {
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