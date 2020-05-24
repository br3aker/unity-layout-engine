using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    public class VerticalGroup : LayoutGroup {
        public VerticalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.y;
        }
        public VerticalGroup(bool ignoreConstaints = false) 
            : this(ExtendedEditor.Resources.VerticalGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            if(EntriesRequestedSize.x < 0) {
                EntriesRequestedSize.x = AutomaticWidth;
            }
            else {
                EntriesRequestedSize.x += TotalOffset.horizontal;
            }
            EntriesRequestedSize.y += TotalOffset.vertical + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            EntriesRequestedSize.x = Mathf.Max(EntriesRequestedSize.x, width);
            EntriesRequestedSize.y += height;
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            EntriesCount += count;
            EntriesRequestedSize.x = Mathf.Max(EntriesRequestedSize.x, elemWidth);
            EntriesRequestedSize.y += elemHeight * count;
        }

        protected override bool QueryAndOcclude(Vector2 entrySize) {
            var currentEntryPositionY = NextEntryPosition.y;
            NextEntryPosition.y += entrySize.y + SpaceBetweenEntries;
            
            // occlusion
            return currentEntryPositionY + entrySize.y >= ContainerRectInternal.y
                    && currentEntryPositionY <= ContainerRectInternal.yMax;
        }
    }
}