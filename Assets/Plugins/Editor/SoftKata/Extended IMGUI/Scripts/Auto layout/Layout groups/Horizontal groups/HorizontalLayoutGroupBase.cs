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

        public class HorizontalGroupScope : IDisposable {
            public readonly bool Valid;
            
            public HorizontalGroupScope(GroupModifier modifier, GUIStyle style) {
                Valid = BeginHorizontalGroup(modifier, style);
            }
            public HorizontalGroupScope(GroupModifier modifier = GroupModifier.None) {
                Valid = BeginHorizontalGroup(modifier);
            }
            
            public void Dispose() {
                EndLayoutGroup<HorizontalGroup>();
            }
        }
        
        public static bool BeginHorizontalGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterGroup(new HorizontalGroup(modifier, style));
            }

            return GatherGroup().IsGroupValid;
        }
        public static bool BeginHorizontalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginHorizontalGroup(modifier, ExtendedEditorGUI.Resources.LayoutGroups.HorizontalGroup);
        }

        public static void EndHorizontalGroup() {
            EndLayoutGroup<HorizontalGroup>();
        }
    }
}