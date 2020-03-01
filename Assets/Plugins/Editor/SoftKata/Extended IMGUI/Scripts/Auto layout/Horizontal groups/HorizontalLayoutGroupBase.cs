using System;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginHorizontalGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) return RegisterForLayout(new HorizontalGroup(modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginHorizontalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginHorizontalGroup(modifier, ExtendedEditorGUI.LayoutResources.HorizontalGroup);
        }
        public static void EndHorizontalGroup() {
            EndLayoutGroup<HorizontalGroup>();
        }

        internal class HorizontalGroup : LayoutGroupBase {
            protected int FixedWidthEntriesCount;
            
            public HorizontalGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) { }

            protected override void CalculateFinalContentSize() {
                // vertical "service" height addition: margin/border/padding + space between entries
                RequestedHeight += ConstraintsHeight;
                
                // fixed width is already calculated, adding automatic-width entries width sum
                RequestedWidth += (EntriesCount - FixedWidthEntriesCount) * _visibleContentWidth + ContentOffset.x * (EntriesCount - 1) + ConstraintsWidth;
            }

            protected override bool PrepareNextRect(float width, float height) {
                if (IsLayout) {
                    if (width > 0f) {
                        RequestedWidth += width;
                        FixedWidthEntriesCount++;
                    }
                    EntriesCount++;
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
                if (elemWidth > 0f) {
                    RequestedWidth += elemWidth * count;
                    FixedWidthEntriesCount += count;
                }
                EntriesCount += count; ;
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