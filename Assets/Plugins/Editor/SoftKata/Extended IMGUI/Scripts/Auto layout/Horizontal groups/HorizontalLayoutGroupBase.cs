using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginHorizontalGroup(LayoutGroupBase group){
            return BeginLayoutGroup(group);
        }
        public static bool BeginHorizontalGroup(Constraints modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) return RegisterForLayout(new HorizontalGroup(modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginHorizontalGroup(Constraints modifier = Constraints.None) {
            return BeginHorizontalGroup(modifier, ExtendedEditorGUI.LayoutResources.HorizontalGroup);
        }
       
        public static void EndHorizontalGroup() {
            EndLayoutGroup<HorizontalGroup>();
        }

        internal class HorizontalGroup : LayoutGroupBase {
            protected int FixedWidthEntriesCount;
            
            public HorizontalGroup(Constraints modifier, GUIStyle style) : base(modifier, style) {
                SpaceBetweenEntries = style.contentOffset.x;
            }

            protected override void ModifyContainerSize() {
                // vertical "service" height addition: margin/border/padding + space between entries
                RequestedHeight += ConstraintsHeight;

                // fixed width is already calculated, adding automatic-width entries width sum
                RequestedWidth += (EntriesCount - FixedWidthEntriesCount) * _visibleContentWidth + SpaceBetweenEntries * (EntriesCount - 1) + ConstraintsWidth;
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


                NextEntryPosition.x += width + SpaceBetweenEntries;

                // occlusion
                return CurrentEntryPosition.x + width >= VisibleAreaRect.x
                       && CurrentEntryPosition.x <= VisibleAreaRect.xMax;
            }
            
            public override void RegisterArray(float elemWidth, float elemHeight, int count) {
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

            public HorizontalScope(Constraints modifier, GUIStyle style) {
                Valid = BeginHorizontalGroup(modifier, style);
            }

            public HorizontalScope(Constraints modifier = Constraints.None) {
                Valid = BeginHorizontalGroup(modifier);
            }

            public void Dispose() {
                EndLayoutGroup<HorizontalGroup>();
            }
        }
    }
}