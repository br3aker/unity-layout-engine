using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginScrollGroup(LayoutGroupBase group){
            if (Event.current.type == EventType.Layout){
                group.ResetLayout();
                return RegisterForLayout(group);
            }

            var currentGroup = RetrieveNextGroup() as ScrollGroup;
            currentGroup.CalculateScrollContainerSize();
            return currentGroup.IsGroupValid;
        }
        public static bool BeginScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars, GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout)
                return RegisterForLayout(new ScrollGroup(containerSize, scrollPos, disableScrollbars, modifier, style));

            var currentGroup = RetrieveNextGroup() as ScrollGroup;
            currentGroup.CalculateScrollContainerSize();
            return currentGroup.IsGroupValid;
        }
        public static bool BeginScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars = false, GroupModifier modifier = GroupModifier.None) {
            return BeginScrollGroup(containerSize, scrollPos, disableScrollbars, modifier, ExtendedEditorGUI.LayoutResources.ScrollGroup);
        }
        
        public static Vector2 EndScrollGroup() {
            var group = EndLayoutGroup<ScrollGroup>();
            group.DoScrollGroupEndRoutine();
            return group.ScrollPos;
        }

        internal class ScrollGroup : VerticalClippingGroup {
            private const float MinimalScrollbarSizeMultiplier = 0.07f;

            private float _actualContentWidth;
    

            private readonly Color _backgroundColor;

            public Vector2 ContainerSize;

            private Vector2 _containerToActualSizeRatio;
            private readonly Vector2 _horizontalScrollbarDelta;

            // horizontal scrollbar settings
            private readonly float _horizontalScrollBarHeight;
            private bool _needsHorizontalScroll;
            private int _horizontalScrollId;

            // vertical scrollbar settings
            private readonly float _verticalScrollBarWidth;
            private bool _needsVerticalScroll;
            private int _verticalScrollId;
            
            private readonly Color _scrollbarColor;
            private readonly Vector2 _verticalScrollbarDelta;

            private Vector2 _scrollPos;
            public Vector2 ScrollPos {get => _scrollPos; set => _scrollPos = value;}
            public float ScrollPosX {get => _scrollPos.x; set => _scrollPos.x = value;}
            public float ScrollPosY {get => _scrollPos.y; set => _scrollPos.y = value;}

            private bool _disableScrollbars;

            public ScrollGroup(Vector2 containerSize, Vector2 scrollPos, bool disableScrollbars, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                ContainerSize = containerSize;

                // Scroll settings
                _scrollPos = scrollPos;

                _disableScrollbars = disableScrollbars;

                var overflow = style.overflow;
                _verticalScrollBarWidth = overflow.right;
                _verticalScrollbarDelta = new Vector2(_verticalScrollBarWidth - overflow.left, 0f);

                _horizontalScrollBarHeight = overflow.bottom;
                _horizontalScrollbarDelta = new Vector2(0f, _horizontalScrollBarHeight - overflow.top);

                // Colors
                _backgroundColor = style.normal.textColor;
                _scrollbarColor = style.onNormal.textColor;
            }

            protected override void ModifyContainerSize() {
                base.ModifyContainerSize();
                // GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                // GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);

                // Same can be done with content height
                _actualContentWidth = RequestedWidth;
                RequestedWidth = ContainerSize.x;
                
                if (ContainerSize.y > 0f && RequestedHeight > ContainerSize.y) {
                    _needsVerticalScroll = true;

                    _containerToActualSizeRatio.y = ContainerSize.y / RequestedHeight;
                    NextEntryPosition.y += Mathf.Lerp(0f, ContainerSize.y - RequestedHeight, _scrollPos.y);

                    RequestedHeight = ContainerSize.y;
                }
            }

            internal void CalculateScrollContainerSize() {
                _verticalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);
                _horizontalScrollId = GUIUtility.GetControlID(LayoutGroupControlIdHint, FocusType.Passive);

                var allowedWidth = ContainerRect.width;
                var actualWidth = _actualContentWidth > 0 ? _actualContentWidth : _visibleContentWidth;

                if (actualWidth > allowedWidth) {
                    _needsHorizontalScroll = true;

                    NextEntryPosition.x += Mathf.Lerp(0, allowedWidth - actualWidth, _scrollPos.x);
                    _containerToActualSizeRatio.x = allowedWidth / actualWidth;
                }
                else {
                    _needsHorizontalScroll = false;
                    ContainerRect.width = actualWidth;
                }
            }

            internal void DoScrollGroupEndRoutine() {
                if (!IsGroupValid || _disableScrollbars) return;
                var current = Event.current;
                var eventType = current.type;

                var actualContentRect = ContainerRect;
                ContainerRect = Margin.Add(Border.Add(Padding.Add(ContainerRect)));

                if (_needsVerticalScroll) {
                    var scrollbarHeight = Mathf.Max(actualContentRect.height * _containerToActualSizeRatio.y,
                        actualContentRect.height * MinimalScrollbarSizeMultiplier);
                    var scrollMovementLength = actualContentRect.height - scrollbarHeight;

                    if (eventType == EventType.ScrollWheel && ContainerRect.Contains(current.mousePosition)) {
                        current.Use();
                        GUIUtility.keyboardControl = 0;

                        _scrollPos.y = Mathf.Clamp01(_scrollPos.y + current.delta.y / scrollMovementLength);
                        return;
                    }

                    var verticalScrollPos = actualContentRect.xMax + Padding.right - _verticalScrollBarWidth;

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
                            scrollMovementLength,
                            _verticalScrollbarDelta
                        );
                }

                if (_needsHorizontalScroll) {
                    var scrollBarWidth = Mathf.Max(actualContentRect.width * _containerToActualSizeRatio.x,
                        actualContentRect.width * MinimalScrollbarSizeMultiplier);
                    var scrollMovementLength = actualContentRect.width - scrollBarWidth;

                    var horizontalScrollPos = actualContentRect.yMax - _horizontalScrollBarHeight + Padding.bottom;

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
                            scrollMovementLength,
                            _horizontalScrollbarDelta
                        );
                }
            }

            private float DoGenericScrollbar(Event currentEvent, float scrollPos, Rect scrollbarRect,
                Rect backgroundRect, int controlId, bool verticalBar, float totalMovementLength,
                Vector2 renderingDelta) {
                switch (currentEvent.type) {
                    case EventType.MouseDown:
                        var mousePos = currentEvent.mousePosition;
                        if (backgroundRect.Contains(mousePos)) {
                            currentEvent.Use();
                            GUIUtility.keyboardControl = 0;
                            GUIUtility.hotControl = controlId;

                            if (!scrollbarRect.Contains(mousePos))
                                scrollPos = verticalBar
                                    ? mousePos.y / ContainerRect.yMax
                                    : mousePos.x / ContainerRect.xMax;
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
                        if (_backgroundColor.a > 0f) EditorGUI.DrawRect(backgroundRect, _backgroundColor);

                        // Actual scrollbar
                        EditorGUI.DrawRect(scrollbarRect, _scrollbarColor);

                        break;
                }

                return scrollPos;
            }
        }
    }
}