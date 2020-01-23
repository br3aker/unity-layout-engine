//using System;
//using UnityEditor;
//using UnityEngine;
//
//
//namespace SoftKata.ExtendedEditorGUI {
//    public static partial class LayoutEngine {
//        internal class FlexibleHorizontalLayoutGroup : HorizontalLayoutGroup {
//            private float _containerWidth;
//            
//            private int _fixedEntriesCount;
//            private float _totalFixedEntriesWidth;
//            
//            private float _flexibleElementWidth = -1f;
//
//            private float _horizontalContentOffset; 
//            
//            public FlexibleHorizontalLayoutGroup(bool discardMargin, float width, GUIStyle style) : base(discardMargin, style) {
//                TotalRequestedWidth = width;
//                _containerWidth = TotalRequestedWidth;
//                MaxAllowedWidth = -1f;
//
//                
//                _horizontalContentOffset = ContentOffset.x;
//                ContentOffset = new Vector2(0, 0);
//            }
//
//            protected override void CalculateLayoutData() {
//                _containerWidth -= _horizontalContentOffset * (EntriesCount - 1) + Padding.horizontal + Margin.horizontal;
//                _flexibleElementWidth = (_containerWidth - _totalFixedEntriesWidth) / (EntriesCount - _fixedEntriesCount);
//            }
//            
//            internal override Rect GetRect(float height) {
//                return GetRect(height, -1f);
//            }
//            
//            internal override Rect GetRect(float height, float width) {
//                if (CurrentEventType == EventType.Layout) {
//                    if (width > 0f) {
//                        _totalFixedEntriesWidth += width;
//                        _fixedEntriesCount++;
//                    }
//                    EntriesCount++;
//                    TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, height);
//
//                    return InvalidRect;
//                }
//
//                if (!IsGroupValid) {
//                    return InvalidRect;
//                }
//
//                var calculatedWidth = width > 0f ? width : _flexibleElementWidth;
//                var entryRect = GetActualRect(0, 0, height, calculatedWidth);
//                NextEntryPosition.x += calculatedWidth + _horizontalContentOffset;
//                return entryRect;
//            }
//
//            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
//                if (elementWidth > 0f) {
//                    _fixedEntriesCount += count;
//                    _totalFixedEntriesWidth += elementHeight * count;
//                }
//                EntriesCount += count;
//                TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, elementHeight);
//            }
//        }
//
//        public static bool BeginRestrictedHorizontalGroup(bool discardMarginAndPadding, float width, GUIStyle style) {
//            var eventType = Event.current.type;
//            LayoutGroupBase layoutGroup;
//            if (eventType == EventType.Layout) {
//                layoutGroup = new FlexibleHorizontalLayoutGroup(discardMarginAndPadding, width, style);
//                SubscribedForLayout.Enqueue(layoutGroup);
//            }
//            else {
//                layoutGroup = SubscribedForLayout.Dequeue();
//                layoutGroup.RetrieveLayoutData(eventType);
//            }
//            
//            _topGroup = layoutGroup;
//
//            return layoutGroup.IsGroupValid;
//        }
//        public static bool BeginRestrictedHorizontalGroup(float width, bool discardMarginAndPadding = false) {
//            return BeginRestrictedHorizontalGroup(discardMarginAndPadding, width, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalRestrictedGroup);
//        }
//
//        public static void EndRestrictedHorizontalGroup() {
//            var lastGroup = EndLayoutGroup() as FlexibleHorizontalLayoutGroup;
//            if (lastGroup == null) {
//                throw new Exception("Layout group type mismatch");
//            }
//        }
//    }
//}