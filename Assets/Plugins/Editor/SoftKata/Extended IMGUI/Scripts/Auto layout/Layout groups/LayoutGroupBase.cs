using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal struct GroupRectData {
            public Rect ClippedRect;
            public Rect FullContentRect;
        }
        
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
            protected Rect VisibleAreaRect;
            protected Rect ContentRect;

            // entries layout data
            protected Vector2 NextEntryPosition;
            protected Vector2 CurrentEntryPosition;

            // padding settings
            protected readonly RectOffset Margin;
            protected readonly RectOffset Border;
            protected readonly RectOffset Padding;

            protected Vector2 ContentOffset;

            protected LayoutGroupBase(bool discardMargin, GUIStyle style) {
                Parent = _topGroup;

                // Child groups indexing
                _groupIndex = SubscribedForLayout.Count;
                
                // group layout settings
                if (discardMargin) {
                    Margin = ZeroRectOffset;
                }
                else {
                    Margin = style.margin;
                }
                Border = style.border;
                Padding = style.padding;

                ContentOffset = style.contentOffset;

                MaxAllowedWidth = (Parent?.MaxAllowedWidth ?? EditorGUIUtility.currentViewWidth) - Margin.horizontal - Padding.horizontal;
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
                    
                    VisibleAreaRect = Parent?.GetRect(TotalRequestedHeight, TotalRequestedWidth) ?? LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                }
            }

            internal virtual void RetrieveLayoutData(EventType currentEventType) {
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
                        NextEntryPosition = ContentRect.position;
                        
                        MaxAllowedWidth = VisibleAreaRect.width;

                        return;
                    }
                }

                // Nested groups should be banished exactly here at non-layout layout data pull
                // This would ensure 2 things:
                // 1. Entries > 0 because this is called after PushLayoutRequest() which checks that condition
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

            internal Rect GetRect(float height, float width) {
                CurrentEntryPosition = NextEntryPosition;
                if (width < 0f) {
                    width = MaxAllowedWidth;
                }
                if (RegisterNewEntry(height, width)) {
                    return GetRectInternal(CurrentEntryPosition.x, CurrentEntryPosition.y, height, width);
                }

                return InvalidRect;
            }

            internal GroupRectData GetGroupRect(float height, float width) {
                CurrentEntryPosition = NextEntryPosition;

                var groupRect = 
                    RegisterNewEntry(height, width)
                    ? GetGroupRectInternal(CurrentEntryPosition.x, CurrentEntryPosition.y, height, Mathf.Min(VisibleAreaRect.width, width))
                    : InvalidRect;
                
                return new GroupRectData {
                    ClippedRect = groupRect,
                    FullContentRect = new Rect(CurrentEntryPosition, new Vector2(width, height))
                };
            }

            protected abstract bool RegisterNewEntry(float height, float width);

            protected virtual Rect GetRectInternal(float x, float y, float height, float width) {
                return new Rect(x, y, width, height);
            }

            private Rect GetGroupRectInternal(float x, float y, float height, float width) {
                var clippedHeight = height;
                var modifiedY = y;

                float upClip = 0f;
                float botClip = 0f;
                
                var entryBottom = y + height;
                if (y < VisibleAreaRect.y) {
                    var uselessTop = Mathf.Abs(VisibleAreaRect.y - y);
                    clippedHeight -= uselessTop;
                    modifiedY += uselessTop;

                    upClip = uselessTop;

//                    if (GetType() == typeof(ScrollGroup)) {
//                        Debug.Log(uselessTop);
//                    }
                }
                if (entryBottom > VisibleAreaRect.yMax) {
                    var uselessBottom = Mathf.Abs(entryBottom - VisibleAreaRect.yMax);
                    clippedHeight -= uselessBottom;

                    botClip = uselessBottom;
                }
                
//                if (GetType() == typeof(VerticalFadeGroup)) {
//                    Debug.Log($"Clip Space: {VisibleAreaRect} | Requested height {height} - actual height: {clippedHeight} | Clipped top: {upClip} - Clipped bottom: {botClip}");
//                }
                
                return new Rect(0f, modifiedY, width, clippedHeight);
            }

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
        }

        
        internal static bool BeginGroupBase<T>(bool discardMargins, GUIStyle style, Func<bool, GUIStyle, T> creator) where T : LayoutGroupBase {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = creator(discardMargins, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        
        internal static T EndLayoutGroup<T>() where T : LayoutGroupBase {
            var eventType = Event.current.type;

            var currentGroup = _topGroup;
            if (eventType == EventType.Layout) {
                currentGroup.RegisterLayoutRequest();
            }
            else {
                currentGroup.EndGroup(eventType);
            }
            _topGroup = currentGroup.Parent;

            if (!(currentGroup is T)) {
                throw new Exception($"Group type mismatch: Expected {typeof(T).Name} | Got {currentGroup.GetType().Name}");
            }

            return (T) currentGroup;
        }
    }
}