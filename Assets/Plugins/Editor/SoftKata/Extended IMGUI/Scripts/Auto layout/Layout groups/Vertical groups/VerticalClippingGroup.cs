using System;
using System.Collections.Specialized;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalClippingGroup : VerticalGroup {
            public VerticalClippingGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) {}

            internal override void RetrieveLayoutData(EventType currentEventType) {
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    
                    if (Parent != null) {
                        var rectData = Parent.GetGroupRect(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = rectData.ClippedRect;
                        ContentRect = rectData.FullContentRect;
                    }
                    else {
                        VisibleAreaRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                        ContentRect = VisibleAreaRect;
                    }
                    
                    IsGroupValid = VisibleAreaRect.IsValid();

                    if (IsGroupValid) {
                        ContentRect = Padding.Remove(Margin.Remove(ContentRect));

                        VisibleAreaRect = Utility.RectIntersection(ContentRect, VisibleAreaRect);
                        NextEntryPosition += (ContentRect.position - VisibleAreaRect.position);
                        GUI.BeginClip(VisibleAreaRect);
                        VisibleAreaRect.position = Vector2.zero;

                        return;
                    }
                }

                // Nested groups should be banished exactly here at non-layout layout data pull
                // This would ensure 2 things:
                // 1. Entries > 0 because this is called after PushLayoutRequest() which checks that
                // 2. Parent group returned Valid rect
                if (Parent != null) {
                    Parent.EntriesCount -= _childrenCount + 1;
                }
                LayoutEngine.ScrapGroups(_childrenCount);
            }

            internal sealed override void EndGroup(EventType currentEventType) {
                if (IsGroupValid) {

                    GUI.EndClip();
                    EndGroupRoutine(currentEventType);
                }
            }
        }
    }
}