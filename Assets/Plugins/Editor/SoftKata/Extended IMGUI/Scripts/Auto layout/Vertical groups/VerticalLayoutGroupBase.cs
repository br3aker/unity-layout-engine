using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static LayoutGroupScope VerticalScope(int indent = 1) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new VerticalLayoutGroup(ExtendedEditorGUI.Resources.LayoutGroup.VerticalGroup);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
            }
            
            return new LayoutGroupScope(group, indent, eventType);
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


            public VerticalLayoutGroup(GUIStyle style) : base(style) {
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