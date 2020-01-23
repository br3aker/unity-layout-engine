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

            internal override GroupRectData GetGroupRect(float height, float width) {
                var currentEntryPosition = NextEntryPosition;
                var groupRectData = new GroupRectData {
                    Rect = GetRect(height, width),
                    OffsetFromParentRect = new Vector2(currentEntryPosition.x, FullContainerRect.y)
                };
                return groupRectData;
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

                var currentEntryX = NextEntryPosition.x;
                NextEntryPosition.x += width + ContentOffset.x;
                
                if (currentEntryX + width < FullContainerRect.x || currentEntryX > FullContainerRect.xMax) {
//                    Debug.Log($"{currentEntryX} | Borders: {FullContainerRect.x} - {FullContainerRect.xMax}");
                    return InvalidRect;
                }
                
                return GetActualRect(currentEntryX, NextEntryPosition.y, height, width);
            }
            
            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth += elementWidth * count;
                TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, elementHeight);
            }

            protected virtual Rect GetActualRect(float x, float y, float height, float width) {
//                if (NextEntryPosition.x + width < FullContainerRect.x || NextEntryPosition.x > FullContainerRect.xMax) {
//                    return InvalidRect;
//                }
                
                return new Rect(x, y, width, height);
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