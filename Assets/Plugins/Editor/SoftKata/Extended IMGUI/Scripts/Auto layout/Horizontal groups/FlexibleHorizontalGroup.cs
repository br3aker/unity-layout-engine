using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class FlexibleHorizontalGroup : HorizontalGroup {
        private int _fixedEntriesCount;
        private float _fixedEntriesWidth;

        private float _containerWidth;
        private float _fixedWidth;

        public FlexibleHorizontalGroup(float width, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _containerWidth = width;
        }
        public FlexibleHorizontalGroup(float width, bool ignoreConstaints = false)
            : this(width, ExtendedEditorGUI.LayoutResources.HorizontalRestrictedGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            // vertical "service" height addition: margin/border/padding
            ContentRect.height += TotalOffset.vertical;

            _fixedWidth = ContentRect.width;
            ContentRect.width = _containerWidth;
        }
    
        internal override void BeginNonLayout() {
            RetrieveLayoutData();
            var totalFlexibleWidth = ContentRect.width - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1);
            // AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - FixedWidthEntriesCount), 0f);
        }
    }
}