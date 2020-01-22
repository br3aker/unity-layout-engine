using System.Collections.Specialized;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalClippingGroup : VerticalLayoutGroupBase {
            private Vector2 _worldPosition;
            
            public VerticalClippingGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) {}

            internal override void RetrieveLayoutData(EventType currentEventType) {
                RegisterDebugData();
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    FullContainerRect = Parent?.GetRect(TotalRequestedHeight, TotalRequestedWidth) ?? LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                    
                    IsGroupValid = FullContainerRect.IsValid();

                    if (IsGroupValid) {
                        FullContainerRect = Margin.Remove(FullContainerRect);
                        _worldPosition = FullContainerRect.position;
                        FullContainerRect = Padding.Remove(FullContainerRect);
                        GUI.BeginClip(FullContainerRect);
                        FullContainerRect.position = Vector2.zero;
                        MaxAllowedWidth = FullContainerRect.width;
                        
                        EntryRectBorders = GetContentBorderValues();

                        return;
                    }
                }
                
                UpdateDebugData();

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
//                if (NextEntryPosition.y + height < FullContainerRect.y || NextEntryPosition.y > FullContainerRect.yMax) {
//                    return InvalidRect;
//                }

//                return new Rect(NextEntryPosition.x, NextEntryPosition.y, width, height);

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