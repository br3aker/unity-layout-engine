using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalScrollGroup : VerticalLayoutGroupBase {
            private float _containerHeight;
            
            internal float ScrollPos;

            private bool _needsScroll;

            private int _groupId;
            
            // Scrollbar settings
            private float _scrollBarContentOffset;
            
            private float _scrollBarFullWidth;
            private float _scrollBarMinimizedWidthDelta;
            
            private float _scrollBarMinimalHeight;
            private float _scrollBarHeight;

            private Color _backgroundColor;
            private Color _scrollbarColor;

            public VerticalScrollGroup(float height, float scrollPos, GUIStyle style) : base(style) {
                _containerHeight = height;
                ScrollPos = scrollPos;

                _groupId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                
                _scrollBarContentOffset = style.contentOffset.x;
                
                _scrollBarFullWidth = style.border.right;
                _scrollBarMinimizedWidthDelta = _scrollBarFullWidth - style.border.left;
                
                _scrollBarMinimalHeight = style.border.top;

                // Colors
                _backgroundColor = style.normal.textColor;
                _scrollbarColor = style.onNormal.textColor;
            }

            protected override void CalculateLayoutData() {
                TotalHeight += ContentOffset * (EntriesCount - 1);
                TotalWidth += _scrollBarFullWidth + _scrollBarContentOffset;

                if (TotalHeight > _containerHeight) {
                    _needsScroll = true;
                    NextEntryY = Mathf.Lerp(0f, _containerHeight - TotalHeight, ScrollPos);

                    float containerToContentHeightRatio = _containerHeight / TotalHeight;
                    _scrollBarHeight = Mathf.Max(_containerHeight * containerToContentHeightRatio, _scrollBarMinimalHeight);
                    
                    // this action is not very clear, TotalHeight is used at layout entries data registration
                    // probably needs to be renamed
                    TotalHeight = _containerHeight;
                }
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                if (!_needsScroll) return;
                
                float sliderHorizontalPosition = FullRect.width - _scrollBarFullWidth;

                var scrollbarRect = new Rect(
                    sliderHorizontalPosition,
                    (FullRect.height - _scrollBarHeight) * ScrollPos,
                    _scrollBarFullWidth,
                    _scrollBarHeight
                );

                var scrollbarBackgroundRect = new Rect(
                    sliderHorizontalPosition,
                    FullRect.y,
                    _scrollBarFullWidth,
                    FullRect.height
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
                                
                                ScrollPos = Event.current.mousePosition.y / FullRect.height; 
                                
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
                            var delta = Event.current.delta.y;
                            var currentY = Mathf.Clamp(scrollbarRect.y + delta, 0f, FullRect.height - _scrollBarHeight);

                            ScrollPos = currentY / (FullRect.height - _scrollBarHeight);
                            
                            Event.current.Use();
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
                        // Check if we should render full-sized scrollbar
                        if (!FullRect.Contains(Event.current.mousePosition) && GUIUtility.hotControl != _groupId) {
                            scrollbarBackgroundRect.xMin += _scrollBarMinimizedWidthDelta;
                            scrollbarRect.xMin += _scrollBarMinimizedWidthDelta;
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