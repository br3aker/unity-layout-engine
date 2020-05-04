using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class FlexibleHorizontalGroup : HorizontalGroup {
        public const float FullScreenWidth = -1;

        private int _fixedEntriesCount;

        private float _containerWidth;
        private float _fixedWidth;

        public float Width {
            get => _containerWidth;
            set => _containerWidth = value;
        }

        protected override float GetAutomaticWidth() {
            // This is a hack, _fixedEntriesCount must be reset
            // But we don't have access to reset mechanism of base class
            _fixedEntriesCount = 0;
            return -1;
        }

        public FlexibleHorizontalGroup(float width, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _containerWidth = width;
        }
        public FlexibleHorizontalGroup(float width = FullScreenWidth, bool ignoreConstaints = false)
            : this(width, ExtendedEditorGUI.Resources.HorizontalRestrictedGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            EntriesRequestedSize.y += TotalOffset.vertical;

            _fixedWidth = EntriesRequestedSize.x;
            
            EntriesRequestedSize.x = _containerWidth > 0 ? (_containerWidth + TotalOffset.horizontal) : AvailableWidth;
        }
    
        internal override bool BeginNonLayout() {
            if(base.BeginNonLayout()) {
                var totalFlexibleWidth = ContentRectInternal.width - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1);
                AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - _fixedEntriesCount), 0f);
                // Debug.Log($"ContentRect: {ContentRectInternal}");
                return true;
            }
            return false;
        }

        // Entry registration and querying
        protected override void RegisterEntry(float width, float height) {
            if(width > 0) {
                EntriesRequestedSize.x += width;
                ++_fixedEntriesCount;
            }
            EntriesRequestedSize.y = Mathf.Max(EntriesRequestedSize.y, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0) {
                EntriesRequestedSize.x += elemWidth * count;
                ++_fixedEntriesCount;
            }
            EntriesCount += count;
            EntriesRequestedSize.y = Mathf.Max(EntriesRequestedSize.y, elemHeight);
        }
    }
}