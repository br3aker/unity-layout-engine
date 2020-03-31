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
            ContentRect.height += TotalOffset.vertical;
            ContentRect.width += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override bool PrepareNextRect(float width, float height) {
            if (IsLayoutEvent) {
                ContentRect.width += width;
                EntriesCount++;
                ContentRect.height = Mathf.Max(ContentRect.height, height);
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
                ContentRect.width += elemWidth * count;
                FixedWidthEntriesCount += count;
            }
            EntriesCount += count; ;
            ContentRect.height = Mathf.Max(ContentRect.height, elemHeight);
        }
    }
}