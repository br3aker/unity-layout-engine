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

            public FlexibleHorizontalGroup(float width, GroupModifier modifier, GUIStyle style) :
                base(modifier, style) {
                RequestedWidth = width;
            }

            internal void CalculateLayout() {
                var totalFlexibleWidth =
                    ContainerRect.width - _fixedEntriesWidth - ContentOffset.x * (EntriesCount - 1);

                _pureContentWidth = Mathf.Max(totalFlexibleWidth / (EntriesCount - _fixedEntriesCount), 0f);
            }

            protected override bool PrepareNextRect(float width, float height) {
                if (IsLayout) {
                    if (width > 0) {
                        _fixedEntriesCount++;
                        _fixedEntriesWidth += width;
                    }

                    EntriesCount++;
                    RequestedHeight = Mathf.Max(RequestedHeight, height);
                    return false;
                }

                if (!IsGroupValid) return false;

                NextEntryPosition.x += width + ContentOffset.x;

                // occlusion
                return CurrentEntryPosition.x + width >= VisibleAreaRect.x
                       && CurrentEntryPosition.x <= VisibleAreaRect.xMax;
            }

            internal override void RegisterArray(float elemWidth, float elemHeight, int count) {
                if (elemWidth > 0f) {
                    _fixedEntriesCount += count;
                    _fixedEntriesWidth += elemHeight * count;
                }

                EntriesCount += count;
                RequestedHeight = Mathf.Max(RequestedHeight, elemHeight);
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