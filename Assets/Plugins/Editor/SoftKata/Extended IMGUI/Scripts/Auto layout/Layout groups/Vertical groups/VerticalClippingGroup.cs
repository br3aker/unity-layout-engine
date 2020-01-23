using System;
using System.Collections.Specialized;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalClippingGroup : VerticalLayoutGroupBase {
            private Vector2 _worldPosition;

            private float _lastRectActualHeight;
            private float _lastRectActualY;

            public VerticalClippingGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) {}

            internal override GroupRectData GetGroupRect(float height, float width) {
                var currentEntryPosition = NextEntryPosition;
                var rect = GetRect(height, width);
                rect.x = 0f;
                rect.y = _lastRectActualY;
//                rect.height = _lastRectActualHeight;

                var groupRectData = new GroupRectData {
                    Rect = rect,
                    OffsetFromParentRect = currentEntryPosition
                };
                return groupRectData;
            }

            internal override void RetrieveLayoutData(EventType currentEventType) {
//                RegisterDebugData();
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    
                    if (Parent != null) {
                        var rectData = Parent.GetGroupRect(TotalRequestedHeight, TotalRequestedWidth);
                        FullContainerRect = rectData.Rect;
                        NextEntryPosition = rectData.OffsetFromParentRect - FullContainerRect.position;
                    }
                    else {
                        FullContainerRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                    }
                    
                    IsGroupValid = FullContainerRect.IsValid();

                    if (IsGroupValid) {
                        FullContainerRect = Margin.Remove(FullContainerRect);
                        _worldPosition = FullContainerRect.position;
                        FullContainerRect = Padding.Remove(FullContainerRect);

                        GUI.BeginClip(FullContainerRect);
                        FullContainerRect.position = Vector2.zero;
                        MaxAllowedWidth = FullContainerRect.width;

                        return;
                    }
                }
                
//                UpdateDebugData();

                // Nested groups should be banished exactly here at non-layout layout data pull
                // This would ensure 2 things:
                // 1. Entries > 0 because this is called after PushLayoutRequest() which checks that
                // 2. Parent group returned Valid rect
                if (Parent != null) {
                    Parent.EntriesCount -= _childrenCount + 1;
                }
                LayoutEngine.ScrapGroups(_childrenCount);
            }
            
            protected override Rect GetActualRect(float x, float y, float height, float width) {
                _lastRectActualHeight = height;
                _lastRectActualY = y;
                
                var entryBottom = y + height;
                if (y < FullContainerRect.y) {
                    var uselessTop = Mathf.Abs(FullContainerRect.y - y);
                    _lastRectActualHeight -= uselessTop;
                    _lastRectActualY += uselessTop;
                }
                else if (entryBottom > FullContainerRect.yMax) {
                    var uselessBottom = Mathf.Abs(entryBottom - FullContainerRect.yMax);
                    _lastRectActualHeight -= uselessBottom;
                }

                return new Rect(x, y, Mathf.Min(width, FullContainerRect.width), Mathf.Min(height, FullContainerRect.height));
            }

            internal sealed override void EndGroup(EventType currentEventType) {
                if (IsGroupValid) {
                    GUI.EndClip();
                    FullContainerRect = Padding.Add(FullContainerRect);
                    FullContainerRect.position = _worldPosition;
                    EndGroupRoutine(currentEventType);
                }
            }
        }
    }
}