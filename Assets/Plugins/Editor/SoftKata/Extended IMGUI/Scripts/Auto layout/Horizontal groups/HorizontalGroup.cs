using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public class HorizontalGroup : LayoutGroup {
        protected int FixedWidthEntriesCount;
        
        public HorizontalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.x;
        }
        public HorizontalGroup(bool ignoreConstaints = false)
            : this(ExtendedEditorGUI.LayoutResources.HorizontalGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            ContentRect.height += TotalOffset.vertical;
            ContentRect.width += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override bool PrepareNextRect(float width, float height) {
            if (IsLayoutEvent) {
                EntriesCount++;
                ContentRect.width += width;
                ContentRect.height = Mathf.Max(ContentRect.height, height);

                return false;
            }

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


        // experimental APi
        protected override void _RegisterEntry(float width, float height) {
            ContentRect.width += width;
            ContentRect.height = Mathf.Max(ContentRect.height, height);
        }
        protected override bool _EntryQueryCallback(Vector2 entrySize) {
            var currentEntryPositionX = NextEntryPosition.x;
            NextEntryPosition.x += entrySize.x + SpaceBetweenEntries;

            // occlusion
            return currentEntryPositionX + entrySize.x >= ContainerRect.x
                    && currentEntryPositionX <= ContainerRect.xMax;
        }
    }
}