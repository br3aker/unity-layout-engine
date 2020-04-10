using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public class HorizontalGroup : LayoutGroup {
        public HorizontalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.x;
        }
        public HorizontalGroup(bool ignoreConstaints = false)
            : this(ExtendedEditorGUI.LayoutResources.HorizontalGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            ContentRect.height += TotalOffset.vertical;
            ContentRect.width += TotalOffset.horizontal + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            ContentRect.width += width;
            ContentRect.height = Mathf.Max(ContentRect.height, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0f) {
                ContentRect.width += elemWidth * count;
            }
            EntriesCount += count; ;
            ContentRect.height = Mathf.Max(ContentRect.height, elemHeight);
        }
        
        protected override bool QueryAndOcclude(Vector2 entrySize) {
            var currentEntryPositionX = NextEntryPosition.x;
            NextEntryPosition.x += entrySize.x + SpaceBetweenEntries;

            // occlusion
            return currentEntryPositionX + entrySize.x >= ContainerRect.x
                    && currentEntryPositionX <= ContainerRect.xMax;
        }
    }
}