using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class ScrollGroup : VerticalClippingGroup {
            private const float MinimalScrollbarSizeMultiplier = 0.07f;

            private Vector2 _containerSize;
            
            internal Vector2 ScrollPos;

            private bool _needsVerticalScroll;
            private bool _needsHorizontalScroll;

            private int _verticalScrollId;
            private int _horizontalScrollId;
            
            // Scrollbar settings
            private float _verticalScrollBarWidth;
            private float _horizontalScrollBarHeight;
            
            private float _verticalScrollLength;
            private float _horizontalScrollLength;

            private Color _backgroundColor;
            private Color _scrollbarColor;

            public ScrollGroup(bool discardMargin, float height, float width, Vector2 scrollPos, GUIStyle style) : base(discardMargin, style) {
                _containerSize = new Vector2(width, height);
                ScrollPos = scrollPos;
                
                _verticalScrollBarWidth = Border.right;
                _horizontalScrollBarHeight = Border.bottom;

                // Colors
                _backgroundColor = style.normal.textColor;
                _scrollbarColor = style.onNormal.textColor;


                MaxAllowedWidth = _containerSize.x - Margin.horizontal - Padding.horizontal;
                
                _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
            }

            protected override void CalculateLayoutData() {
                if (TotalRequestedHeight > _containerSize.y) {
                    _needsVerticalScroll = true;
                    NextEntryPosition.y = Mathf.Lerp(0f, _containerSize.y - TotalRequestedHeight, ScrollPos.y);
                    
                    float containerToContentHeightRatio = _containerSize.y / TotalRequestedHeight;
                    _verticalScrollLength = Mathf.Max(_containerSize.y * containerToContentHeightRatio, _containerSize.y * MinimalScrollbarSizeMultiplier);

                    // this action is not very clear, TotalHeight is used at layout entries data registration
                    // probably needs to be renamed
                    TotalRequestedHeight = _containerSize.y;
                }
                
                if (TotalRequestedWidth > _containerSize.x) {
                    _needsHorizontalScroll = true;
                    NextEntryPosition.x = Mathf.Lerp(0f, _containerSize.x - TotalRequestedWidth, ScrollPos.x);

                    float containerToContentWidthRatio = _containerSize.x / TotalRequestedWidth;
                    _horizontalScrollLength = Mathf.Max(_containerSize.x * containerToContentWidthRatio, _containerSize.x * MinimalScrollbarSizeMultiplier);

                    // this action is not very clear, TotalHeight is used at layout entries data registration
                    // probably needs to be renamed
                    TotalRequestedWidth = _containerSize.x;
                }
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                if (!_needsVerticalScroll && !_needsHorizontalScroll) return;

                // Vertical
                float verticalScrollPos = FullContainerRect.xMax - _verticalScrollBarWidth;
                var verticalScrollRect = new Rect(
                    verticalScrollPos,
                    FullContainerRect.y + (FullContainerRect.height - _verticalScrollLength - Padding.bottom) * ScrollPos.y,
                    _verticalScrollBarWidth,
                    _verticalScrollLength
                );
                var verticalScrollbarBackgroundRect = new Rect(
                    verticalScrollPos,
                    FullContainerRect.y,
                    _verticalScrollBarWidth,
                    FullContainerRect.height - Padding.vertical
                );
                
                
                //Horizontal
                float horizontalScrollPos = FullContainerRect.yMax - _horizontalScrollBarHeight;
                var horizontalScrollRect = new Rect(
                    FullContainerRect.x + (FullContainerRect.width - _horizontalScrollLength - Padding.right) * ScrollPos.x,
                    horizontalScrollPos,
                    _horizontalScrollLength,
                    _horizontalScrollBarHeight
                );
                var horizontalScrollbarBackgroundRect = new Rect(
                    FullContainerRect.x,
                    horizontalScrollPos,
                    FullContainerRect.width - Padding.horizontal,
                    _horizontalScrollBarHeight
                );

                switch (currentEventType) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (verticalScrollRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = _verticalScrollId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (verticalScrollbarBackgroundRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = _verticalScrollId;
                                GUIUtility.keyboardControl = 0;
                                
                                ScrollPos.y = Event.current.mousePosition.y / FullContainerRect.height; 
                                
                                Event.current.Use();
                            }
                        }

                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == _verticalScrollId) {
                            GUIUtility.hotControl = 0;
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == _verticalScrollId) {
                            var delta = Event.current.delta.y;
                            var currentY = Mathf.Clamp(verticalScrollRect.y + delta, 0f, FullContainerRect.height - _verticalScrollLength);

                            ScrollPos.y = currentY / (FullContainerRect.height - _verticalScrollLength);
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.ScrollWheel:
                        if (FullContainerRect.Contains(Event.current.mousePosition)) {
                            GUIUtility.keyboardControl = 0;
                            
                            ScrollPos.y = Mathf.Clamp01(ScrollPos.y + Event.current.delta.y / FullContainerRect.height);
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.Repaint:
//                        // Check if we should render full-sized scrollbar
//                        if (!FullRect.Contains(Event.current.mousePosition) && GUIUtility.hotControl != _verticalScrollId) {
//                            scrollbarBackgroundRect.xMin += _scrollBarMinimizedWidthDelta;
//                            verticalScrollRect.xMin += _scrollBarMinimizedWidthDelta;
//                        }
//                        
//                        // Background
//                        if (_backgroundColor.a > 0f) {
//                            EditorGUI.DrawRect(horizontalScrollbarBackgroundRect, _backgroundColor);
//                        }
//                        
//                        // Scrollbar
//                        EditorGUI.DrawRect(verticalScrollRect, _scrollbarColor);
//                        break;

                        EditorGUI.DrawRect(verticalScrollbarBackgroundRect, Color.red);
                        EditorGUI.DrawRect(verticalScrollRect, Color.cyan);

                        EditorGUI.DrawRect(horizontalScrollbarBackgroundRect, Color.red);
                        EditorGUI.DrawRect(horizontalScrollRect, Color.cyan);
                        break;
                }
            }
        }

        public static bool BeginHybridScrollGroup(bool discardMarginAndPadding, float width, float height, Vector2 scrollValue, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new ScrollGroup(discardMarginAndPadding, width, height, scrollValue, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHybridScrollGroup(float width, float height, Vector2 scrollValue, bool discardMarginAndPadding = false) {
            return BeginHybridScrollGroup(discardMarginAndPadding, width, height, scrollValue, ExtendedEditorGUI.Resources.LayoutGroup.VerticalScrollGroup);
        }

        public static Vector2 EndHybridScrollGroup() {
            var lastGroup = EndLayoutGroup() as ScrollGroup;
            if (lastGroup == null) {
                throw new Exception("Layout group type mismatch");
            }

            return lastGroup.ScrollPos;
        }
    }
}
