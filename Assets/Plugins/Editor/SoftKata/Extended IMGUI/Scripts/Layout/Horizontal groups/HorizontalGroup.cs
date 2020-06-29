using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    public class HorizontalGroup : LayoutGroup {
        public HorizontalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.x;
        }
        public HorizontalGroup(bool ignoreConstaints = false)
            : this(Resources.DefaultHorizontalStyle, ignoreConstaints) {}
        protected override void PreLayoutRequest() {
            RequestedSize.y += ContentOffset.vertical;
            RequestedSize.x += ContentOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            RequestedSize.x += width;
            RequestedSize.y = Mathf.Max(RequestedSize.y, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0f) {
                RequestedSize.x += elemWidth * count;
            }
            EntriesCount += count; ;
            RequestedSize.y = Mathf.Max(RequestedSize.y, elemHeight);
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