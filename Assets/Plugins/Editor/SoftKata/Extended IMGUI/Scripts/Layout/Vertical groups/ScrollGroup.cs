using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.UnityEditor {
    public class ScrollGroup : VerticalGroup {
        private const float MinimalScrollbarSizeMultiplier = 0.07f;

        private readonly Color _backgroundColor;

        private Vector2 _containerSize;
        public Vector2 ContainerSize {
            set {
                if(_containerSize != value) {
                    _containerSize = value;
                    MarkLayoutDirty();
                }
            }
        }
        private Vector2 _containerToActualSizeRatio;

        // horizontal scrollbar settings
        private readonly int _horizontalScrollBarHeight;
        private readonly int _horizontalScrollBarPadding;
        private readonly int _bottomMargin;
        private bool _needsHorizontalScroll;
        private int _horizontalScrollId;

        // vertical scrollbar settings
        private readonly int _verticalScrollBarWidth;
        private readonly int _verticalScrollBarPadding;
        private readonly int _rightMargin;
        private bool _needsVerticalScroll;
        private int _verticalScrollId;

        private readonly Color _scrollbarColor;

        private float _verticalScroll;
        public float VerticalScroll { 
            get => _verticalScroll;
            set {
                _verticalScroll = value;
                _scrollContentOffset.y = 
                    Mathf.Lerp(0, _invisibleAreaSize.y, value);
            } 
        }

        private float _horizontalScroll;
        public float HorizontalScroll { 
            get => _horizontalScroll;
            set {
                _horizontalScroll = value;
                _scrollContentOffset.x = 
                    Mathf.Lerp(0, _invisibleAreaSize.x, value);
            }
        }

        private bool _disableScrollbars;

        private bool _isFirstLayoutBuild = true;

        private Vector2 _invisibleAreaSize;
        private Vector2 _scrollContentOffset;

        private UnityAction RepaintView;

        public ScrollGroup(Vector2 containerSize, bool disableScrollbars, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            RepaintView = ExtendedEditor.CurrentView.Repaint;

            Clip = true;

            ContainerSize = containerSize;

            // Scroll settings
            var margin = style.margin;
            _rightMargin = margin.right;
            _bottomMargin = margin.bottom;

            if(_disableScrollbars = disableScrollbars) return;

            var padding = style.padding;
            var border = style.border;
            
            // Vertical
            _verticalScrollBarWidth = border.right;
            _verticalScrollBarPadding = padding.right;

            // Horizontal
            _horizontalScrollBarHeight = border.bottom;
            _horizontalScrollBarPadding = padding.bottom;

            // Colors
            _backgroundColor = style.normal.textColor;
            _scrollbarColor = style.onNormal.textColor;
        }
        public ScrollGroup(Vector2 containerSize, bool disableScrollbars = false, bool ignoreConstaints = false)
            : this(containerSize, disableScrollbars, ExtendedEditor.Resources.ScrollGroup, ignoreConstaints) {}

        protected override float CalculateAutomaticContentWidth() {
            return (_containerSize.x > 0 ? _containerSize.x : AvailableWidth) - TotalOffset.horizontal;
        }

        protected override void PreLayoutRequest() {
            // Resetting total offset to if scrollbars are not used
            TotalOffset.right = _rightMargin;
            TotalOffset.bottom = _bottomMargin;

            // Adding extra content size
            EntriesRequestedSize.y += SpaceBetweenEntries * (EntriesCount - 1);

            // Calculating container visible area size
            var visibleAreaSize = new Vector2(
                (_containerSize.x > 0 ? _containerSize.x : AvailableWidth) - TotalOffset.horizontal,
                _containerSize.y - TotalOffset.vertical
            );

            _invisibleAreaSize = visibleAreaSize - EntriesRequestedSize;

            // 1st pass - checking if we actually need scrollbars
            if(!_disableScrollbars) {
                if(EntriesRequestedSize.x > visibleAreaSize.x) {
                    var horizontalBarExtraHeight = _horizontalScrollBarPadding + _horizontalScrollBarHeight;
                    TotalOffset.bottom += horizontalBarExtraHeight;
                    visibleAreaSize.y -= horizontalBarExtraHeight;
                }
                if(EntriesRequestedSize.y > visibleAreaSize.y) {
                    var verticalBarExtraWidth = _verticalScrollBarPadding + _verticalScrollBarWidth;
                    TotalOffset.right += verticalBarExtraWidth;
                    visibleAreaSize.x -= verticalBarExtraWidth;
                }
            }

            // 2nd pass - calculations based on 1st pass
            if(_needsHorizontalScroll = EntriesRequestedSize.x > visibleAreaSize.x) {
                _containerToActualSizeRatio.x = visibleAreaSize.x / EntriesRequestedSize.x;

                EntriesRequestedSize.x = visibleAreaSize.x;
            }
            if(_needsVerticalScroll = EntriesRequestedSize.y > visibleAreaSize.y) {
                _containerToActualSizeRatio.y = visibleAreaSize.y / EntriesRequestedSize.y;
                
                EntriesRequestedSize.y = visibleAreaSize.y;
            }

            // Applying offsets to actual group rect
            EntriesRequestedSize.x += TotalOffset.horizontal;
            EntriesRequestedSize.y += TotalOffset.vertical;
        }

        internal override bool BeginNonLayout() {
            if(base.BeginNonLayout()) {
                // requesting ids for scrollbars
                _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);

                // scroll content offset
                NextEntryPosition += _scrollContentOffset;
                return true;
            }
            return false;
        }
        internal override void EndNonLayout() {
            base.EndNonLayout();

            var currentEvent = Event.current;
            var currentEventType = currentEvent.type;

            if(_needsHorizontalScroll) {
                DoHorizontalScroll(currentEvent);
            }

            if(_needsVerticalScroll) {
                DoVerticalScroll(currentEvent, currentEventType);
            }
            else if(_isFirstLayoutBuild) {
                _isFirstLayoutBuild = false;
                MarkLayoutDirty();
                RepaintView();
            }
        }


        private float DoScroll(Event evt, float pos, Rect thumbRect, Rect scrollAreaRect, 
            int controlId, float totalMovementLength, float relativeMousePos, float dragDelta) {
            switch (evt.type) {
                case EventType.MouseDown:
                    var mousePos = evt.mousePosition;
                    if (scrollAreaRect.Contains(mousePos)) {
                        evt.Use();
                        GUIUtility.keyboardControl = 0;
                        GUIUtility.hotControl = controlId;

                        if (!thumbRect.Contains(mousePos)){
                            pos = relativeMousePos;
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId) {
                        evt.Use();
                        GUIUtility.hotControl = 0;
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId) {
                        evt.Use();
                        pos = Mathf.Clamp01(pos + dragDelta / totalMovementLength);
                    }
                    break;
                case EventType.Repaint:
                    // Background
                    if (_backgroundColor.a > 0f) global::UnityEditor.EditorGUI.DrawRect(scrollAreaRect, _backgroundColor);

                    // Actual scrollbar
                    global::UnityEditor.EditorGUI.DrawRect(thumbRect, _scrollbarColor);

                    break;
            }

            return pos;
        }
    
        private void DoVerticalScroll(Event evt, EventType evtType) {
            var actualContentRect = ContentRectInternal;

            var scrollbarHeight = Mathf.Max(actualContentRect.height * _containerToActualSizeRatio.y,
                actualContentRect.height * MinimalScrollbarSizeMultiplier);
            var scrollMovementLength = actualContentRect.height - scrollbarHeight;

            if (evtType == EventType.ScrollWheel && TotalOffset.Add(actualContentRect).Contains(evt.mousePosition)) {
                evt.Use();
                GUIUtility.keyboardControl = 0;

                VerticalScroll = Mathf.Clamp01(VerticalScroll + evt.delta.y / scrollMovementLength);
                return;
            }

            var verticalScrollPos = actualContentRect.xMax + _verticalScrollBarPadding;

            var verticalScrollbarRect = new Rect(
                verticalScrollPos,
                actualContentRect.y + scrollMovementLength * VerticalScroll,
                _verticalScrollBarWidth,
                scrollbarHeight
            );

            var verticalScrollbarBackgroundRect = new Rect(
                verticalScrollPos,
                actualContentRect.y,
                _verticalScrollBarWidth,
                actualContentRect.height
            );

            VerticalScroll =
                DoScroll(
                    evt,
                    VerticalScroll,
                    verticalScrollbarRect, verticalScrollbarBackgroundRect,
                    _verticalScrollId,
                    scrollMovementLength,
                    evt.mousePosition.y / ContentRectInternal.yMax,
                    evt.delta.y
                );
        }
        private void DoHorizontalScroll(Event evt) {
            var actualContentRect = ContentRectInternal;

            var scrollBarWidth = Mathf.Max(actualContentRect.width * _containerToActualSizeRatio.x,
                actualContentRect.width * MinimalScrollbarSizeMultiplier);
            var scrollMovementLength = actualContentRect.width - scrollBarWidth;

            var horizontalScrollPos = actualContentRect.yMax + _horizontalScrollBarPadding;

            var horizontalScrollbarRect = new Rect(
                actualContentRect.x + scrollMovementLength * HorizontalScroll,
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

            HorizontalScroll =
                DoScroll(
                    evt,
                    HorizontalScroll,
                    horizontalScrollbarRect,
                    horizontalScrollbarBackgroundRect,
                    _horizontalScrollId,
                    scrollMovementLength,
                    evt.mousePosition.x / ContentRectInternal.xMax,
                    evt.delta.x
                );
        }
    }
}