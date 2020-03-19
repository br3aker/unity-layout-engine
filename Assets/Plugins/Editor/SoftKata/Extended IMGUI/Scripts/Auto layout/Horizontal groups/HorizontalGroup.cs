using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public class HorizontalGroup : LayoutGroup {
        protected int FixedWidthEntriesCount;
        
        public HorizontalGroup(Constraints modifier, GUIStyle style) : base(modifier, style) {
            SpaceBetweenEntries = style.contentOffset.x;
        }
        public HorizontalGroup(Constraints modifier = Constraints.None)
            : this(modifier, ExtendedEditorGUI.LayoutResources.HorizontalGroup) {}

        protected override void PreLayoutRequest() {
            // vertical "service" height addition: margin/border/padding + space between entries
            TotalHeight += TotalOffset.vertical;
            TotalWidth += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1) + (EntriesCount - FixedWidthEntriesCount) * AutomaticWidth;
        }

        protected override bool PrepareNextRect(float width, float height) {
            if (IsLayoutEvent) {
                if (width > 0f) {
                    TotalWidth += width;
                    // TODO: don't count fixed entries - use _visibleContentWidth instead
                    FixedWidthEntriesCount++;
                }
                EntriesCount++;
                TotalHeight = Mathf.Max(TotalHeight, height);
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
                TotalWidth += elemWidth * count;
                FixedWidthEntriesCount += count;
            }
            EntriesCount += count; ;
            TotalHeight = Mathf.Max(TotalHeight, elemHeight);
        }
    }
}