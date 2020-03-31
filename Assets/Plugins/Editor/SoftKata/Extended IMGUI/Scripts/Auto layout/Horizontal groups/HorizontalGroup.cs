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
            TotalHeight += TotalOffset.vertical;
            TotalWidth += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override bool PrepareNextRect(float width, float height) {
            if (IsLayoutEvent) {
                TotalWidth += width;
                EntriesCount++;
                TotalHeight = Mathf.Max(TotalHeight, height);
                return false;
            }

            if (!IsGroupValid) return false;

            var currentEntryPositionX = NextEntryPosition.x;
            NextEntryPosition.x += width + SpaceBetweenEntries;

            // occlusion
            return currentEntryPositionX + width >= ContainerRect.x
                    && currentEntryPositionX <= ContainerRect.xMax;
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