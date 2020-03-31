using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class ScrollGroup : VerticalGroup {
        private const float MinimalScrollbarSizeMultiplier = 0.07f;

        private readonly Color _backgroundColor;

        protected override float GetAutomaticWidth() => LayoutEngine.CurrentContentWidth - TotalOffset.horizontal;

        public Vector2 ContainerSize;
        private float ContainerWidth;

        private Vector2 _containerToActualSizeRatio;

        // horizontal scrollbar settings
        private readonly int _horizontalScrollBarHeight;
        private readonly int _horizontalScrollBarOffset;
        private readonly int _bottomOffset;
        private bool _needsHorizontalScroll;
        private int _horizontalScrollId;

        // vertical scrollbar settings
        private readonly int _verticalScrollBarWidth;
        private readonly int _verticalScrollBarOffset;
        private readonly int _rightOffset;
        private bool _needsVerticalScroll;
        private int _verticalScrollId;
        
        private readonly Color _scrollbarColor;

        private Vector2 _scrollPos;
        public Vector2 ScrollPos {get => _scrollPos; set => _scrollPos = value;}
        public float ScrollPosX {get => _scrollPos.x; set => _scrollPos.x = value;}
        public float ScrollPosY {get => _scrollPos.y; set => _scrollPos.y = value;}

        private bool _disableScrollbars;

        public ScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars, Constraints modifier, GUIStyle style) : base(modifier, style) {
            Clip = true;

            ContainerSize = containerSize;

            // Scroll settings
            _scrollPos = scrollPos;

            _disableScrollbars = disableScrollbars;

            var padding = style.padding;
            var border = style.border;
            var margin = style.margin;
            // Vertical
            _verticalScrollBarWidth = border.right;
            _verticalScrollBarOffset = padding.right;
            _rightOffset = margin.right;

            // Horizontal
            _horizontalScrollBarHeight = border.bottom;
            _horizontalScrollBarOffset = padding.bottom;
            _bottomOffset = margin.bottom;

            // Colors
            _backgroundColor = style.normal.textColor;
            _scrollbarColor = style.onNormal.textColor;
        }
        public ScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars = false, Constraints modifier = Constraints.None)
            : this(containerSize, scrollPos, disableScrollbars, modifier, ExtendedEditorGUI.LayoutResources.ScrollGroup) {}

        protected override void PreLayoutRequest() {
            // These offsets are applied even if scrollbars are not needed
            TotalOffset.right = _rightOffset;
            TotalOffset.bottom = _bottomOffset;

            /* VERTICAL */
            TotalHeight += SpaceBetweenEntries * (EntriesCount - 1);
            if (_needsVerticalScroll = TotalHeight > ContainerSize.y) {
                _containerToActualSizeRatio.y = ContainerSize.y / TotalHeight;
                NextEntryPosition.y += Mathf.Lerp(0, ContainerSize.y - TotalHeight, _scrollPos.y);

                TotalHeight = ContainerSize.y;

                if(!_disableScrollbars) {
                    TotalOffset.right += _verticalScrollBarOffset + _verticalScrollBarWidth;
                }
            }


            /* HORIZONTAL */
            ContainerWidth = ContainerSize.x > 0 ? ContainerSize.x : LayoutEngine.CurrentContentWidth;
            TotalWidth = TotalWidth > 0 ? TotalWidth : ContainerWidth;
            if(_needsHorizontalScroll = TotalWidth > ContainerWidth) {
                _containerToActualSizeRatio.x = ContainerWidth / TotalWidth;
                NextEntryPosition.x += Mathf.Lerp(0, ContainerWidth - TotalWidth, _scrollPos.x);

                TotalWidth = ContainerWidth;

                if(!_disableScrollbars) {
                    TotalOffset.bottom += _horizontalScrollBarOffset + _horizontalScrollBarHeight;
                }
            }

            // Applying offsets
            TotalHeight += TotalOffset.vertical;
            TotalWidth += TotalOffset.horizontal;
        }

        private float DoGenericScrollbar(Event currentEvent, float scrollPos, Rect scrollbarRect,
            Rect backgroundRect, int controlId, bool verticalBar, float totalMovementLength) {
            switch (currentEvent.type) {
                case EventType.MouseDown:
                    var mousePos = currentEvent.mousePosition;
                    if (backgroundRect.Contains(mousePos)) {
                        currentEvent.Use();
                        GUIUtility.keyboardControl = 0;
                        GUIUtility.hotControl = controlId;

                        if (!scrollbarRect.Contains(mousePos)){
                            scrollPos = verticalBar
                                ? mousePos.y / ContentRect.yMax
                                : mousePos.x / ContentRect.xMax;
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
                    // Background
                    if (_backgroundColor.a > 0f) EditorGUI.DrawRect(backgroundRect, _backgroundColor);

                    // Actual scrollbar
                    EditorGUI.DrawRect(scrollbarRect, _scrollbarColor);

                    break;
            }

            return scrollPos;
        }
    
        internal override void BeginNonLayout() {
            RetrieveLayoutData();

            _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
            _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
        } 
        protected override void EndNonLayoutRoutine() {
            if (!IsGroupValid || _disableScrollbars) return;
            var current = Event.current;
            var eventType = current.type;

            var actualContentRect = ContentRect;

            if (_needsVerticalScroll) {
                var scrollbarHeight = Mathf.Max(actualContentRect.height * _containerToActualSizeRatio.y,
                    actualContentRect.height * MinimalScrollbarSizeMultiplier);
                var scrollMovementLength = actualContentRect.height - scrollbarHeight;

                if (eventType == EventType.ScrollWheel && actualContentRect.Contains(current.mousePosition)) {
                    current.Use();
                    GUIUtility.keyboardControl = 0;

                    _scrollPos.y = Mathf.Clamp01(_scrollPos.y + current.delta.y / scrollMovementLength);
                    return;
                }

                var verticalScrollPos = actualContentRect.xMax + _verticalScrollBarOffset;

                var verticalScrollbarRect = new Rect(
                    verticalScrollPos,
                    actualContentRect.y + scrollMovementLength * _scrollPos.y,
                    _verticalScrollBarWidth,
                    scrollbarHeight
                );

                var verticalScrollbarBackgroundRect = new Rect(
                    verticalScrollPos,
                    actualContentRect.y,
                    _verticalScrollBarWidth,
                    actualContentRect.height
                );

                _scrollPos.y =
                    DoGenericScrollbar(
                        current,
                        _scrollPos.y,
                        verticalScrollbarRect, verticalScrollbarBackgroundRect,
                        _verticalScrollId,
                        true,
                        scrollMovementLength
                    );
            }

            if (_needsHorizontalScroll) {
                var scrollBarWidth = Mathf.Max(actualContentRect.width * _containerToActualSizeRatio.x,
                    actualContentRect.width * MinimalScrollbarSizeMultiplier);
                var scrollMovementLength = actualContentRect.width - scrollBarWidth;

                var horizontalScrollPos = actualContentRect.yMax + _horizontalScrollBarOffset;

                var horizontalScrollbarRect = new Rect(
                    actualContentRect.x + scrollMovementLength * _scrollPos.x,
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

                _scrollPos.x =
                    DoGenericScrollbar(
                        current,
                        _scrollPos.x,
                        horizontalScrollbarRect,
                        horizontalScrollbarBackgroundRect,
                        _horizontalScrollId,
                        false,
                        scrollMovementLength
                    );
            }
        }
    }
}