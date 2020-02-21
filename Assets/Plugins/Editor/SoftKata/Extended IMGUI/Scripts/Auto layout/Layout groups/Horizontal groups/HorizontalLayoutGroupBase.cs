using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalGroup : LayoutGroupBase {
            public HorizontalGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) {}

            protected override bool RegisterNewEntry(float height, float width) {
                if (IsLayout) {
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
                return CurrentEntryPosition.x + width >= VisibleAreaRect.x 
                       && CurrentEntryPosition.x <= VisibleAreaRect.xMax;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth += elementWidth * count;
                TotalRequestedHeight = Mathf.Max(TotalRequestedHeight, elementHeight);
            }
        }

        public static bool BeginHorizontalGroup(GroupModifier modifier, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalGroup(modifier, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData();
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHorizontalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginHorizontalGroup(modifier, ExtendedEditorGUI.Resources.LayoutGroups.HorizontalGroup);
        }

        public static void EndHorizontalGroup() {
            EndLayoutGroup<HorizontalGroup>();
        }
    }
}