using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalScrollGroup : VerticalLayoutGroupBase {
            private static readonly int VerticalScrollGroupHash = nameof(VerticalScrollGroup).GetHashCode();
            
            private float _containerHeight;
            
            internal float ScrollPos;

            private bool _needsScroll;

            private float _sliderContentOffset;
            private float _sliderWidth;
            private float _sliderHeight;
            private Color _sliderColor;
            private Color _sliderBackgroundColor;

            public VerticalScrollGroup(float height, float scrollPos, GUIStyle style) : base(style) {
                _containerHeight = height;
                ScrollPos = scrollPos;

                _sliderContentOffset = style.contentOffset.x;
                _sliderWidth = style.border.right;

                _sliderColor = style.focused.textColor;
                _sliderBackgroundColor = style.normal.textColor;
            }

            protected override void CalculateLayoutData() {
                TotalHeight += ContentOffset * (EntriesCount - 1);
                TotalWidth += _sliderWidth + _sliderContentOffset;

                if (TotalHeight > _containerHeight) {
                    _needsScroll = true;
                    NextEntryY = Mathf.Lerp(0f, _containerHeight - TotalHeight, ScrollPos);

                    float containerToContentHeightRatio = _containerHeight / TotalHeight;
                    _sliderHeight = _containerHeight * containerToContentHeightRatio;
                    
                    // this action is not very clear, TotalHeight is used at layout entries data registration
                    // probably needs to be renamed
                    TotalHeight = _containerHeight;
                }
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryY + height < 0 || NextEntryY > _containerHeight) {
                    return InvalidRect;
                }

                return new Rect(NextEntryX, NextEntryY, width, height);
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                if (!_needsScroll) return;
                
                var groupId = GUIUtility.GetControlID(VerticalScrollGroupHash, FocusType.Passive);
                
                float sliderHorizontalPosition = FullRect.width - _sliderWidth;

                var sliderRect = new Rect(
                    sliderHorizontalPosition,
                    (FullRect.height - _sliderHeight) * ScrollPos,
                    _sliderWidth,
                    _sliderHeight
                );

                var sliderBackgroundRect = new Rect(
                    sliderHorizontalPosition,
                    FullRect.y,
                    _sliderWidth,
                    FullRect.height
                );
                
                switch (currentEventType) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (sliderRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = groupId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (sliderBackgroundRect.Contains(Event.current.mousePosition)) {
                                ScrollPos = Event.current.mousePosition.y / FullRect.height; 
                                
                                GUIUtility.hotControl = groupId;
                                GUIUtility.keyboardControl = 0;
                                
                                Event.current.Use();
                            }
                        }

                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == groupId) {
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == groupId) {
                            var delta = Event.current.delta.y;
                            var currentY = Mathf.Clamp(sliderRect.y + delta, 0f, FullRect.height - _sliderHeight);

                            ScrollPos = currentY / (FullRect.height - _sliderHeight);
                        }
                        break;
                    case EventType.ScrollWheel:
                        if (FullRect.Contains(Event.current.mousePosition)) {
                            GUIUtility.keyboardControl = 0;
                            ScrollPos = Mathf.Clamp01(ScrollPos + Event.current.delta.y / FullRect.height);
                            Event.current.Use();
                        }
                        break;
                    case EventType.Repaint:
                        if (_sliderBackgroundColor.a > 0f) {
                            EditorGUI.DrawRect(sliderBackgroundRect, _sliderBackgroundColor);
                        }
                        EditorGUI.DrawRect(sliderRect, _sliderColor);
                        break;
                }
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