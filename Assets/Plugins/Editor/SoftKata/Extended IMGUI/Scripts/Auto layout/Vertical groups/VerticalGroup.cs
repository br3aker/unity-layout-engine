using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public class VerticalGroup : LayoutGroup {
        public VerticalGroup(Constraints modifier, GUIStyle style) : base(modifier, style) {
            SpaceBetweenEntries = style.contentOffset.y;
        }

        public VerticalGroup(Constraints modifier = Constraints.None) 
            : this(modifier, ExtendedEditorGUI.LayoutResources.VerticalGroup) {}

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

            if (!IsGroupValid) return false;

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
    }
}