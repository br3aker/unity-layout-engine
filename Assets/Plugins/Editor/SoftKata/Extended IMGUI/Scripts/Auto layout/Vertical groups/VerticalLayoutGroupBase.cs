using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static void BeginVerticalScope() {
            BeginVerticalScope(ExtendedEditorGUI.Resources.LayoutGroup.VerticalGroup);
        }
        public static void BeginVerticalScope(GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new VerticalLayoutGroup(TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;
        }
        public static void EndVerticalScope() {
            var eventType = Event.current.type;

            if (eventType == EventType.Layout) {
                TopGroup.PushLayoutRequest();
            }
            
            TopGroup.EndGroup();
            TopGroup = TopGroup.Parent;
            ActiveGroupStack.Pop();
        }
        

        private class VerticalLayoutGroupData : LayoutGroupDataBase {
            private readonly float _verticalContentOffset;

            public override float TotalHeight => _height + _verticalContentOffset * (_entries.Count - 1);

            public VerticalLayoutGroupData(float contentOffset) {
                _verticalContentOffset = contentOffset;
            }
            
            public override void AddEntry(float height) {
                _height += height;
                _entries.Enqueue(new LayoutEntry{Height = height});
            }
        }
        private class VerticalLayoutGroup : LayoutGroup {
            private readonly float _contentVerticalGap;


            public VerticalLayoutGroup(LayoutGroup parent, GUIStyle style) : base(parent, style) {
                _contentVerticalGap = style.contentOffset.y;
                LayoutData = new VerticalLayoutGroupData(_contentVerticalGap);
            }
            
            protected virtual float GetContainerHeight() {
                return LayoutData.TotalHeight;
            }

            internal override void PushLayoutRequest() {
                if (LayoutData) {
                    var containerHeight = GetContainerHeight();
                    FullRect = Parent?.GetRect(containerHeight) ?? RequestIndentedRect(containerHeight);
                }
                
                GUI.BeginClip(FullRect);
            }

            internal override Rect GetRect(float height) {
                if (Event.current.type == EventType.Layout) {
                    LayoutData.AddEntry(height);
                    return LayoutDummyRect;
                }
                
                var entryRect = LayoutData.FetchNextRect(_nextEntryX, _nextEntryY, IsFixedWidth ? FixedWidth : FullRect.width, height);
                _nextEntryY += height + _contentVerticalGap;
                return _nextEntryY > 0f && entryRect.y < FullRect.height ? entryRect : InvalidDummyRect;
            }
        }
    }
}