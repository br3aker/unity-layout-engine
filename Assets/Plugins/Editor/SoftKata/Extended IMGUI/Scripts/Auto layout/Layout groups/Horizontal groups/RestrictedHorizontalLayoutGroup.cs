using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class FixedHorizontalLayoutGroup : HorizontalLayoutGroupBase {
            private float _containerContainerWidth;
            
            private int _fixedEntriesCount;
            private float _totalFixedEntriesWidth;
            
            private float _flexibleElementWidth = -1f;
            
            
            public FixedHorizontalLayoutGroup(float containerWidth, GUIStyle style) : base(-1f, style) {
                TotalContainerWidth = containerWidth - Margin.horizontal;
                _containerContainerWidth = containerWidth - Margin.horizontal;
            }

            protected override void CalculateLayoutData() {
                _containerContainerWidth -= ContentOffset.x * (EntriesCount - 1); 
                TotalContainerWidth -= ContentOffset.x * (EntriesCount - 1);
                _flexibleElementWidth = (_containerContainerWidth - _totalFixedEntriesWidth) / (EntriesCount - _fixedEntriesCount);
            }

            
            internal override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    if (width > 0) {
                        _totalFixedEntriesWidth += width;
                        _fixedEntriesCount++;
                    }
                    EntriesCount++;
                    TotalContainerHeight = Mathf.Max(TotalContainerHeight, height);
                    
                    return LayoutDummyRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }

                var calculatedWidth = width > 0 ? width : _flexibleElementWidth;
                var entryRect = GetActualRect(height, calculatedWidth);
                NextEntryX += calculatedWidth + ContentOffset.x;
                return entryRect;
            }
        }

        public static bool BeginRestrictedHorizontalGroup(float width, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new FixedHorizontalLayoutGroup(width, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
                layoutGroup.RegisterDebugData();
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginRestrictedHorizontalGroup(float width) {
            return BeginRestrictedHorizontalGroup(width, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalRestrictedGroup);
        }

        public static void EndRestrictedHorizontalGroup() {
            var lastGroup = EndLayoutGroup() as FixedHorizontalLayoutGroup;
            if (lastGroup == null) {
                throw new Exception("Layout group type mismatch");
            }
        }
    }
}