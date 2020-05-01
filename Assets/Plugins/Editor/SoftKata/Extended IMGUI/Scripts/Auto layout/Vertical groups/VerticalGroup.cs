using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public class VerticalGroup : LayoutGroup {
        public VerticalGroup(GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            SpaceBetweenEntries = style.contentOffset.y;
        }
        public VerticalGroup(bool ignoreConstaints = false) 
            : this(Layout.Resources.VerticalGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            if(ContentRectInternal.width < 0) {
                ContentRectInternal.width = AutomaticWidth;
            }
            else {
                ContentRectInternal.width += TotalOffset.horizontal;
            }
            ContentRectInternal.height += TotalOffset.vertical + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            ContentRectInternal.width = Mathf.Max(ContentRectInternal.width, width);
            ContentRectInternal.height += height;
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            EntriesCount += count;
            ContentRectInternal.width = Mathf.Max(ContentRectInternal.width, elemWidth);
            ContentRectInternal.height += elemHeight * count;
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