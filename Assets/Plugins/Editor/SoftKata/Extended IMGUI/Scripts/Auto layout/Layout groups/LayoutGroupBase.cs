using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal abstract class LayoutGroupBase {
            protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroupBase).GetHashCode();
            
            internal LayoutGroupBase Parent { get; }
            private int _groupIndex;
            internal int _childrenCount;

            protected EventType CurrentEventType = EventType.Layout;
            
            // group layouting data
            protected float TotalContainerHeight = 0f;
            protected float TotalContainerWidth = 0f;
            
            protected int EntriesCount;
            internal bool IsGroupValid = true;

            protected float DefaultEntryWidth;

            // total rect of the group
            internal Rect FullRect;
            
            // entries layout data
            protected float NextEntryX = 0f;
            protected float NextEntryY = 0f;

            // padding settings
            protected readonly RectOffset Margin;
            protected readonly RectOffset Padding;

            protected Vector2 ContentOffset;
            
            // Debug data
            private int _debugIndentLevel;
            

            protected LayoutGroupBase(bool discardMargin, GUIStyle style) {
                Parent = _topGroup;

                // Child groups indexing
                _groupIndex = SubscribedForLayout.Count;
                
                // group layout settings
                if (discardMargin) {
                    Margin = new RectOffset(0, 0, 0, 0);
                }
                else {
                    Margin = style.margin;
                }
                Padding = style.padding;

                ContentOffset = style.contentOffset;

                DefaultEntryWidth = (Parent?.DefaultEntryWidth ?? EditorGUIUtility.currentViewWidth) - Margin.horizontal - Padding.horizontal;

                // Debug data
                _debugIndentLevel = (Parent?._debugIndentLevel ?? -1) + 1;
            }

            internal void RegisterLayoutRequest() {
                _childrenCount = SubscribedForLayout.Count - _groupIndex - 1;
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    TotalContainerHeight += 
                        Margin.vertical
                        + Padding.vertical
                        + ContentOffset.y * (EntriesCount - 1);

                    TotalContainerWidth +=
                        Margin.horizontal
                        + Padding.horizontal
                        + ContentOffset.x * (EntriesCount - 1);
                    
                    CalculateLayoutData();
                    
                    FullRect = Parent?.GetRect(TotalContainerHeight, TotalContainerWidth) ?? LayoutEngine.RequestRectRaw(TotalContainerHeight, TotalContainerWidth);
                }
            }

            internal void RetrieveLayoutData(EventType currentEventType) {
                //RegisterDebugData();
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    FullRect = Parent?.GetRect(TotalContainerHeight, TotalContainerWidth) ?? LayoutEngine.RequestRectRaw(TotalContainerHeight, TotalContainerWidth);
                    IsGroupValid = FullRect.IsValid();
                    if (IsGroupValid) {
//                        EditorGUI.DrawRect(FullRect, Color.red);
                        
                        FullRect = Margin.Remove(FullRect);
                        GUI.BeginClip(FullRect);
                        DefaultEntryWidth = FullRect.width - Padding.horizontal;
                        NextEntryX += Padding.left;
                        NextEntryY += Padding.top;
                        
                        FullRect.y = 0;
                        FullRect.x = 0;
//                        EditorGUI.DrawRect(FullRect, Color.cyan);
//                        EditorGUI.LabelField(FullRect, $"{FullRect.ToString()} | {DefaultEntryWidth}");

                        return;
                    }
                }
                
                //UpdateDebugData();

                // Nested groups should be banished exactly here at non-layout layout data pull
                // This would ensure 2 things:
                // 1. Entries > 0 because this is called after PushLayoutRequest() which checks that
                // 2. Parent group returned Valid rect
                LayoutEngine.ScrapGroups(_childrenCount);
            }

            protected virtual void CalculateLayoutData() { }

            internal virtual Rect GetRect(float height) {
                return GetRect(height, DefaultEntryWidth);
            }
            internal abstract Rect GetRect(float height, float width);

            internal void RegisterRectArray(float elementHeight, int count) {
                RegisterRectArray(elementHeight, DefaultEntryWidth, count);
            }
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

                TotalContainerHeight = 0;
                TotalContainerWidth = 0;
                
                NextEntryX = 0;
                NextEntryY = 0;
            }
            
            internal void RegisterDebugData() {
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

            internal void UpdateDebugData() {
                var registeredData = _debugDataList[_groupIndex];
                _debugDataList[_groupIndex] = new LayoutDebugData {Data = registeredData.Data, IsValid = IsGroupValid};
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