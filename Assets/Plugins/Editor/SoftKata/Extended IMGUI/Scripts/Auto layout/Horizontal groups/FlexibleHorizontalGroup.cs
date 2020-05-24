using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.Editor {
    public class FlexibleHorizontalGroup : HorizontalGroup {
        public const float FullScreenWidth = -1;

        private int _fixedEntriesCount;

        private float _containerWidth;
        private float _fixedWidth;

        public float Width {
            get => _containerWidth;
            set {
                if(_containerWidth != value) {
                    MarkLayoutDirty();
                    _containerWidth = value;
                }
            }
        }

        protected override float CalculateAutomaticContentWidth() {
            _fixedEntriesCount = 0;
            return 0;
        }

        public FlexibleHorizontalGroup(float width, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _containerWidth = width;
        }
        public FlexibleHorizontalGroup(float width = FullScreenWidth, bool ignoreConstaints = false)
            : this(width, ExtendedEditor.Resources.HorizontalRestrictedGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            EntriesRequestedSize.y += TotalOffset.vertical;

            _fixedWidth = EntriesRequestedSize.x;
            
            EntriesRequestedSize.x = _containerWidth > 0 ? _containerWidth : AvailableWidth;

            var totalFlexibleWidth = EntriesRequestedSize.x - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1) - TotalOffset.horizontal;
            AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - _fixedEntriesCount), 0f);

            // Debug.Log(_fixedWidth);
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