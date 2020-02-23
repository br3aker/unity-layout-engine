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
            
            public ScrollGroup(float height, float width, Vector2 scrollPos, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
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

            private float _actualContentWidth;
            
            // TODO: redo this method 
            // Actually we only need to assign fixed Width and Height to this
            // Actual content position can be calculated in CalculateScrollContainerSize() method
            protected override void PreLayoutRequest() {
                // Same can be done with content height
                _actualContentWidth = TotalRequestedWidth;
                TotalRequestedWidth = _containerSize.x;
                
                if (_containerSize.y > 0f && TotalRequestedHeight > _containerSize.y) {
                    _needsVerticalScroll = true;
                    
                    _containerToActualSizeRatio.y = _containerSize.y / TotalRequestedHeight;
                    NextEntryPosition.y = Mathf.Lerp(0f, _containerSize.y - TotalRequestedHeight, ScrollPos.y);
                    
                    TotalRequestedHeight = _containerSize.y;
                }
            }

            internal void CalculateScrollContainerSize() {
				_verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
			
                var allowedWidth = ContainerRect.width;
                var actualWidth = _actualContentWidth > 0 ? _actualContentWidth - ServiceWidth : AutomaticEntryWidth;
                
                if (actualWidth > allowedWidth) {
                    _needsHorizontalScroll = true;
                    NextEntryPosition.x = Mathf.Lerp(0f, allowedWidth - actualWidth, ScrollPos.x);
                    _containerToActualSizeRatio.x = allowedWidth / actualWidth;
                }
                else {
                    _needsHorizontalScroll = false;
                    ContainerRect.width = actualWidth;
                }
            }

            internal void DoScrollGroupEndRoutine() {
                var current = Event.current;
                var eventType = current.type;
                if (!IsGroupValid) return;

                var actualContentRect = ContainerRect;
                ContainerRect = Margin.Add(Border.Add(Padding.Add(ContainerRect)));
                

                if (_needsVerticalScroll) {
//                    Debug.Log("Vertical scroll code");
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

        public static bool BeginScrollGroup(float width, float height, Vector2 scrollValue, GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterGroup(new ScrollGroup(height, width, scrollValue, modifier, style));
            }

            var currentGroup = GatherGroup() as ScrollGroup;
            currentGroup.CalculateScrollContainerSize();
            return currentGroup.IsGroupValid;
        }
        public static bool BeginScrollGroup(float width, float height, Vector2 scrollValue, GroupModifier modifier = GroupModifier.None) {
            return BeginScrollGroup(width, height, scrollValue, modifier, ExtendedEditorGUI.Resources.LayoutGroups.ScrollGroup);
        }

        public static Vector2 EndScrollGroup() {
            var group = EndLayoutGroup<ScrollGroup>();
            group.DoScrollGroupEndRoutine();
            return group.ScrollPos;
        }
    }
}
