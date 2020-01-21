using System.Collections.Specialized;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalLayoutGroupBase : LayoutGroupBase {
            public VerticalLayoutGroupBase(bool discardMargin, GUIStyle style) : base(discardMargin, style) {
                TotalRequestedWidth = float.MinValue; // this setup is used auto-defined width layout calls
            }

            internal sealed override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    TotalRequestedWidth = Mathf.Max(TotalRequestedWidth, width);
                    TotalRequestedHeight += height;
                    return InvalidRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }
                
                if (width < 0f) {
                    width = MaxAllowedWidth;
                }

                var entryRect = GetActualRect(height, width);
                NextEntryPosition.y += height + ContentOffset.y;
                return entryRect;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth = Mathf.Max(TotalRequestedWidth, elementWidth);
                TotalRequestedHeight += elementHeight * count;
            }

            protected virtual Rect GetActualRect(float height, float width) {
                if (NextEntryPosition.y + height < FullContainerRect.y  || NextEntryPosition.y > FullContainerRect.yMax) {
                    return InvalidRect;
                }

                return new Rect(NextEntryPosition.x, NextEntryPosition.y, width, height);
            }
        }

        public static bool BeginVerticalGroup(bool discardMarginAndPadding, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalLayoutGroupBase(discardMarginAndPadding, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalGroup(bool discardMarginAndPadding = false) {
            return BeginVerticalGroup(discardMarginAndPadding, ExtendedEditorGUI.Resources.LayoutGroup.VerticalGroup);
        }

        public static void EndVerticalGroup() {
            EndLayoutGroup();
        }
    }
}