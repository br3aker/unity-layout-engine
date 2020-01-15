using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalScrollGroup : HorizontalLayoutGroupBase {
            private float _containerWidth;
            
            internal float ScrollPos;

            private bool _needsScroll;

            private int _groupId;
            
            // Scrollbar settings
            private float _scrollBarContentOffset;
            
            private float _scrollBarFullHeight;
            private float _scrollBarMinimizedHeightDelta;
            
            private float _scrollBarMinimalWidth;
            private float _scrollBarWidth;

            private Color _backgroundColor;
            private Color _scrollbarColor;

            private bool _scrollbarPositionedAtBottom;

            public HorizontalScrollGroup(bool discardMargin, float width, float scrollPos, GUIStyle style) : base(discardMargin, style) {
                _containerWidth = width;
                ScrollPos = scrollPos;
                
                _groupId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                
                _scrollBarContentOffset = style.contentOffset.y;
                
                _scrollBarFullHeight = style.border.top;
                _scrollBarMinimizedHeightDelta = _scrollBarFullHeight - style.border.bottom;
                
                _scrollBarMinimalWidth = style.border.right;
                
                _backgroundColor = style.normal.textColor;
                _scrollbarColor = style.onNormal.textColor;

                _scrollbarPositionedAtBottom = (int)style.alignment > 2;
            }

            protected override void CalculateLayoutData() {
                TotalContainerHeight += _scrollBarFullHeight + _scrollBarContentOffset;

                if (!_scrollbarPositionedAtBottom) {
                    NextEntryY = _scrollBarContentOffset + _scrollBarFullHeight;
                }
                ;
                if (TotalContainerWidth > _containerWidth) {
                    _needsScroll = true;
                    NextEntryX = Mathf.Lerp(0f, _containerWidth - TotalContainerWidth, ScrollPos);
                        
                    float _containerToContentWidthRatio = _containerWidth / TotalContainerWidth;
                    _scrollBarWidth = Mathf.Max(_containerWidth * _containerToContentWidthRatio, _scrollBarMinimalWidth);

                    // this action is not very clear, TotalWidth is used at layout entries data registration
                    // probably needs to be renamed
                    TotalContainerWidth = _containerWidth;
                }
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                if (!_needsScroll) return;

                float scrollbarVerticalPosition = 
                    _scrollbarPositionedAtBottom 
                    ? FullRect.height - _scrollBarFullHeight
                    : 0f;
                
                var scrollbarRect = new Rect(
                    (FullRect.width - _scrollBarWidth) * ScrollPos,
                    scrollbarVerticalPosition,
                    _scrollBarWidth,
                    _scrollBarFullHeight
                );

                var scrollbarBackgroundRect = new Rect(
                    0f,
                    scrollbarVerticalPosition,
                    FullRect.width,
                    _scrollBarFullHeight
                );

                switch (currentEventType) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (scrollbarRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = _groupId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (scrollbarBackgroundRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = _groupId;
                                GUIUtility.keyboardControl = 0;
                                
                                ScrollPos = Event.current.mousePosition.x / FullRect.width;
                                
                                Event.current.Use();
                            }
                        }

                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == _groupId) {
                            GUIUtility.hotControl = 0;
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == _groupId) {
                            var currentX = Mathf.Clamp((scrollbarRect.x + Event.current.delta.x) - FullRect.x, 0f, FullRect.width - _scrollBarWidth);

                            ScrollPos = currentX / (FullRect.width - _scrollBarWidth);
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.ScrollWheel:
                        if (FullRect.Contains(Event.current.mousePosition)) {
                            GUIUtility.keyboardControl = 0;
                            
                            ScrollPos = Mathf.Clamp01(ScrollPos + Event.current.delta.y / FullRect.width);
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.Repaint:
                        // Check if we should render full-sized scrollbar
                        if (!FullRect.Contains(Event.current.mousePosition) && GUIUtility.hotControl != _groupId) {
                            if (_scrollbarPositionedAtBottom) {
                                scrollbarBackgroundRect.yMin += _scrollBarMinimizedHeightDelta;
                                scrollbarRect.yMin += _scrollBarMinimizedHeightDelta;
                            }
                            else {
                                scrollbarBackgroundRect.height -= _scrollBarMinimizedHeightDelta;
                                scrollbarRect.height -= _scrollBarMinimizedHeightDelta;
                            }
                        }
                        
                        // Background
                        if (_backgroundColor.a > 0f) {
                            EditorGUI.DrawRect(scrollbarBackgroundRect, _backgroundColor);
                        }
                        
                        // Scrollbar
                        EditorGUI.DrawRect(scrollbarRect, _scrollbarColor);
                        break;
                }
            }
        }

        public static bool BeginHorizontalScrollGroup(bool discardMarginAndPadding, float width, float scrollValue, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalScrollGroup(discardMarginAndPadding, width, scrollValue, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHorizontalScrollGroup(float width, float scrollValue, bool discardMarginAndPadding = false) {
            return BeginHorizontalScrollGroup(discardMarginAndPadding, width, scrollValue, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalScrollGroup);
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