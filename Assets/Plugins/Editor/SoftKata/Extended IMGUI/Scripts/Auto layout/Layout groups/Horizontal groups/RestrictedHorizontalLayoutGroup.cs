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
                if (IsLayout) {
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
                return CurrentEntryPosition.x + width >= VisibleAreaRect.x 
                       && CurrentEntryPosition.x <= VisibleAreaRect.xMax;
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
        
        public class FlexibleHorizontalLayoutGroupScope : IDisposable {
            public readonly bool Valid;
            
            public FlexibleHorizontalLayoutGroupScope(float width, GroupModifier modifier, GUIStyle style) {
                Valid = BeginRestrictedHorizontalGroup(width, modifier, style);
            }
            public FlexibleHorizontalLayoutGroupScope(float width, GroupModifier modifier = GroupModifier.None) {
                Valid = BeginRestrictedHorizontalGroup(width, modifier);
            }
            
            public void Dispose() {
                EndLayoutGroup<FlexibleHorizontalLayoutGroup>();
            }
        }

        public static bool BeginRestrictedHorizontalGroup(float width, GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterGroup(new FlexibleHorizontalLayoutGroup(width, modifier, style));
            }

            return GatherGroup().IsGroupValid;
        }
        public static bool BeginRestrictedHorizontalGroup(float width, GroupModifier modifier = GroupModifier.None) {
            return BeginRestrictedHorizontalGroup(width, modifier, ExtendedEditorGUI.Resources.LayoutGroups.HorizontalRestrictedGroup);
        }

        public static void EndRestrictedHorizontalGroup() {
            EndLayoutGroup<FlexibleHorizontalLayoutGroup>();
        }
    }
}