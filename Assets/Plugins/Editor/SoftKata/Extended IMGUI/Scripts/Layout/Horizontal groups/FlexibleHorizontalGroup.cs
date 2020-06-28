using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {
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
            : this(width, ExtendedEditor.Resources.DefaultHorizontalStyle, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            RequestedSize.y += TotalOffset.vertical;

            _fixedWidth = RequestedSize.x;
            
            RequestedSize.x = _containerWidth > 0 ? _containerWidth : AvailableWidth;

            var totalFlexibleWidth = RequestedSize.x - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1) - TotalOffset.horizontal;
            AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - _fixedEntriesCount), 0f);

            // Debug.Log(_fixedWidth);
        }

        // Entry registration and querying
        protected override void RegisterEntry(float width, float height) {
            if(width > 0) {
                RequestedSize.x += width;
                ++_fixedEntriesCount;
            }
            RequestedSize.y = Mathf.Max(RequestedSize.y, height);
        }
        public override void RegisterEntriesArray(float elemWidth, float elemHeight, int count) {
            if (elemWidth > 0) {
                RequestedSize.x += elemWidth * count;
                ++_fixedEntriesCount;
            }
            EntriesCount += count;
            RequestedSize.y = Mathf.Max(RequestedSize.y, elemHeight);
        }
    }
}