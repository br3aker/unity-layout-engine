using System.Collections.Specialized;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalClippingGroup : VerticalLayoutGroupBase {
            public VerticalClippingGroup(bool discardMargin, GUIStyle style) : base(discardMargin, style) {}

            internal override void RetrieveLayoutData(EventType currentEventType) {
                //RegisterDebugData();
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    FullContainerRect = Parent?.GetRect(TotalRequestedHeight, TotalRequestedWidth) ?? LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                    IsGroupValid = FullContainerRect.IsValid();

                    if (IsGroupValid) {
                        FullContainerRect = Padding.Remove(Margin.Remove(FullContainerRect));
                        var color = Color.black;
                        color.a = 0.5f;
                        EditorGUI.DrawRect(FullContainerRect, color);
                        GUI.BeginClip(FullContainerRect);
                        FullContainerRect.position = Vector2.zero;
                        
                        return;
                    }
                }
                
                //UpdateDebugData();

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
                    FullContainerRect = Padding.Add(FullContainerRect);
                    EndGroupRoutine(currentEventType);
                }
            }
        }
    }
}