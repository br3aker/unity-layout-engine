using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        internal abstract class LayoutGroupBase {
            internal LayoutGroupBase Parent { get; }

            protected EventType CurrentEventType = EventType.Layout;
            
            // group metadata
            protected float TotalHeight;
            protected float TotalWidth;
            
            protected int EntriesCount;
            internal bool IsGroupValid = true;

            // total rect of the group
            internal Rect FullRect;
            
            // entries layout data
            protected float NextEntryX = 0f;
            protected float NextEntryY = 0f;

            protected LayoutGroupBase(GUIStyle style) {
                Parent = TopGroup;
            }

            internal void PushLayoutRequest() {
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    CalculateLayoutData();
                    FullRect = Parent?.GetRect(TotalHeight, TotalWidth) ?? AutoLayout.RequestRectRaw(TotalHeight);
                }
            }

            internal void RetrieveLayoutData(EventType currentEventType) {
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    FullRect = Parent?.GetRect(TotalHeight, TotalWidth) ?? AutoLayout.RequestRectRaw(TotalHeight);
                    IsGroupValid = FullRect.IsValid();
                    if (IsGroupValid) {
                        GUI.BeginClip(FullRect);
                        FullRect.y = 0;
                        FullRect.x = 0;
                    }
                }
            }

            protected abstract void CalculateLayoutData();

            internal abstract Rect GetRect(float height);
            internal abstract Rect GetRect(float height, float width);

            internal void EndGroup(EventType currentEventType) {
                if (IsGroupValid) {
                    EndGroupRoutine(currentEventType);
                    GUI.EndClip();
                }
            }

            protected virtual void EndGroupRoutine(EventType currentEventType) { }
        }
        
        internal static LayoutGroupBase EndLayoutGroup() {
            var eventType = Event.current.type;

            var currentGroup = TopGroup;
            if (eventType == EventType.Layout) {
                currentGroup.PushLayoutRequest();
            }
            else {
                currentGroup.EndGroup(eventType);
            }
            
            TopGroup = currentGroup.Parent;
            ActiveGroupStack.Pop();

            return currentGroup;
        }
    }
}