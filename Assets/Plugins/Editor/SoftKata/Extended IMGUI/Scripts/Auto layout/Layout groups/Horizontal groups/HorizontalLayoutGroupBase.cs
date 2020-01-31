using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalGroup : LayoutGroupBase {
            public HorizontalGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) {}

            protected override bool RegisterNewEntry(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    if (width > 0f) {
                        TotalRequestedWidth += width;
                    }
                    TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, height);
                    return false;
                }

                if (!IsGroupValid) {
                    return false;
                }

                
                NextEntryPosition.x += width + ContentOffset.x;

                // occlusion
                if (CurrentEntryPosition.x + width < VisibleAreaRect.x || CurrentEntryPosition.x > VisibleAreaRect.xMax) {
                    return false;
                }

                return true;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth += elementWidth * count;
                TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, elementHeight);
            }
        }

        public static bool BeginHorizontalGroup(bool discardMarginAndPadding, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalGroup(discardMarginAndPadding, style);
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
            EndLayoutGroup<HorizontalGroup>();
        }
    }
}