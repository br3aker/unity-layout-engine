using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public class HorizontalGroup : LayoutGroup {
        public HorizontalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.x;
        }
        public HorizontalGroup(bool ignoreConstaints = false)
            : this(Layout.Resources.HorizontalGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            ContentRectInternal.height += TotalOffset.vertical;
            ContentRectInternal.width += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            ContentRectInternal.width += width;
            ContentRectInternal.height = Mathf.Max(ContentRectInternal.height, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0f) {
                ContentRectInternal.width += elemWidth * count;
            }
            EntriesCount += count; ;
            ContentRectInternal.height = Mathf.Max(ContentRectInternal.height, elemHeight);
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