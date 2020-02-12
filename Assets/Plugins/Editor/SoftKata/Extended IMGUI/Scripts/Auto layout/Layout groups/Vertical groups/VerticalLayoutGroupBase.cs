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
                if (CurrentEventType == EventType.Layout) {
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
                if (CurrentEntryPosition.y + height < VisibleAreaRect.y || CurrentEntryPosition.y > VisibleAreaRect.yMax) {
                    return false;
                }
                
                return true;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalRequestedWidth = Mathf.Max(TotalRequestedWidth, elementWidth);
                TotalRequestedHeight += elementHeight * count;
            }
        }

        public static bool BeginVerticalGroup(GroupModifier modifier, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalGroup(modifier, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalGroup(modifier, ExtendedEditorGUI.Resources.LayoutGroups.VerticalGroup);
        }

        public static void EndVerticalGroup() {
            EndLayoutGroup<VerticalGroup>();
        }
    }
}