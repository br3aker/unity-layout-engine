using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalLayoutGroup : LayoutGroupBase {
            public HorizontalLayoutGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) {}

            protected override void CalculateLayoutData() {
//                TotalRequestedWidth = MaxAllowedWidth;
            }

            internal override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    TotalRequestedWidth += width;
                    TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, height);
                    return InvalidRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }
                
                if (width < 0f) {
                    width = MaxAllowedWidth;
                }

                var entryRect = GetActualRect(height, width);
                NextEntryPosition.x += width + ContentOffset.x;
                return entryRect;
            }
            
            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth += elementWidth * count;
                TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, elementHeight);
            }

            protected virtual Rect GetActualRect(float height, float width) {
                if (NextEntryPosition.x + width < FullContainerRect.x || NextEntryPosition.x > FullContainerRect.xMax) {
                    return InvalidRect;
                }
                
                return new Rect(NextEntryPosition.x, NextEntryPosition.y, width, height);
            }
        }

        public static bool BeginHorizontalGroup(bool discardMarginAndPadding, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalLayoutGroup(discardMarginAndPadding, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHorizontalGroup(bool discardMarginAndPadding = false) {
            return BeginHorizontalGroup(discardMarginAndPadding, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
        }

        public static void EndHorizontalGroup() {
            EndLayoutGroup();
        }
    }
}