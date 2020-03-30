using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class FlexibleHorizontalGroup : HorizontalGroup {
        private int _fixedEntriesCount;
        private float _fixedEntriesWidth;

        private float _containerWidth;
        private float _fixedWidth;

        public FlexibleHorizontalGroup(float width, Constraints modifier, GUIStyle style) : base(modifier, style) {
            _containerWidth = width;
        }
        public FlexibleHorizontalGroup(float width, Constraints modifier)
            : this(width, modifier, ExtendedEditorGUI.LayoutResources.HorizontalRestrictedGroup) {}

        protected override void PreLayoutRequest() {
            // vertical "service" height addition: margin/border/padding
            TotalHeight += TotalOffset.vertical;

            _fixedWidth = TotalWidth;
            TotalWidth = _containerWidth;
        }
    
        internal override void BeginNonLayout() {
            RetrieveLayoutData();
            var totalFlexibleWidth = TotalWidth - _fixedWidth - SpaceBetweenEntries * (EntriesCount - 1);
            // AutomaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - FixedWidthEntriesCount), 0f);
        }
    }
}