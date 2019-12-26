using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static void BeginHorizontalScroll(float elemWidth, float scrollPos) {
            BeginHorizontalScroll(elemWidth, scrollPos, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalScrollGroup);
        }
        public static void BeginHorizontalScroll(float elemWidth, float scrollPos, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroupBase group;
            if (eventType == EventType.Layout) {
                group = new HorizontalScrollGroup(elemWidth, scrollPos, TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;
        }
        public static float EndHorizontalScroll() {
            var rawTopGroup = EndLayoutGroup();
            var topHorizontalGroup = rawTopGroup as HorizontalScrollGroup;
            if (topHorizontalGroup == null) {
                throw new Exception($"Group ending method [Horizontal Scroll] mismatch with actual registered top group: [{rawTopGroup.GetType()}]");
            }

            return topHorizontalGroup.ScrollPos;
        }

        private class HorizontalScrollGroup : HorizontalLayoutGroup {
            private static readonly int HorizontalScrollGroupHash = nameof(HorizontalScrollGroup).GetHashCode();
            private readonly int groupId;
            
            private float _elemWidth;
            
            public float ScrollPos;

            private float _containerToContentWidthRatio;
            private float _sliderWidth;
            
            private Rect _sliderRect;
            private Rect _sliderBackgroundRect;

            private float _sliderHeight;

            public HorizontalScrollGroup(float elemWidth, float scrollPos, LayoutGroupBase parent, GUIStyle style) : base(parent, style) {
                _elemWidth = elemWidth;
                
                groupId = GUIUtility.GetControlID(HorizontalScrollGroupHash, FocusType.Passive);
                
                ScrollPos = scrollPos;
                _sliderHeight = style.border.bottom;
            }

            protected override void PushLayoutEntries() {
                var contentWidth = _elemWidth * LayoutData.Count + _contentHorizontalGap * (LayoutData.Count - 1);
                FullRect = Parent?.GetRect(LayoutData.TotalHeight + _sliderHeight) ?? RequestIndentedRect(LayoutData.TotalHeight+ _sliderHeight);
                _nextEntryX = Mathf.Lerp(0, FullRect.width - contentWidth, ScrollPos);
                _entryWidth = _elemWidth;
                    
                _containerToContentWidthRatio = FullRect.width  / contentWidth;
                _sliderWidth = FullRect.width * _containerToContentWidthRatio;

                _sliderRect = new Rect(
                    (FullRect.width - _sliderWidth) * ScrollPos,
                    FullRect.height - _sliderHeight,
                    _sliderWidth,
                    _sliderHeight
                );
                    
                _sliderBackgroundRect = new Rect(
                    0f,
                    FullRect.height - _sliderHeight,
                    FullRect.width,
                    _sliderHeight
                );
            }
            
            protected override void EndGroupRoutine() {
                switch (_eventType) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (_sliderRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = groupId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (_sliderBackgroundRect.Contains(Event.current.mousePosition)) {
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
                            var currentX = Mathf.Clamp((_sliderRect.x + delta) - FullRect.x, 0f, FullRect.width - _sliderWidth);

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
                        EditorGUI.DrawRect(_sliderBackgroundRect, Color.black);
                        EditorGUI.DrawRect(_sliderRect, Color.magenta);
                        break;
                }
            }
        }
    }
}