using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class FlexibleHorizontalLayoutGroup : HorizontalGroup {
            private float _containerWidth;
            
            private int _fixedEntriesCount;
            
            private float _totalFixedEntriesWidth;
            
            private float _flexibleElementWidth = -1f;

            
            public FlexibleHorizontalLayoutGroup(bool discardMargin, float width, GUIStyle style) : base(discardMargin, style) {
                _containerWidth = width;
            }

            protected override void PreLayoutRequest() {
                // Calculation flexible elements width
                float totalFlexibleElementsWidth = _containerWidth - TotalRequestedWidth;

                TotalRequestedWidth += totalFlexibleElementsWidth;
                
                _flexibleElementWidth = Mathf.Max(totalFlexibleElementsWidth / (EntriesCount - _fixedEntriesCount), 0f);
                
                AutomaticEntryWidth = _flexibleElementWidth;
            }

            protected override bool RegisterNewEntry(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    if (width > 0) {
                        _fixedEntriesCount++;
                        TotalRequestedWidth += width;
                    }

                    EntriesCount++;
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
                if (elementWidth > 0f) {
                    _fixedEntriesCount += count;
                    _totalFixedEntriesWidth += elementHeight * count;
                }
                EntriesCount += count;
                TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, elementHeight);
            }
        }

        public static bool BeginRestrictedHorizontalGroup(bool discardMarginAndPadding = false) {
            return BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth, discardMarginAndPadding);
        }
        
        public static bool BeginRestrictedHorizontalGroup(float width, bool discardMarginAndPadding, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new FlexibleHorizontalLayoutGroup(discardMarginAndPadding, width, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginRestrictedHorizontalGroup(float width, bool discardMarginAndPadding = false) {
            return BeginRestrictedHorizontalGroup(width, discardMarginAndPadding, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalRestrictedGroup);
        }

        public static void EndRestrictedHorizontalGroup() {
            EndLayoutGroup<FlexibleHorizontalLayoutGroup>();
        }
    }
}