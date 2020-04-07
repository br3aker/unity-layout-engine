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
        
        protected sealed override bool PrepareNextRect(float width, float height) {
            if (IsLayoutEvent) {
                EntriesCount++;
                ContentRect.width = Mathf.Max(ContentRect.width, width);
                ContentRect.height += height;

                return false;
            }

            var currentEntryPositionY = NextEntryPosition.y;
            NextEntryPosition.y += height + SpaceBetweenEntries;
            
            // occlusion
            return currentEntryPositionY + height >= ContainerRect.y
                    && currentEntryPositionY <= ContainerRect.yMax;
        }

        public override void RegisterArray(float elemWidth, float elemHeight, int count) {
            EntriesCount += count;
            ContentRect.width = Mathf.Max(ContentRect.width, elemWidth);
            ContentRect.height += elemHeight * count;
        }


        // experimental APi
        protected override void _RegisterEntry(float width, float height) {
            ContentRect.width = Mathf.Max(ContentRect.width, width);
            ContentRect.height += height;
        }
        protected override bool _EntryQueryCallback(Vector2 entrySize) {
            var currentEntryPositionY = NextEntryPosition.y;
            NextEntryPosition.y += entrySize.y + SpaceBetweenEntries;
            
            // occlusion
            return currentEntryPositionY + entrySize.y >= ContainerRect.y
                    && currentEntryPositionY <= ContainerRect.yMax;
        }
    }
}