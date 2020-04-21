using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class FlexibleHorizontalGroup : HorizontalGroup {
        private int _fixedEntriesCount;

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
        public FlexibleHorizontalGroup(float width = -1, bool ignoreConstaints = false)
            : this(width, StyleResources.HorizontalRestrictedGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            // vertical "service" height addition: margin/border/padding
            ContentRectInternal.height += TotalOffset.vertical;

            _fixedWidth = ContentRectInternal.width;
            ContentRectInternal.width = _containerWidth > 0 ? (_containerWidth + TotalOffset.horizontal) : AvailableWidth;
        }
    
        internal override bool BeginNonLayout() {
            if(base.BeginNonLayout()) {
                var totalFlexibleWidth = ContentRectInternal.width - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1);
                AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - _fixedEntriesCount), 0f);
                return true;
            }
            return false;
        }

        // Entry registration and querying
        protected override void RegisterEntry(float width, float height) {
            if(width > 0) {
                ContentRectInternal.width += width;
                ++_fixedEntriesCount;
            }
            ContentRectInternal.height = Mathf.Max(ContentRectInternal.height, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0) {
                ContentRectInternal.width += elemWidth * count;
                ++_fixedEntriesCount;
            }
            EntriesCount += count;
            ContentRectInternal.height = Mathf.Max(ContentRectInternal.height, elemHeight);
        }
    }
}