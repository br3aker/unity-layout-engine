using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.UnityEditor {
    public class ScrollGroup : VerticalGroup {
        private const float _minimalScrollBarSize = 35;


        private readonly GUIStyle _thumbStyle;

        private Vector2 _containerSize;
        public Vector2 ContainerSize {
            set {
                if(_containerSize != value) {
                    _containerSize = value;
                    MarkLayoutDirty();
                }
            }
        }

        // horizontal scrollbar settings
        private float _horizontalScrollBarWidth;
        private float _horizontalScrollBarHeight;
        private readonly int _horizontalScrollBarPadding;
        private readonly int _bottomPadding;
        private bool _needsHorizontalScroll;
        private int _horizontalScrollId;

        // vertical scrollbar settings
        private float _verticalScrollBarHeight;
        private float _verticalScrollBarWidth;
        private readonly int _verticalScrollBarPadding;
        private readonly int _rightPadding;
        private bool _needsVerticalScroll;
        private int _verticalScrollId;

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

        private readonly UnityAction RepaintView;


        public ScrollGroup(Vector2 containerSize, bool disableScrollbars, GUIStyle style, GUIStyle thumbStyle, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            RepaintView = ExtendedEditor.CurrentView.Repaint;

            Clip = true;

            ContainerSize = containerSize;

            // Scroll settings
            var containerPadding = style.padding;
            _rightPadding = containerPadding.right;
            _bottomPadding = containerPadding.bottom;

            if(_disableScrollbars = disableScrollbars) return;

            var scrollbarOffset = thumbStyle.padding;
            var scrollbarSize = thumbStyle.margin;
            
            // Vertical
            _verticalScrollBarWidth = scrollbarSize.right;
            _verticalScrollBarPadding = scrollbarOffset.right;

            // Horizontal
            _horizontalScrollBarHeight = scrollbarSize.bottom;
            _horizontalScrollBarPadding = scrollbarOffset.bottom;

            // Thumb renderer style
            _thumbStyle = thumbStyle;
        }
        public ScrollGroup(Vector2 containerSize, bool disableScrollbars = false, bool ignoreConstaints = false)
            : this(containerSize, disableScrollbars, ExtendedEditor.Resources.ScrollGroup, ExtendedEditor.Resources.ScrollGroupThumb, ignoreConstaints) {}

        protected override float CalculateAutomaticContentWidth() {
            return (_containerSize.x > 0 ? _containerSize.x : AvailableWidth) - TotalOffset.horizontal;
        }

        protected override void PreLayoutRequest() {
            // Resetting total offset to if scrollbars are not used
            TotalOffset.right = _rightPadding;
            TotalOffset.bottom = _bottomPadding;

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
                    TotalOffset.bottom += Mathf.RoundToInt(horizontalBarExtraHeight);
                    visibleAreaSize.y -= horizontalBarExtraHeight;
                }
                if(EntriesRequestedSize.y > visibleAreaSize.y) {
                    var verticalBarExtraWidth = _verticalScrollBarPadding + _verticalScrollBarWidth;
                    TotalOffset.right += Mathf.RoundToInt(verticalBarExtraWidth);
                    visibleAreaSize.x -= verticalBarExtraWidth;
                }
            }

            // 2nd pass - calculations based on 1st pass
            if(_needsHorizontalScroll = EntriesRequestedSize.x > visibleAreaSize.x) {
                var containerToContentRatio = visibleAreaSize.x / EntriesRequestedSize.x;
                _horizontalScrollBarWidth = 
                    Mathf.Max(visibleAreaSize.x * containerToContentRatio, _minimalScrollBarSize);

                EntriesRequestedSize.x = visibleAreaSize.x;
            }
            if(_needsVerticalScroll = EntriesRequestedSize.y > visibleAreaSize.y) {
                var containerToContentRatio = visibleAreaSize.y / EntriesRequestedSize.y;
                _verticalScrollBarHeight = 
                    Mathf.Max(visibleAreaSize.y * containerToContentRatio, _minimalScrollBarSize);
                
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

            if(_isFirstLayoutBuild) {
                _isFirstLayoutBuild = false;
                MarkLayoutDirty();
                RepaintView();
            }
        }


        private float DoScroll(Event evt, float pos, Rect thumbRect, Rect fullRect, 
            int controlId, float totalMovementLength, float relativeMousePos, float dragDelta) {
            switch (evt.type) {
                case EventType.MouseDown:
                    var mousePos = evt.mousePosition;
                    if (fullRect.Contains(mousePos)) {
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
                    _thumbStyle.Draw(fullRect, false, false, false, false);

                    // Actual scrollbar
                    _thumbStyle.Draw(thumbRect, false, false, true, false);

                    break;
            }

            return pos;
        }
    
        private void DoVerticalScroll(Event evt, EventType evtType) {
            var actualContentRect = ContentRectInternal;

            var scrollMovementLength = actualContentRect.height - _verticalScrollBarHeight;

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
                _verticalScrollBarHeight
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

            var scrollMovementLength = actualContentRect.width - _horizontalScrollBarWidth;

            var horizontalScrollPos = actualContentRect.yMax + _horizontalScrollBarPadding;

            var horizontalScrollbarRect = new Rect(
                actualContentRect.x + scrollMovementLength * HorizontalScroll,
                horizontalScrollPos,
                _horizontalScrollBarWidth,
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