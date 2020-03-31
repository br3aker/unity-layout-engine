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
            if(TotalWidth < 0) {
                TotalWidth = AutomaticWidth;
            }
            else {
                TotalWidth += TotalOffset.horizontal;
            }
            TotalHeight += TotalOffset.vertical + SpaceBetweenEntries * (EntriesCount - 1);
        }
        
        protected sealed override bool PrepareNextRect(float width, float height) {
            if (IsLayoutEvent) {
                EntriesCount++;
                TotalWidth = Mathf.Max(TotalWidth, width);
                TotalHeight += height;

                return false;
            }

            if (!IsGroupValid) return false;

            NextEntryPosition.y += height + SpaceBetweenEntries;
            
            // occlusion
            return CurrentEntryPosition.y + height >= _ContainerRect.y
                    && CurrentEntryPosition.y <= _ContainerRect.yMax;
        }

        public override void RegisterArray(float elemWidth, float elemHeight, int count) {
            EntriesCount += count;
            TotalWidth = Mathf.Max(TotalWidth, elemWidth);
            TotalHeight += elemHeight * count;
        }
    }
}