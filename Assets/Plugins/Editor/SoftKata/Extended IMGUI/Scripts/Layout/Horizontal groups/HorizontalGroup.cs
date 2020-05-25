using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    public class HorizontalGroup : LayoutGroup {
        public HorizontalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.x;
        }
        public HorizontalGroup(bool ignoreConstaints = false)
            : this(ExtendedEditor.Resources.HorizontalGroup, ignoreConstaints) {}
        protected override void PreLayoutRequest() {
            EntriesRequestedSize.y += TotalOffset.vertical;
            EntriesRequestedSize.x += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            EntriesRequestedSize.x += width;
            EntriesRequestedSize.y = Mathf.Max(EntriesRequestedSize.y, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0f) {
                EntriesRequestedSize.x += elemWidth * count;
            }
            EntriesCount += count; ;
            EntriesRequestedSize.y = Mathf.Max(EntriesRequestedSize.y, elemHeight);
        }
        
        protected override bool QueryAndOcclude(Vector2 entrySize) {
            var currentEntryPositionX = NextEntryPosition.x;
            NextEntryPosition.x += entrySize.x + SpaceBetweenEntries;

            // occlusion
            return currentEntryPositionX + entrySize.x >= ContainerRectInternal.x
                    && currentEntryPositionX <= ContainerRectInternal.xMax;
        }
    }
}