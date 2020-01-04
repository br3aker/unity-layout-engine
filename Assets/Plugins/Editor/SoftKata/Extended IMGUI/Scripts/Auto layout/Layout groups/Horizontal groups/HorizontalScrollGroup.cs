using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        internal class HorizontalScrollGroup : HorizontalLayoutGroupBase {
            private float _containerWidth;
            
            internal float ScrollPos;

            private bool _needsScroll;

            public HorizontalScrollGroup(float width, float scrollPos, GUIStyle style) : base(style) {
                _containerWidth = width;
                ScrollPos = scrollPos;
            }

            protected override void CalculateLayoutData() {
                if (CurrentEventType == EventType.Layout) {
                    TotalWidth += ContentOffset * (EntriesCount - 1);
                    if (TotalWidth > _containerWidth) {
                        _needsScroll = true;
                        NextEntryX = Mathf.Lerp(0f, _containerWidth - TotalWidth, ScrollPos);

                        // this action is not very clear, TotalWidth is used at layout entries data registration
                        // probably needs to be renamed
                        TotalWidth = _containerWidth;
                    }
                }
            }

            internal override Rect GetRect(float height) {
                return GetRect(height, EditorGUIUtility.currentViewWidth);
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryX + width < 0 || NextEntryX > TotalWidth) {
                    return InvalidRect;
                }
                return new Rect(
                    NextEntryX,
                    NextEntryY,
                    width,
                    height
                );
            }
        }

        public static bool BeginHorizontalScrollGroup(float width, float scrollValue, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalScrollGroup(width, scrollValue, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            ActiveGroupStack.Push(layoutGroup);
            TopGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHorizontalScrollGroup(float width, float scrollValue) {
            return BeginHorizontalScrollGroup(width, scrollValue, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalScrollGroup);
        }

        public static float EndHorizontalScrollGroup() {
            var lastGroup = EndLayoutGroup() as HorizontalScrollGroup;
            if (lastGroup == null) {
                throw new Exception("Layout group type mismatch");
            }

            return lastGroup.ScrollPos;
        }
    }
}