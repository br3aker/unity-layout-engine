using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace SoftKata.ExtendedEditorGUI {
    [Flags]
    public enum GroupModifier {
        None = 1 << 1,
        DiscardMargin = 1 << 2,
        DiscardBorder = 1 << 3,
        DiscardPadding = 1 << 4,
        DrawLeftSeparator = 1 << 5
    }
    
    public static partial class LayoutEngine {
        internal struct GroupRectData {
            public Rect VisibleRect;
            public Rect FullContentRect;
        }

        internal abstract class LayoutGroupBase {
            protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroupBase).GetHashCode();

            private GroupModifier _modifier;

            internal LayoutGroupBase Parent { get; }
            private int _groupIndex;
            internal int _childrenCount;

            protected EventType CurrentEventType = EventType.Layout;
            
            // group layouting data
            protected float TotalRequestedHeight = 0f;
            protected float TotalRequestedWidth = 0f;

            protected float ServiceHeight = 0f;
            protected float ServiceWidth = 0f;

            internal int EntriesCount;
            internal bool IsGroupValid = true;

            protected float AutomaticEntryWidth = -1f;

            // total rect of the group
            protected Rect VisibleAreaRect;
            protected Rect ContentRect;

            // entries layout data
            protected Vector2 NextEntryPosition;
            protected Vector2 CurrentEntryPosition;

            // offset settings
            protected readonly RectOffset Margin;
            protected readonly RectOffset Border;
            protected readonly RectOffset Padding;
            
            // main color - used in modfiers
            protected readonly Color ActiveColor;
            protected readonly Color InactiveColor;

            protected Vector2 ContentOffset;

            public string GetDebugData() => $"{ContentRect} | {IsGroupValid}";
            
            protected LayoutGroupBase(GroupModifier modifier, GUIStyle style) {
                Parent = _topGroup;

                _modifier = modifier;

                // Child groups indexing
                _groupIndex = SubscribedForLayout.Count;
                
                // group layout settings
                Margin = (modifier & GroupModifier.DiscardMargin) == GroupModifier.DiscardMargin ? ZeroRectOffset : style.margin;
                Border = (modifier & GroupModifier.DiscardBorder) == GroupModifier.DiscardBorder ? ZeroRectOffset : style.border;
                Padding = (modifier & GroupModifier.DiscardPadding) == GroupModifier.DiscardPadding ? ZeroRectOffset : style.padding;

                ActiveColor = style.onNormal.textColor;
                InactiveColor = style.normal.textColor;

                ContentOffset = style.contentOffset;
            }

            internal void RegisterLayoutRequest() {
                _childrenCount = SubscribedForLayout.Count - _groupIndex - 1;
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    ServiceHeight = Margin.vertical + Border.vertical + Padding.vertical + ContentOffset.y * (EntriesCount - 1);
                    ServiceWidth = Margin.horizontal + Border.horizontal + Padding.horizontal + ContentOffset.x * (EntriesCount - 1);

                    
                    TotalRequestedHeight += ServiceHeight;
                    if (TotalRequestedWidth > 0f) {
                        TotalRequestedWidth += ServiceWidth;
                    }
                    
                    PreLayoutRequest();

                    VisibleAreaRect = Parent?.GetRect(TotalRequestedHeight, TotalRequestedWidth) ?? LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                }
            }

            internal virtual void RetrieveLayoutData(EventType currentEventType) {
                if (IsGroupValid) {
                    CurrentEventType = currentEventType;
                    if (Parent != null) {
                        var rectData = Parent.GetGroupRectData(TotalRequestedHeight, TotalRequestedWidth);
                        VisibleAreaRect = rectData.VisibleRect;
                        ContentRect = rectData.FullContentRect;
                        Debug.Log($"Parent layout request call, group count: {LayoutEngine.GroupCount()}");
                    }
                    else {
                        VisibleAreaRect = LayoutEngine.RequestRectRaw(TotalRequestedHeight, TotalRequestedWidth);
                        ContentRect = VisibleAreaRect;
                    }
//                    Debug.Log($"Non-layout validation: {VisibleAreaRect}");
                    IsGroupValid = VisibleAreaRect.IsValid();

                    if (IsGroupValid) {
//                        Debug.Log("Non-layout VALID");
                        
                        ContentRect = Padding.Remove(Border.Remove(Margin.Remove(ContentRect)));
                        NextEntryPosition = ContentRect.position;

//                        var testRect = ContentRect;
//                        testRect.height += Margin.top;
//                        EditorGUI.LabelField(testRect, ContentRect.ToString());
//                        EditorGUI.DrawRect(Padding.Add(Border.Add(Margin.Add(testRect))), Color.white);
//                        EditorGUI.DrawRect(Padding.Add(Border.Add(testRect)), Color.black);
//                        EditorGUI.DrawRect(Padding.Add(testRect), Color.green);

                        if (AutomaticEntryWidth < 0f) {
                            AutomaticEntryWidth = ContentRect.width;
                        }

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

            protected virtual void PreLayoutRequest() { }

            internal Rect GetRect(float height, float width) {
                CurrentEntryPosition = NextEntryPosition;
                
                if (width < 0f) {
                    width = AutomaticEntryWidth;
                }

                if (RegisterNewEntry(height, width)) {
                    return GetRectInternal(CurrentEntryPosition.x, CurrentEntryPosition.y, height, width);
                }

                return InvalidRect;
            }

            internal GroupRectData GetGroupRectData(float height, float width) {
                CurrentEntryPosition = NextEntryPosition;

                if (width <= 0f) {
                    width = AutomaticEntryWidth;
                }
                
                Rect visibleRect = InvalidRect;
                if (RegisterNewEntry(height, width)) {
                    visibleRect = GetVisibleGroupRect(CurrentEntryPosition.x, CurrentEntryPosition.y, height, width);
                }

                return new GroupRectData {
                    VisibleRect = visibleRect,
                    FullContentRect = new Rect(CurrentEntryPosition, new Vector2(width, height))
                };
            }

            protected abstract bool RegisterNewEntry(float height, float width);

            protected virtual Rect GetRectInternal(float x, float y, float height, float width) {
                return new Rect(x, y, width, height);
            }

            private Rect GetVisibleGroupRect(float x, float y, float height, float width) {
                // X axis
                var clippedWidth = width;
                var modifiedX = x;
                var entryRight = x + width;
                
                if (x < VisibleAreaRect.x) {
                    var uselessLeft = Mathf.Abs(VisibleAreaRect.x - x);
                    clippedWidth -= uselessLeft;
                    modifiedX += uselessLeft;
                }
                if (entryRight > VisibleAreaRect.xMax) {
                    var uselessRight = Mathf.Abs(entryRight - VisibleAreaRect.xMax);
                    clippedWidth -= uselessRight;
                }
                
                // Y axis
                var clippedHeight = height;
                var modifiedY = y;
                var entryBottom = y + height;
                
                if (y < VisibleAreaRect.y) {
                    var uselessTop = Mathf.Abs(VisibleAreaRect.y - y);
                    clippedHeight -= uselessTop;
                    modifiedY += uselessTop;
                }
                if (entryBottom > VisibleAreaRect.yMax) {
                    var uselessBottom = Mathf.Abs(entryBottom - VisibleAreaRect.yMax);
                    clippedHeight -= uselessBottom;
                }

                return new Rect(modifiedX, modifiedY, clippedWidth, clippedHeight);
            }

            internal void RegisterRectArray(float elementHeight, int count) {
                RegisterRectArray(elementHeight, AutomaticEntryWidth, count);
            }
            internal abstract void RegisterRectArray(float elementHeight, float elementWidth, int count);

            
            internal virtual void EndGroup(EventType currentEventType) {
                if (IsGroupValid) {
                    EndGroupModifiersRoutine();
                    EndGroupRoutine(currentEventType);
                }
            }
            protected virtual void EndGroupRoutine(EventType currentEventType) { }

            protected void EndGroupModifiersRoutine() {
                var paddedContentRect = Padding.Add(ContentRect);
                
                // Left separator line
                if ((_modifier & GroupModifier.DrawLeftSeparator) == GroupModifier.DrawLeftSeparator) {
                    var separatorLineRect = new Rect(paddedContentRect.x - Border.left, paddedContentRect.y, Border.left, paddedContentRect.height);
                    EditorGUI.DrawRect(separatorLineRect, GUI.enabled ? ActiveColor : InactiveColor);
                }
            }
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