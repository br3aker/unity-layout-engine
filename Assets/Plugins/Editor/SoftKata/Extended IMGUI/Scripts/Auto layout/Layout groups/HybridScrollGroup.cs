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
            private Vector2 _verticalScrollbarDelta;
            
            private float _horizontalScrollBarHeight;
            private Vector2 _horizontalScrollbarDelta;
            
            private Vector2 _containerToActualSizeRatio;

            
            private Color _backgroundColor;
            private Color _scrollbarColor;

            public ScrollGroup(bool discardMargin, float height, float width, Vector2 scrollPos, GUIStyle style) : base(discardMargin, style) {
                _containerSize = new Vector2(width, height);
                ScrollPos = scrollPos;
                
                _verticalScrollBarWidth = Border.right;
                _verticalScrollbarDelta = new Vector2(_verticalScrollBarWidth - Border.left, 0f);
                
                _horizontalScrollBarHeight = Border.bottom;
                _horizontalScrollbarDelta = new Vector2(0f, _horizontalScrollBarHeight - Border.top);

                // Colors
                _backgroundColor = style.normal.textColor;
                _scrollbarColor = style.onNormal.textColor;


                AutomaticEntryWidth = _containerSize.x - Margin.horizontal - Padding.horizontal;
                
                _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
            }

            protected override void PreLayoutRequest() {
                if (TotalRequestedWidth > _containerSize.x) {
                    _needsHorizontalScroll = true;
                    _containerToActualSizeRatio.x = _containerSize.x / TotalRequestedWidth;
                    
                    NextEntryPosition.x = Mathf.Lerp(0f, _containerSize.x - TotalRequestedWidth, ScrollPos.x);
                    TotalRequestedWidth = _containerSize.x;
                }
                
                if (TotalRequestedHeight > _containerSize.y) {
                    _needsVerticalScroll = true;
                    _containerToActualSizeRatio.y = _containerSize.y / TotalRequestedHeight;
                    
                    NextEntryPosition.y = Mathf.Lerp(0f, _containerSize.y - TotalRequestedHeight, ScrollPos.y);
                    TotalRequestedHeight = _containerSize.y;
                }
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                var fullGroupRect = Margin.Add(Padding.Add(ContentRect));
                
                if (_needsVerticalScroll) {
                    float scrollbarHeight = Mathf.Max(ContentRect.height * _containerToActualSizeRatio.y, ContentRect.height * MinimalScrollbarSizeMultiplier);

                    float verticalScrollPos = ContentRect.xMax - _verticalScrollBarWidth + Padding.right;
                    float scrollMovementLength = ContentRect.height - scrollbarHeight; 
                    
                    var verticalScrollbarRect = new Rect(
                        verticalScrollPos,
                        ContentRect.y + scrollMovementLength * ScrollPos.y,
                        _verticalScrollBarWidth,
                        scrollbarHeight
                    );
                    
                    var verticalScrollbarBackgroundRect = new Rect(
                        verticalScrollPos,
                        ContentRect.y,
                        _verticalScrollBarWidth,
                        ContentRect.height
                    );
                    
                    ScrollPos.y = 
                        DoScrollbar(
                            fullGroupRect,
                            ScrollPos.y,
                            verticalScrollbarRect, verticalScrollbarBackgroundRect, 
                            _verticalScrollId, 
                            Event.current.delta.y, 
                            scrollMovementLength,
                            _verticalScrollbarDelta
                        );
                }

                if (_needsHorizontalScroll) {
                    float scrollBarWidth = Mathf.Max(ContentRect.width * _containerToActualSizeRatio.x, ContentRect.width * MinimalScrollbarSizeMultiplier);
                    float scrollMovementLength = ContentRect.width - scrollBarWidth;
                
                    float horizontalScrollPos = ContentRect.yMax - _horizontalScrollBarHeight + Padding.bottom;
                    var horizontalScrollbarRect = new Rect(
                        ContentRect.x + scrollMovementLength * ScrollPos.x,
                        horizontalScrollPos,
                        scrollBarWidth,
                        _horizontalScrollBarHeight
                    );
                    var horizontalScrollbarBackgroundRect = new Rect(
                        ContentRect.x,
                        horizontalScrollPos,
                        ContentRect.width,
                        _horizontalScrollBarHeight
                    );
                    
                    ScrollPos.x = 
                        DoScrollbar(
                            fullGroupRect,
                            ScrollPos.x,
                            horizontalScrollbarRect, 
                            horizontalScrollbarBackgroundRect, 
                            _horizontalScrollId, 
                            Event.current.delta.x,
                            scrollMovementLength,
                            _horizontalScrollbarDelta
                        );
                }
            }

            private float DoScrollbar(Rect fullGroupRect, float scrollPos, Rect scrollbarRect, Rect backgroundRect, int controlId, float movementDelta, float totalMovementLength, Vector2 renderingDelta) {
                switch (Event.current.type) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (scrollbarRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = controlId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (backgroundRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = controlId;
                                GUIUtility.keyboardControl = 0;
                                Event.current.Use();
                                
                                scrollPos = Event.current.mousePosition.x / ContentRect.xMax;
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId) {
                            GUIUtility.hotControl = 0;
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlId) {
                            scrollPos = Mathf.Clamp01(scrollPos + movementDelta / totalMovementLength);
                            
                            Event.current.Use();
                        }
                        break;
                    case EventType.Repaint:
                        // Check if we should render full-sized scrollbar
                        if (!fullGroupRect.Contains(Event.current.mousePosition) && GUIUtility.hotControl != controlId) {
                            scrollbarRect.min += renderingDelta;
                            backgroundRect.min += renderingDelta;
                        }
                        
                        // Background
                        if (_backgroundColor.a > 0f) {
                            EditorGUI.DrawRect(backgroundRect, _backgroundColor);
                        }
                        
                        // Actual scrollbar
                        EditorGUI.DrawRect(scrollbarRect, _scrollbarColor);
                        break;
                }

                return scrollPos;
            }
        }

        public static bool BeginHybridScrollGroup(bool discardMarginAndPadding, float width, float height, Vector2 scrollValue, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new ScrollGroup(discardMarginAndPadding, height, width, scrollValue, style);
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
            return EndLayoutGroup<ScrollGroup>().ScrollPos;
        }
    }
}
