using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static LayoutGroupScope VerticalFadeScope(AnimBool animBool, int indent = 1) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new VerticalFadeLayoutGroup(animBool.faded, ExtendedEditorGUI.Resources.LayoutGroup.VerticalFadeGroup);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
            }
            
            return new LayoutGroupScope(group, indent, eventType);
        }

        private class VerticalFadeLayoutGroup : VerticalLayoutGroup {
            private readonly float _amount;

            public VerticalFadeLayoutGroup(float amount, GUIStyle style) : base(style) {
                _amount = amount;
            }

            protected override float GetContainerHeight() {
                return LayoutData.TotalHeight * _amount;
            }
        }
    }
}