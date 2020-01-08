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

            // padding settings
            private RectOffset _padding;
            
            // Debug data
            private int _debugIndentLevel;
            

            protected LayoutGroupBase(GUIStyle style) {
                Parent = _topGroup;

                // Child groups indexing
                _groupIndex = _groupCount++;
                
                // group layout settings
                _padding = style.padding;
                
                // Debug data
                _debugIndentLevel = _globalIndentLevel++;
            }

            internal void RegisterLayoutRequest() {
                _childrenCount = _groupCount - _groupIndex - 1;
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    CalculateLayoutData();
                    TotalHeight += _padding.vertical;
                    TotalWidth += _padding.horizontal;
                    FullRect = Parent?.GetRect(TotalHeight, TotalWidth) ?? LayoutEngine.RequestRectRaw(TotalHeight, TotalWidth);
                }
            }

            internal void RetrieveLayoutData(EventType currentEventType) {
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    FullRect = Parent?.GetRect(TotalHeight, TotalWidth) ?? LayoutEngine.RequestRectRaw(TotalHeight, TotalWidth);
                    IsGroupValid = FullRect.IsValid();
                    if (IsGroupValid) {
                        EditorGUI.DrawRect(FullRect, Color.red);
                        
                        FullRect = _padding.Remove(FullRect);
                        GUI.BeginClip(FullRect);
                        FullRect.y = 0;
                        FullRect.x = 0;
                        
                        var color = Color.cyan;
                        color.a = 100f / 255;
                        EditorGUI.DrawRect(FullRect, color);
                        EditorGUI.LabelField(FullRect, FullRect.ToString());
                        
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

            internal Rect GetRect(float height) {
                return GetRect(height, EditorGUIUtility.currentViewWidth);
            }
            internal abstract Rect GetRect(float height, float width);
            internal abstract void RegisterRectArray(float elementHeight, float elementWidth, int count);

            internal void EndGroup(EventType currentEventType) {
                if (IsGroupValid) {
                    EndGroupRoutine(currentEventType);
                    GUI.EndClip();
                }
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
                _globalIndentLevel--;
                string tabSpacing = new string('\t', _debugIndentLevel);
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
                currentGroup.RegisterLayoutRequest();
            }
            else {
                currentGroup.EndGroup(eventType);
            }
            
            _topGroup = currentGroup.Parent;

            return currentGroup;
        }
    }
}