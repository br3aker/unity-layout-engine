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
            : this(ExtendedEditorGUI.LayoutResources.VerticalGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            if(ContentRect.width < 0) {
                ContentRect.width = AutomaticWidth;
            }
            else {
                ContentRect.width += TotalOffset.horizontal;
            }
            ContentRect.height += TotalOffset.vertical + SpaceBetweenEntries * (EntriesCount - 1);
        }

        protected override void RegisterEntry(float width, float height) {
            ContentRect.width = Mathf.Max(ContentRect.width, width);
            ContentRect.height += height;
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            EntriesCount += count;
            ContentRect.width = Mathf.Max(ContentRect.width, elemWidth);
            ContentRect.height += elemHeight * count;
        }

        protected override bool QueryAndOcclude(Vector2 entrySize) {
            var currentEntryPositionY = NextEntryPosition.y;
            NextEntryPosition.y += entrySize.y + SpaceBetweenEntries;
            
            // occlusion
            return currentEntryPositionY + entrySize.y >= ContainerRect.y
                    && currentEntryPositionY <= ContainerRect.yMax;
        }
    }
}