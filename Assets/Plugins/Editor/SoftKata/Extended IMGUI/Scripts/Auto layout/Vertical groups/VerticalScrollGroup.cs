using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
//        public static LayoutGroupScope VerticalScrollScope(float height, float scrollPos, int indent = 1) {
//            var eventType = Event.current.type;
//
//            LayoutGroup group;
//            if (eventType == EventType.Layout) {
//                group = new VerticalScrollGroup(height, scrollPos, ExtendedEditorGUI.Resources.LayoutGroup.VerticalScrollGroup);
//                SubscribedForLayout.Enqueue(group);
//            }
//            else {
//                group = SubscribedForLayout.Dequeue();
//            }
//            
//            return new LayoutGroupScope(group, indent, eventType);
//        }

        public static void BeginVerticalScroll(float height, float scrollPos) {
            BeginVerticalScroll(height, scrollPos, ExtendedEditorGUI.Resources.LayoutGroup.VerticalScrollGroup);
        }

        public static void BeginVerticalScroll(float height, float scrollPos, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new VerticalScrollGroup(height, scrollPos, TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;
        } 
        public static void EndVerticalScroll() {
            var eventType = Event.current.type;

            if (eventType == EventType.Layout) {
                TopGroup.PushLayoutRequest();
            }
            
            TopGroup.EndGroup();
            TopGroup = TopGroup.Parent;
            ActiveGroupStack.Pop();
        }

        private class VerticalScrollGroup : VerticalLayoutGroup {
            private readonly float _height;
            private readonly float _scrollPos;

            public VerticalScrollGroup(float height, float scrollPos, LayoutGroup parent, GUIStyle style) : base(parent, style) {
                _height = height;
                _scrollPos = scrollPos;
            }

            internal override void PushLayoutRequest() {
                if (LayoutData) {
                    FullRect = Parent?.GetRect(_height) ?? RequestIndentedRect(_height);
                    
                    _nextEntryY = Mathf.Lerp(0, _height - LayoutData.TotalHeight, _scrollPos);
                }
                
                GUI.BeginClip(FullRect);
            }
            
            internal override void EndGroup() {
                GUI.EndClip();
            }
        }
    }
}