using System;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
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
                        var rectData = Parent.GetGroupRectData(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = rectData.VisibleRect;
                        ContentRect = rectData.FullContentRect;
                    }
                    else {
                        ContentRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = ContentRect;
                    }
                    
                    IsGroupValid = VisibleAreaRect.IsValid();

                    if (IsGroupValid) {
                        ContentRect = Padding.Remove(Border.Remove(Margin.Remove(ContentRect)));
                        VisibleAreaRect = Utility.RectIntersection(VisibleAreaRect, ContentRect);
                        
                        NextEntryPosition += ContentRect.position - VisibleAreaRect.position;


//                        if (GetType() == typeof(ScrollGroup)) {
//                            EditorGUI.DrawRect(Padding.Add(Border.Add(Margin.Add(ContentRect))), Color.black);
//                            EditorGUI.LabelField(Padding.Add(Border.Add(Margin.Add(ContentRect))), Padding.Add(Border.Add(Margin.Add(ContentRect))).ToString());
//                            EditorGUI.DrawRect(Padding.Add(Border.Add(ContentRect)), Color.white);
//                            EditorGUI.DrawRect(Padding.Add(ContentRect), Color.grey);
//
////                            var rect = Padding.Add(ContentRect);
////                            EditorGUI.DrawRect(rect, Color.black);
////                            EditorGUI.LabelField(rect, NextEntryPosition.ToString());
//
//
////                            EditorGUI.DrawRect(VisibleAreaRect, Color.magenta);
////                            EditorGUI.LabelField(VisibleAreaRect, VisibleAreaRect.ToString());
//                        }

                        GUI.BeginClip(VisibleAreaRect);
                        VisibleAreaRect.position = Vector2.zero;


                        if (AutomaticEntryWidth < 0f) {
                            AutomaticEntryWidth = ContentRect.width;
                        }

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