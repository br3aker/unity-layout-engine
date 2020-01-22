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
            protected float TotalRequestedHeight = 0f;
            protected float TotalRequestedWidth = 0f;

            internal int EntriesCount;
            internal bool IsGroupValid = true;

            protected float MaxAllowedWidth = -1f;

            // total rect of the group
            internal Rect FullContainerRect;
            
            // entries layout data
            protected Vector2 NextEntryPosition;
            protected Vector2 EntryRectBorders;

            // padding settings
            protected readonly RectOffset Margin;
            protected readonly RectOffset Border;
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
                Border = style.border;
                Padding = style.padding;

                ContentOffset = style.contentOffset;

                MaxAllowedWidth = (Parent?.MaxAllowedWidth ?? EditorGUIUtility.currentViewWidth) - Margin.horizontal - Padding.horizontal;

                // Debug data
                _debugIndentLevel = Parent?._debugIndentLevel + 1 ?? 0;
            }

            internal void RegisterLayoutRequest() {
                _childrenCount = SubscribedForLayout.Count - _groupIndex - 1;
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    TotalRequestedHeight += 
                        Margin.vertical
                        + Padding.vertical
                        + ContentOffset.y * (EntriesCount - 1);

                    TotalRequestedWidth +=
                        Margin.horizontal
                        + Padding.horizontal
                        + ContentOffset.x * (EntriesCount - 1);
                    
                    CalculateLayoutData();
                    
                    FullContainerRect = Parent?.GetRect(TotalRequestedHeight, TotalRequestedWidth) ?? LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                }
            }

            protected abstract Vector2 GetContentBorderValues();

            internal virtual void RetrieveLayoutData(EventType currentEventType) {
//                MaxAllowedWidth = Parent?.MaxAllowedWidth ?? EditorGUIUtility.currentViewWidth - Margin.horizontal - Padding.horizontal;
                
                RegisterDebugData();
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    if (Parent != null) {
                        FullContainerRect = Parent.GetRect(TotalRequestedHeight, TotalRequestedWidth);
                    }
                    else {
                        FullContainerRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                    }
                    IsGroupValid = FullContainerRect.IsValid();

                    if (IsGroupValid) {
                        FullContainerRect = Margin.Remove(FullContainerRect);
                        
                        NextEntryPosition = FullContainerRect.position + new Vector2(Padding.left, Padding.top);
                        MaxAllowedWidth = FullContainerRect.width;// - Padding.horizontal;
                        
                        EntryRectBorders = GetContentBorderValues();


//                        if (GetType() == typeof(HorizontalLayoutGroup)) {
//                            EditorGUI.DrawRect(FullContainerRect, Color.magenta);
//                            EditorGUI.LabelField(FullContainerRect, FullContainerRect.ToString());
//                        }

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

            protected virtual void CalculateLayoutData() { }

            internal virtual Rect GetRect(float height) {
                return GetRect(height, MaxAllowedWidth);
            }
            internal abstract Rect GetRect(float height, float width);

            internal void RegisterRectArray(float elementHeight, int count) {
                RegisterRectArray(elementHeight, MaxAllowedWidth, count);
            }
            internal abstract void RegisterRectArray(float elementHeight, float elementWidth, int count);

            
            internal virtual void EndGroup(EventType currentEventType) {
                if (IsGroupValid) {
                    EndGroupRoutine(currentEventType);
                }
            }
            protected virtual void EndGroupRoutine(EventType currentEventType) { }
            
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