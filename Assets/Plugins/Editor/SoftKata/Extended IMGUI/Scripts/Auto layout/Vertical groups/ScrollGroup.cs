using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class ScrollGroup : VerticalGroup {
        private const float MinimalScrollbarSizeMultiplier = 0.07f;

        private readonly Color _backgroundColor;

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

        private Vector2 _actualContentSize;

        public ScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            Clip = true;

            ContainerSize = containerSize;

            // Scroll settings
            _scrollPos = scrollPos;


            if(_disableScrollbars = disableScrollbars) return;

            var padding = style.padding;
            var border = style.border;
            var margin = style.margin;
            // Vertical
            _verticalScrollBarWidth = border.right;
            _verticalScrollBarOffset = padding.right;

            // Horizontal
            _horizontalScrollBarHeight = border.bottom;
            _horizontalScrollBarOffset = padding.bottom;

            _rightOffset = margin.right;
            _bottomOffset = margin.bottom;

            // Colors
            _backgroundColor = style.normal.textColor;
            _scrollbarColor = style.onNormal.textColor;
        }
        public ScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars = false, bool ignoreConstaints = false)
            : this(containerSize, scrollPos, disableScrollbars, ExtendedEditorGUI.Resources.ScrollGroup, ignoreConstaints) {}

        protected override float CalculateAutomaticContentWidth() {
            return (ContainerSize.x > 0 ? ContainerSize.x : AvailableWidth) - (TotalOffset.left + _rightOffset);
        }

        protected override void PreLayoutRequest() {
            // These offsets are applied even if scrollbars are not needed
            TotalOffset.right = _rightOffset;
            TotalOffset.bottom = _bottomOffset;

            /* VERTICAL */
            EntriesRequestedSize.y += SpaceBetweenEntries * (EntriesCount - 1);
            _actualContentSize.y = EntriesRequestedSize.y;
            if (_needsVerticalScroll = EntriesRequestedSize.y > ContainerSize.y) {
                _containerToActualSizeRatio.y = ContainerSize.y / EntriesRequestedSize.y;

                EntriesRequestedSize.y = ContainerSize.y;

                if(!_disableScrollbars) {
                    TotalOffset.right += _verticalScrollBarOffset + _verticalScrollBarWidth;
                }
            }


            /* HORIZONTAL */
            ContainerWidth = ContainerSize.x > 0 ? ContainerSize.x : AvailableWidth;
            EntriesRequestedSize.x = EntriesRequestedSize.x > 0 ? (EntriesRequestedSize.x + TotalOffset.horizontal) : ContainerWidth;
            _actualContentSize.x = EntriesRequestedSize.x;
            if(_needsHorizontalScroll = EntriesRequestedSize.x > ContainerWidth) {
                _containerToActualSizeRatio.x = ContainerWidth / EntriesRequestedSize.x;

                EntriesRequestedSize.x = ContainerWidth;

                if(!_disableScrollbars) {
                    TotalOffset.bottom += _horizontalScrollBarOffset + _horizontalScrollBarHeight;
                }
            }

            // Applying offsets
            EntriesRequestedSize.y += TotalOffset.vertical;
        }
    
        internal override bool BeginNonLayout() {
            if(base.BeginNonLayout()) {
                // requesting ids for scrollbars
                _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);

                // scroll content offset
                var contentOffset = 
                    new Vector2(
                        Mathf.Lerp(0, ContainerWidth - _actualContentSize.x, _scrollPos.x), 
                        Mathf.Lerp(0, ContainerSize.y - _actualContentSize.y, _scrollPos.y)
                    );
                NextEntryPosition += contentOffset;
                return true;
            }
            return false;
        } 
        internal override void EndNonLayout() {
            base.EndNonLayout();
            DoScrollbars();
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
                                ? mousePos.y / ContentRectInternal.yMax
                                : mousePos.x / ContentRectInternal.xMax;
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
        private void DoScrollbars() {
            if (_disableScrollbars) return;
            var current = Event.current;
            var eventType = current.type;

            var actualContentRect = ContentRectInternal;

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