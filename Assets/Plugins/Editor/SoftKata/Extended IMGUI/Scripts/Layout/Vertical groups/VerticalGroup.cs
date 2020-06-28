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
            : this(ExtendedEditor.Resources.DefaultVerticalStyle, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            if(RequestedSize.x < 0) {
                RequestedSize.x = AutomaticWidth;
            }
            else {
                RequestedSize.x += TotalOffset.horizontal;
            }
            RequestedSize.y += TotalOffset.vertical + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            RequestedSize.x = Mathf.Max(RequestedSize.x, width);
            RequestedSize.y += height;
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            EntriesCount += count;
            RequestedSize.x = Mathf.Max(RequestedSize.x, elemWidth);
            RequestedSize.y += elemHeight * count;
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