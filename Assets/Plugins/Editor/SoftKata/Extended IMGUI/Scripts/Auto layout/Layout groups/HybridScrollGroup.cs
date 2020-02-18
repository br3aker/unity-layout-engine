using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class ScrollGroup : VerticalClippingGroup {
            private const float MinimalScrollbarSizeMultiplier = 0.07f;

            private float _actualContainerWidth;
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
            
            public ScrollGroup(float height, float width, Vector2 scrollPos, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                // Container size
                _containerSize = new Vector2(width, height);
                
                // Scroll settings
                ScrollPos = scrollPos;
                
                var overflow = style.overflow;
                _verticalScrollBarWidth = overflow.right;
                _verticalScrollbarDelta = new Vector2(_verticalScrollBarWidth - overflow.left, 0f);
                
                _horizontalScrollBarHeight = overflow.bottom;
                _horizontalScrollbarDelta = new Vector2(0f, _horizontalScrollBarHeight - overflow.top);

                // Colors
                _backgroundColor = style.normal.textColor;
                _scrollbarColor = style.onNormal.textColor;

                GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
            }
            
            protected override void PreLayoutRequest() {
                if (TotalRequestedWidth > _containerSize.x) {
                    _needsHorizontalScroll = true;

                    _actualContainerWidth = TotalRequestedWidth;
                    TotalRequestedWidth = _containerSize.x;
                }
                
                if (TotalRequestedHeight > _containerSize.y) {
                    _needsVerticalScroll = true;
                    
                    _containerToActualSizeRatio.y = _containerSize.y / TotalRequestedHeight;
                    NextEntryPosition.y = Mathf.Lerp(0f, _containerSize.y - TotalRequestedHeight, ScrollPos.y);
                    
                    TotalRequestedHeight = _containerSize.y;
                }
            }
            
            internal override void RetrieveLayoutData() {
                _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                if (IsGroupValid) {
                    if (Parent != null) {
                        var rectData = Parent.GetGroupRectData(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = rectData.VisibleRect;
                        ContainerRect = rectData.FullContentRect;
                    }
                    else {
                        ContainerRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = ContainerRect;
                    }
                    
                    IsGroupValid = VisibleAreaRect.IsValid();

                    if (IsGroupValid) {
                        IsLayout = false;
                        
                        // Scroll bar calculations
                        var allowedWidth = ContainerRect.width;
                        if (_actualContainerWidth > allowedWidth) {
                            NextEntryPosition.x = Mathf.Lerp(0f, allowedWidth - _actualContainerWidth, ScrollPos.x);
                            _containerToActualSizeRatio.x = allowedWidth / _actualContainerWidth;
                        }
                        else {
                            _needsHorizontalScroll = false;
                            ContainerRect.width = _actualContainerWidth;
                        }
                        ContainerRect = Padding.Remove(Border.Remove(Margin.Remove(ContainerRect)));

                        VisibleAreaRect = Utility.RectIntersection(VisibleAreaRect, ContainerRect);

                        NextEntryPosition += ContainerRect.position - VisibleAreaRect.position;

                        GUI.BeginClip(VisibleAreaRect);
                        VisibleAreaRect.position = Vector2.zero;


                        if (AutomaticEntryWidth < 0f) {
                            AutomaticEntryWidth = ContainerRect.width;
                        }

                        return;
                    }
                }

                // Nested groups should be banished exactly here at non-layout layout data pull
                // This would ensure 2 things:
                // 1. Entries > 0 because this is called after PushLayoutRequest() which checks that
                // 2. Parent group returned Valid rect
                if (Parent != null) {
                    Parent.EntriesCount -= _childrenCount + 1;
                }
                LayoutEngine.ScrapGroups(_childrenCount);
            }

            internal void DoScrollGroupEndRoutine() {
                var current = Event.current;
                var eventType = current.type;
                if (eventType == EventType.Layout || !IsGroupValid) return;

                var actualContentRect = ContainerRect;
                ContainerRect = Margin.Add(Border.Add(Padding.Add(ContainerRect)));
                

                if (_needsVerticalScroll) {
                    float scrollbarHeight = Mathf.Max(actualContentRect.height * _containerToActualSizeRatio.y, actualContentRect.height * MinimalScrollbarSizeMultiplier);
                    float scrollMovementLength = actualContentRect.height - scrollbarHeight;
                    
                    if (eventType == EventType.ScrollWheel && ContainerRect.Contains(current.mousePosition)) {
                        current.Use();
                        GUIUtility.keyboardControl = 0;
                        
                        ScrollPos.y = Mathf.Clamp01(ScrollPos.y + current.delta.y / scrollMovementLength);
                        return;
                    }

                    float verticalScrollPos = actualContentRect.xMax - _verticalScrollBarWidth + Padding.right;
                    
                    var verticalScrollbarRect = new Rect(
                        verticalScrollPos,
                        actualContentRect.y + scrollMovementLength * ScrollPos.y,
                        _verticalScrollBarWidth,
                        scrollbarHeight
                    );
                    
                    var verticalScrollbarBackgroundRect = new Rect(
                        verticalScrollPos,
                        actualContentRect.y,
                        _verticalScrollBarWidth,
                        actualContentRect.height
                    );

                    ScrollPos.y = 
                        DoGenericScrollbar(
                            current,
                            ScrollPos.y,
                            verticalScrollbarRect, verticalScrollbarBackgroundRect, 
                            _verticalScrollId, 
                            true, 
                            scrollMovementLength,
                            _verticalScrollbarDelta
                        );
                }

                if (_needsHorizontalScroll) {
                    float scrollBarWidth = Mathf.Max(actualContentRect.width * _containerToActualSizeRatio.x,
                        actualContentRect.width * MinimalScrollbarSizeMultiplier);
                    float scrollMovementLength = actualContentRect.width - scrollBarWidth;

                    float horizontalScrollPos = actualContentRect.yMax - _horizontalScrollBarHeight + Padding.bottom;

                    var horizontalScrollbarRect = new Rect(
                        actualContentRect.x + scrollMovementLength * ScrollPos.x,
                        horizontalScrollPos,
                        scrollBarWidth,
                        _horizontalScrollBarHeight
                    );
                    var horizontalScrollbarBackgroundRect = new Rect(
                        actualContentRect.x,
                        horizontalScrollPos,
                        actualContentRect.width,
                        _horizontalScrollBarHeight
                    );

                    ScrollPos.x =
                        DoGenericScrollbar(
                            current,
                            ScrollPos.x,
                            horizontalScrollbarRect,
                            horizontalScrollbarBackgroundRect,
                            _horizontalScrollId,
                            false,
                            scrollMovementLength,
                            _horizontalScrollbarDelta
                        );
                }
            }
            private float DoGenericScrollbar(Event currentEvent, float scrollPos, Rect scrollbarRect, Rect backgroundRect, int controlId, bool verticalBar, float totalMovementLength, Vector2 renderingDelta) {
                switch (currentEvent.type) {
                    case EventType.MouseDown:
                        var mousePos = currentEvent.mousePosition;
                        if (backgroundRect.Contains(mousePos)) {
                            currentEvent.Use();
                            GUIUtility.keyboardControl = 0;
                            GUIUtility.hotControl = controlId;

                            if (!scrollbarRect.Contains(mousePos)) {
                                scrollPos = verticalBar 
                                    ? mousePos.y / ContainerRect.yMax 
                                    : mousePos.x / ContainerRect.xMax;
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId) {
                            currentEvent.Use();
                            GUIUtility.hotControl = 0;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlId) {
                            currentEvent.Use();
                            var dragDelta = currentEvent.delta;
                            var movementDelta = verticalBar ? dragDelta.y : dragDelta.x; 
                            scrollPos = Mathf.Clamp01(scrollPos + movementDelta / totalMovementLength);
                        }
                        break;
                    case EventType.Repaint:
                        // Check if we should render full-sized scrollbar
                        if (!ContainerRect.Contains(currentEvent.mousePosition)) {
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

        public static bool BeginHybridScrollGroup(float width, float height, Vector2 scrollValue, GroupModifier modifier, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new ScrollGroup(height, width, scrollValue, modifier, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData();
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHybridScrollGroup(float width, float height, Vector2 scrollValue, GroupModifier modifier = GroupModifier.None) {
            return BeginHybridScrollGroup(width, height, scrollValue, modifier, ExtendedEditorGUI.Resources.LayoutGroups.ScrollGroup);
        }

        public static Vector2 EndHybridScrollGroup() {
            var group = EndLayoutGroup<ScrollGroup>();
            group.DoScrollGroupEndRoutine();
            return group.ScrollPos;
        }
    }
}
