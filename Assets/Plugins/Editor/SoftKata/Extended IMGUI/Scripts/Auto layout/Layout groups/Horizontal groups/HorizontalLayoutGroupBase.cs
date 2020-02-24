using System;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginHorizontalGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) return RegisterForLayout(new HorizontalGroup(modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginHorizontalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginHorizontalGroup(modifier, ExtendedEditorGUI.Resources.LayoutGroups.HorizontalGroup);
        }
        public static void EndHorizontalGroup() {
            EndLayoutGroup<HorizontalGroup>();
        }

        internal class HorizontalGroup : LayoutGroupBase {
            public HorizontalGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) { }

            protected override bool PrepareNextRect(float width, float height) {
                if (IsLayout) {
                    EntriesCount++;
                    if (width > 0f) RequestedWidth += width;
                    RequestedHeight = Mathf.Max(RequestedHeight, height);
                    return false;
                }

                if (!IsGroupValid) return false;


                NextEntryPosition.x += width + ContentOffset.x;

                // occlusion
                return CurrentEntryPosition.x + width >= VisibleAreaRect.x
                       && CurrentEntryPosition.x <= VisibleAreaRect.xMax;
            }

            internal override void RegisterArray(float elemWidth, float elemHeight, int count) {
                EntriesCount += count;
                RequestedWidth += elemWidth * count;
                RequestedHeight = Mathf.Max(RequestedHeight, elemHeight);
            }
        }

        public class HorizontalScope : IDisposable {
            public readonly bool Valid;

            public HorizontalScope(GroupModifier modifier, GUIStyle style) {
                Valid = BeginHorizontalGroup(modifier, style);
            }

            public HorizontalScope(GroupModifier modifier = GroupModifier.None) {
                Valid = BeginHorizontalGroup(modifier);
            }

            public void Dispose() {
                EndLayoutGroup<HorizontalGroup>();
            }
        }
    }
}