using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal abstract class LayoutGroupBase {
            internal LayoutGroupBase Parent { get; }
            private int _groupIndex;
            internal int _childrenCount;

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

            private int _localIndentLevel;

            protected LayoutGroupBase(GUIStyle style) {
                Parent = _topGroup;

                _groupIndex = _groupCount++;

                _localIndentLevel = _globalIndentLevel;
                _globalIndentLevel++;
            }

            internal void PushLayoutRequest() {
                _childrenCount = _groupCount - _groupIndex - 1;
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    CalculateLayoutData();
                    FullRect = Parent?.GetRect(TotalHeight, TotalWidth) ?? LayoutEngine.RequestRectRaw(TotalHeight);
                }
            }

            internal void RetrieveLayoutData(EventType currentEventType) {
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    FullRect = Parent?.GetRect(TotalHeight, TotalWidth) ?? LayoutEngine.RequestRectRaw(TotalHeight);
                    IsGroupValid = FullRect.IsValid();
                    if (IsGroupValid) {
                        GUI.BeginClip(FullRect);
                        FullRect.y = 0;
                        FullRect.x = 0;
                        return;
                    }
                }

                // Nested groups should be banished exactly here at non-layout layout data pull
                // This would ensure 2 things:
                // 1. Entries > 0 because this is called after PushLayoutRequest() which checks that
                // 2. Parent group returned Valid rect
                _globalIndentLevel -= _childrenCount;
                LayoutEngine.ScrapGroups(_childrenCount);
            }

            protected abstract void CalculateLayoutData();

            internal abstract Rect GetRect(float height);
            internal abstract Rect GetRect(float height, float width);

            internal void EndGroup(EventType currentEventType) {
                _globalIndentLevel--;
                if (IsGroupValid) {
                    EndGroupRoutine(currentEventType);
                    GUI.EndClip();
                }
                
                IsGroupValid = true;
            }

            protected virtual void EndGroupRoutine(EventType currentEventType) { }

            internal void ResetGroup() {
                CurrentEventType = EventType.Layout;

                IsGroupValid = true;

                TotalHeight = 0;
                TotalWidth = 0;
                
                NextEntryX = 0;
                NextEntryY = 0;
            }

            internal void RegisterDebugData() {
                string tabSpacing = new string('\t', _localIndentLevel);
                string childrenCount = _childrenCount > 0 ? $" | Children count: {_childrenCount}" : "";
                string data = $"{tabSpacing}{GetType().Name}{childrenCount}";
                _debugDataList.Add(
                    new LayoutDebugData {
                        IsValid = IsGroupValid,
                        Data = data 
                    }
                );
            }
        }
        
        internal static LayoutGroupBase EndLayoutGroup() {
            var eventType = Event.current.type;

            var currentGroup = _topGroup;
            if (eventType == EventType.Layout) {
                currentGroup.PushLayoutRequest();
            }
            else {
                currentGroup.EndGroup(eventType);
            }
            
            _topGroup = currentGroup.Parent;

            return currentGroup;
        }
    }
}