using System;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Profiling;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalClippingGroup : VerticalGroup {
            public VerticalClippingGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) {}

            internal override void RetrieveLayoutData() {
                if (IsGroupValid) {
                    if (Parent != null) {
                        var rectData = Parent.GetGroupRectData(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = rectData.VisibleRect;
                        ContainerRect = rectData.FullContentRect;
                    }
                    else {
                        ContainerRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = ContainerRect;
                    }
                    
                    IsGroupValid = VisibleAreaRect.IsValid();

                    if (IsGroupValid) {
                        IsLayout = false;
                        
                        ContainerRect = Padding.Remove(Border.Remove(Margin.Remove(ContainerRect)));
                        VisibleAreaRect = Utility.RectIntersection(VisibleAreaRect, ContainerRect);
                        
                        NextEntryPosition += ContainerRect.position - VisibleAreaRect.position;

                        GUI.BeginClip(VisibleAreaRect);
                        VisibleAreaRect.position = Vector2.zero;


                        if (AutomaticEntryWidth < 0f) {
                            AutomaticEntryWidth = ContainerRect.width;
                        }

                        return;
                    }
                }

                IsGroupValid = false;
                LayoutEngine.ScrapGroups(_childrenCount);
            }

            internal sealed override void EndGroup(EventType eventType) {
                GUI.EndClip();
                EndGroupModifiersRoutine(eventType);
            }
        }
    }
}