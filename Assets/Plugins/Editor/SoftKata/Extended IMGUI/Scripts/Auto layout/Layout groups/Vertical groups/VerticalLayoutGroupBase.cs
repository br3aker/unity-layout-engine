using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalGroup : LayoutGroupBase {
            public VerticalGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                TotalRequestedWidth = float.MinValue;
            }

            protected sealed override bool RegisterNewEntry(float height, float width) {
                if (IsLayout) {
                    EntriesCount++;
                    TotalRequestedWidth = Mathf.Max(TotalRequestedWidth, width);
                    TotalRequestedHeight += height;
                    return false;
                }

                if (!IsGroupValid) {
                    return false;
                }

                NextEntryPosition.y += height + ContentOffset.y;

                // occlusion
                return CurrentEntryPosition.y + height >= VisibleAreaRect.y 
                       && CurrentEntryPosition.y <= VisibleAreaRect.yMax;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth = Mathf.Max(TotalRequestedWidth, elementWidth);
                TotalRequestedHeight += elementHeight * count;
            }
        }

        public static bool BeginVerticalGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterGroup(new VerticalGroup(modifier, style));
            }

            return GatherGroup().IsGroupValid;
        }
        public static bool BeginVerticalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalGroup(modifier, ExtendedEditorGUI.Resources.LayoutGroups.VerticalGroup);
        }

        public static void EndVerticalGroup() {
            EndLayoutGroup<VerticalGroup>();
        }
    }
}