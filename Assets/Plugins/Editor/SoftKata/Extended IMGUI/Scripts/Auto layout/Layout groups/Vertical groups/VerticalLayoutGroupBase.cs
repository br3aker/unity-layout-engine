using System.Collections.Specialized;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalLayoutGroupBase : LayoutGroupBase {
            public VerticalLayoutGroupBase(bool discardMargin, GUIStyle style) : base(discardMargin, style) {
                TotalRequestedWidth = float.MinValue; // this setup is used auto-defined width layout calls
            }

            protected override Vector2 GetContentBorderValues(bool isClippedByParentGroup) {
                if (isClippedByParentGroup) {
                    return new Vector2(0f, FullContainerRect.height);
                }
                return new Vector2(FullContainerRect.y, FullContainerRect.yMax);
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

                var currentEntryY = NextEntryPosition.y;
                NextEntryPosition.y += height + ContentOffset.y;

                if (currentEntryY + height < EntryRectBorders.x || currentEntryY > EntryRectBorders.y) {
                    return InvalidRect;
                }
                
                return GetActualRect(NextEntryPosition.x, currentEntryY, height, width);
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth = Mathf.Max(TotalRequestedWidth, elementWidth);
                TotalRequestedHeight += elementHeight * count;
            }

            protected virtual Rect GetActualRect(float x, float y, float height, float width) {
//                if (NextEntryPosition.y + height < FullContainerRect.y  || NextEntryPosition.y > FullContainerRect.yMax) {
//                    return InvalidRect;
//                }

                return new Rect(x, y, width, height);
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