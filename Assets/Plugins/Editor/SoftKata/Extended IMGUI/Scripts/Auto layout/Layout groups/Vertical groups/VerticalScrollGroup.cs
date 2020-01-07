using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalScrollGroup : VerticalLayoutGroupBase {
            private float _containerHeight;
            
            internal float ScrollPos;

            private bool _needsScroll;

            public VerticalScrollGroup(float height, float scrollPos, GUIStyle style) : base(style) {
                _containerHeight = height;
                ScrollPos = scrollPos;
            }

            protected override void CalculateLayoutData() {
                TotalHeight += ContentOffset * (EntriesCount - 1);
                
                if (TotalHeight > _containerHeight) {
                    _needsScroll = true;
                    NextEntryY = Mathf.Lerp(0f, _containerHeight - TotalHeight, ScrollPos);

                    // this action is not very clear, TotalHeight is used at layout entries data registration
                    // probably needs to be renamed
                    TotalHeight = _containerHeight;
                }
            }

            internal override Rect GetRect(float height) {
                return GetRect(height, EditorGUIUtility.currentViewWidth);
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryY + height < 0 || NextEntryY > _containerHeight) {
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

        public static bool BeginVerticalScrollGroup(float height, float scrollValue, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalScrollGroup(height, scrollValue, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
                layoutGroup.RegisterDebugData();
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalScrollGroup(float height, float scrollValue) {
            return BeginVerticalScrollGroup(height, scrollValue, ExtendedEditorGUI.Resources.LayoutGroup.VerticalScrollGroup);
        }

        public static float EndVerticalScrollGroup() {
            var lastGroup = EndLayoutGroup() as VerticalScrollGroup;
            if (lastGroup == null) {
                throw new Exception("Layout group type mismatch");
            }

            return lastGroup.ScrollPos;
        }
    }
}