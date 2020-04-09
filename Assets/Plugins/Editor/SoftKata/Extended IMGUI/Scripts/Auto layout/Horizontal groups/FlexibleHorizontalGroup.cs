using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class FlexibleHorizontalGroup : HorizontalGroup {
        private int _fixedEntriesCount;
        private float _fixedEntriesWidth;

        private float _containerWidth;
        private float _fixedWidth;

        protected override float GetAutomaticWidth() {
            // This is a hack, _fixedEntriesCount must be reset
            // But we don't have access to reset mechanism of base class
            _fixedEntriesCount = 0;
            return -1;
        }

        public FlexibleHorizontalGroup(float width, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _containerWidth = width;
        }
        public FlexibleHorizontalGroup(float width, bool ignoreConstaints = false)
            : this(width, ExtendedEditorGUI.LayoutResources.HorizontalRestrictedGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            // vertical "service" height addition: margin/border/padding
            ContentRect.height += TotalOffset.vertical;

            _fixedWidth = ContentRect.width;
            ContentRect.width = _containerWidth > 0 ? (_containerWidth + TotalOffset.horizontal) : AvailableWidth;
        }
    
        internal override void BeginNonLayout() {
            RetrieveLayoutData();
            var totalFlexibleWidth = ContentRect.width - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1);
            AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - _fixedEntriesCount), 0f);
        }

        public override void RegisterArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0) {
                ContentRect.width += elemWidth * count;
                ++_fixedEntriesCount;
            }
            EntriesCount += count;
            ContentRect.height = Mathf.Max(ContentRect.height, elemHeight);
        }

        // Entry registration and querying
        protected override void RegisterEntry(float width, float height) {
            if(width > 0) {
                ContentRect.width += width;
                ++_fixedEntriesCount;
            }
            ContentRect.height = Mathf.Max(ContentRect.height, height);
        }
    }
}