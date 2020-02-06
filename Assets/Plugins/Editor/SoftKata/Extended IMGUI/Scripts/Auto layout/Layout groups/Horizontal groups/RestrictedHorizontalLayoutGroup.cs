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

            
            public FlexibleHorizontalLayoutGroup(float width, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
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

        public static bool BeginRestrictedHorizontalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginRestrictedHorizontalGroup(EditorGUIUtility.currentViewWidth, modifier);
        }
        
        public static bool BeginRestrictedHorizontalGroup(float width, GroupModifier modifier, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new FlexibleHorizontalLayoutGroup(width, modifier, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginRestrictedHorizontalGroup(float width, GroupModifier modifier = GroupModifier.None) {
            return BeginRestrictedHorizontalGroup(width, modifier, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalRestrictedGroup);
        }

        public static void EndRestrictedHorizontalGroup() {
            EndLayoutGroup<FlexibleHorizontalLayoutGroup>();
        }
    }
}