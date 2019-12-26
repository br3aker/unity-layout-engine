using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static void BeginVerticalScroll(float height, float scrollPos) {
            BeginVerticalScroll(height, scrollPos, ExtendedEditorGUI.Resources.LayoutGroup.VerticalScrollGroup);
        }
        public static void BeginVerticalScroll(float height, float scrollPos, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroupBase group;
            if (eventType == EventType.Layout) {
                group = new VerticalScrollGroup(height, scrollPos, TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;
        } 
        public static float EndVerticalScroll() {
            var rawTopGroup = EndLayoutGroup();
            var topVerticalGroup = rawTopGroup as VerticalScrollGroup;
            if (topVerticalGroup == null) {
                throw new Exception($"Group ending method [Vertical Scroll] mismatch with actual registered top group: [{rawTopGroup.GetType()}]");
            }

            return topVerticalGroup.ScrollPos;
        }

        private class VerticalScrollGroup : VerticalLayoutGroup {
            private static readonly int VerticalScrollGroupHash = nameof(VerticalScrollGroup).GetHashCode();
            private readonly int groupId;
            
            private readonly float _height;

            public float ScrollPos;

            private float _containerToContentHeightRatio;
            private float _sliderHeight;
            
            private Rect _sliderRect;
            private Rect _sliderBackgroundRect;

            private float _sliderWidth;
            

            public VerticalScrollGroup(float height, float scrollPos, LayoutGroupBase parent, GUIStyle style) : base(parent, style) {
                _height = height;

                groupId = GUIUtility.GetControlID(VerticalScrollGroupHash, FocusType.Passive);
                
                ScrollPos = scrollPos;
                _sliderWidth = style.border.right;
            }

            protected override void PushLayoutEntries() {
                FullRect = Parent?.GetRect(_height) ?? RequestIndentedRect(_height);
                _nextEntryY = Mathf.Lerp(0, _height - LayoutData.TotalHeight, ScrollPos);

                    
                _containerToContentHeightRatio = _height / LayoutData.TotalHeight;
                _sliderHeight = _height * _containerToContentHeightRatio;

                _sliderRect = new Rect(
                    FullRect.width - _sliderWidth,
                    (_height - _sliderHeight) * ScrollPos,
                    _sliderWidth,
                    _sliderHeight
                );
                    
                _sliderBackgroundRect = new Rect(
                    FullRect.width - _sliderWidth,
                    0f,
                    _sliderWidth,
                    _height
                );
            }
            
            protected override void EndGroupRoutine() {
                // Post group events handling
                switch (_eventType) {
                    case EventType.MouseDown:
                        if (GUIUtility.hotControl == 0) {
                            if (_sliderRect.Contains(Event.current.mousePosition)) {
                                GUIUtility.hotControl = groupId;
                                GUIUtility.keyboardControl = 0;

                                Event.current.Use();
                            }
                            else if (_sliderBackgroundRect.Contains(Event.current.mousePosition)) {
                                ScrollPos = Event.current.mousePosition.y / _height; 
                                
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
                            var delta = Event.current.delta.y;
                            var currentY = Mathf.Clamp(_sliderRect.y + delta, 0f, _height - _sliderHeight);

                            ScrollPos = currentY / (_height - _sliderHeight);
                        }
                        break;
                    case EventType.ScrollWheel:
                        if (FullRect.Contains(Event.current.mousePosition)) {
                            GUIUtility.keyboardControl = 0;
                            ScrollPos = Mathf.Clamp01(ScrollPos + Event.current.delta.y / _height);
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