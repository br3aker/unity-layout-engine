using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalScrollGroup : HorizontalLayoutGroupBase {
            private static readonly int HorizontalScrollGroupHash = nameof(HorizontalScrollGroup).GetHashCode();
            
            private float _containerWidth;
            
            internal float ScrollPos;

            private bool _needsScroll;
            
            private float _sliderWidth;
            private float _sliderHeight;

            public HorizontalScrollGroup(float width, float scrollPos, GUIStyle style) : base(style) {
                _containerWidth = width;
                ScrollPos = scrollPos;
                
                _sliderHeight = style.border.bottom;
            }

            protected override void CalculateLayoutData() {
                if (CurrentEventType == EventType.Layout) {
                    TotalWidth += ContentOffset * (EntriesCount - 1);
                    
                    if (TotalWidth > _containerWidth) {
                        _needsScroll = true;
                        NextEntryX = Mathf.Lerp(0f, _containerWidth - TotalWidth, ScrollPos);
                        
                        float _containerToContentWidthRatio = _containerWidth / TotalWidth;
                        _sliderWidth = _containerWidth * _containerToContentWidthRatio;

                        // this action is not very clear, TotalWidth is used at layout entries data registration
                        // probably needs to be renamed
                        TotalWidth = _containerWidth;
                    }
                }
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryX + width < 0 || NextEntryX > TotalWidth) {
                    return InvalidRect;
                }
                return new Rect(
                    NextEntryX,
                    NextEntryY,
                    width,
                    height
                );
            }

            protected override void EndGroupRoutine(EventType currentEventType) {
                if (!_needsScroll) return;
                
                var groupId = GUIUtility.GetControlID(HorizontalScrollGroupHash, FocusType.Passive);
                
                float sliderVerticalPosition = FullRect.height - _sliderHeight;
                
                var sliderRect = new Rect(
                    (FullRect.width - _sliderWidth) * ScrollPos,
                    sliderVerticalPosition,
                    _sliderWidth,
                    _sliderHeight
                );
                    
                var sliderBackgroundRect = new Rect(
                    0f,
                    sliderVerticalPosition,
                    FullRect.width,
                    _sliderHeight
                );
                
                switch (currentEventType) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (sliderRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = groupId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (sliderBackgroundRect.Contains(Event.current.mousePosition)) {
                                ScrollPos = Event.current.mousePosition.x / FullRect.width;
                                
                                GUIUtility.hotControl = groupId;
                                GUIUtility.keyboardControl = 0;
                                
                                Event.current.Use();
                            }
                        }

                        break;
                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == groupId) {
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == groupId) {
                            var delta = Event.current.delta.x;
                            var currentX = Mathf.Clamp((sliderRect.x + delta) - FullRect.x, 0f, FullRect.width - _sliderWidth);

                            ScrollPos = currentX / (FullRect.width - _sliderWidth);
                        }
                        break;
                    case EventType.ScrollWheel:
                        if (FullRect.Contains(Event.current.mousePosition)) {
                            GUIUtility.keyboardControl = 0;
                            ScrollPos = Mathf.Clamp01(ScrollPos + Event.current.delta.y / FullRect.width);
                            Event.current.Use();
                        }
                        break;
                    case EventType.Repaint:
                        EditorGUI.DrawRect(sliderBackgroundRect, Color.black);
                        EditorGUI.DrawRect(sliderRect, Color.magenta);
                        break;
                }
            }
        }

        public static bool BeginHorizontalScrollGroup(float width, float scrollValue, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalScrollGroup(width, scrollValue, style);
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
        public static bool BeginHorizontalScrollGroup(float width, float scrollValue) {
            return BeginHorizontalScrollGroup(width, scrollValue, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalScrollGroup);
        }

        public static float EndHorizontalScrollGroup() {
            var lastGroup = EndLayoutGroup() as HorizontalScrollGroup;
            if (lastGroup == null) {
                throw new Exception("Layout group type mismatch");
            }

            return lastGroup.ScrollPos;
        }
    }
}