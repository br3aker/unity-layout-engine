using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginFlexibleHorizontalGroup(float width, GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout)
                return RegisterForLayout(new FlexibleHorizontalGroup(width, modifier, style));

            var layoutGroup = RetrieveNextGroup<FlexibleHorizontalGroup>();
            layoutGroup.CalculateLayout();
            return layoutGroup.IsGroupValid;
        }
        public static bool BeginFlexibleHorizontalGroup(float width, GroupModifier modifier = GroupModifier.None) {
            return BeginFlexibleHorizontalGroup(width, modifier,
                ExtendedEditorGUI.LayoutResources.HorizontalRestrictedGroup);
        }
        public static void EndFlexibleHorizontalGroup() {
            EndLayoutGroup<FlexibleHorizontalGroup>();
        }

        private class FlexibleHorizontalGroup : HorizontalGroup {
            private int _fixedEntriesCount;
            private float _fixedEntriesWidth;

            private float _containerWidth;
            private float _fixedWidth;

            public FlexibleHorizontalGroup(float width, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                _containerWidth = width;
            }

            protected override void PreLayoutRequest() {
                _fixedWidth = RequestedWidth;
                RequestedWidth = _containerWidth;
            }
            
            internal void CalculateLayout() {
                var totalFlexibleWidth = ContainerRect.width - _fixedWidth;
                _automaticWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - FixedWidthEntriesCount), 0f);
            }
        }

        public class FlexibleHorizontalScope : IDisposable {
            public readonly bool Valid;

            public FlexibleHorizontalScope(float width, GroupModifier modifier, GUIStyle style) {
                Valid = BeginFlexibleHorizontalGroup(width, modifier, style);
            }

            public FlexibleHorizontalScope(float width, GroupModifier modifier = GroupModifier.None) {
                Valid = BeginFlexibleHorizontalGroup(width, modifier);
            }

            public void Dispose() {
                EndLayoutGroup<FlexibleHorizontalGroup>();
            }
        }
    }
}