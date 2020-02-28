using System;
using UnityEditor;
using UnityEngine;

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
        private static bool RegisterForLayout(LayoutGroupBase layoutGroup) {
            LayoutGroupQueue.Enqueue(layoutGroup);
            _topGroup = layoutGroup;

            return true;
        }
        private static LayoutGroupBase RetrieveNextGroup() {
            _topGroup = LayoutGroupQueue.Dequeue();
            _topGroup.RetrieveLayoutData();
            return _topGroup;
        }
        private static T RetrieveNextGroup<T>() where T : LayoutGroupBase {
            if (RetrieveNextGroup() is T castedTypeGroup) return castedTypeGroup;
            throw new Exception(
                $"Group type mismatch at [{nameof(RetrieveNextGroup)}<{typeof(T)}>]: Expected {typeof(T).Name} | Got {_topGroup.GetType().Name}");
        }
        private static T EndLayoutGroup<T>() where T : LayoutGroupBase {
            var eventType = Event.current.type;

            var currentGroup = _topGroup;
            if (eventType == EventType.Layout)
                currentGroup.RegisterLayoutRequest();
            else if (currentGroup.IsGroupValid) currentGroup.EndGroup(eventType);
            _topGroup = currentGroup._parent;

            if (currentGroup is T castedTypeGroup) return castedTypeGroup;
            throw new Exception(
                $"Group type mismatch at {nameof(EndLayoutGroup)}<{typeof(T).Name}>: Expected: {typeof(T).Name} | Got: {currentGroup.GetType().Name}");
        }

        internal struct GroupRenderingData {
            public Rect VisibleRect;
            public Rect FullContentRect;
        }

        public abstract class LayoutGroupBase {
            protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroupBase).GetHashCode();

            // registered style
            protected readonly GUIStyle _style;

            // offset settings
            protected readonly RectOffset Margin;
            protected readonly RectOffset Border;
            protected readonly RectOffset Padding;
            protected int ChildrenCount;
            private int _groupIndex;

            private readonly GroupModifier _modifier;

            protected float AutomaticEntryWidth = -1f;
            
            protected Rect ContainerRect;
            protected Rect VisibleAreaRect;

            protected Vector2 ContentOffset;

            internal int EntriesCount;
            internal bool IsGroupValid;

            protected bool IsLayout = true;

            // entries layout data
            protected Vector2 CurrentEntryPosition;
            protected Vector2 NextEntryPosition;

            protected float ServiceHeight;
            protected float ServiceWidth;

            // group layouting data
            protected float RequestedHeight;
            protected float RequestedWidth;

            protected LayoutGroupBase(GroupModifier modifier, GUIStyle style) {
                _parent = _topGroup;

                _style = style;
                
                _modifier = modifier;

                // Child groups indexing
                _groupIndex = LayoutGroupQueue.Count;

                // group layout settings
                Margin = (modifier & GroupModifier.DiscardMargin) == GroupModifier.DiscardMargin
                    ? ZeroRectOffset
                    : style.margin;
                Border = (modifier & GroupModifier.DiscardBorder) == GroupModifier.DiscardBorder
                    ? ZeroRectOffset
                    : style.border;
                Padding = (modifier & GroupModifier.DiscardPadding) == GroupModifier.DiscardPadding
                    ? ZeroRectOffset
                    : style.padding;

                ContentOffset = style.contentOffset;
            }

            internal LayoutGroupBase _parent;

            internal void RegisterLayoutRequest() {
                ChildrenCount = LayoutGroupQueue.Count - _groupIndex - 1;
                IsGroupValid = EntriesCount > 0;
                
                if (IsGroupValid) {
                    ServiceHeight = Margin.vertical + Border.vertical + Padding.vertical +
                                    ContentOffset.y * (EntriesCount - 1);
                    ServiceWidth = Margin.horizontal + Border.horizontal + Padding.horizontal +
                                   ContentOffset.x * (EntriesCount - 1);


                    RequestedHeight += ServiceHeight;
                    if (RequestedWidth > 0f) RequestedWidth += ServiceWidth;

                    PreLayoutRequest();

                    VisibleAreaRect = _parent?.GetNextEntryRect(RequestedWidth, RequestedHeight) ??
                                      GetRectFromRoot(RequestedHeight, RequestedWidth);
                }
            }

            internal virtual void RetrieveLayoutData() {
                if (IsGroupValid) {
                    if (_parent != null) {
                        var rectData = _parent.GetGroupRectData(RequestedWidth, RequestedHeight);
                        VisibleAreaRect = rectData.VisibleRect;
                        ContainerRect = rectData.FullContentRect;
                    }
                    else {
                        VisibleAreaRect = GetRectFromRoot(RequestedHeight, RequestedWidth);
                        ContainerRect = VisibleAreaRect;
                    }
                    
                    IsGroupValid = VisibleAreaRect.IsValid() && Event.current.type != EventType.Used;
                    if (VisibleAreaRect.IsValid() && Event.current.type != EventType.Used) {
                        IsLayout = false;

                        ContainerRect = Padding.Remove(Border.Remove(Margin.Remove(ContainerRect)));
                        NextEntryPosition = ContainerRect.position;

                        if (AutomaticEntryWidth < 0f) AutomaticEntryWidth = ContainerRect.width;

                        return;
                    }
                }

                ScrapGroups(ChildrenCount);
            }

            protected virtual void PreLayoutRequest() { }

            internal Rect GetNextEntryRect(float width, float height) {
                CurrentEntryPosition = NextEntryPosition;

                if (width < 0f) width = AutomaticEntryWidth;

                if (PrepareNextRect(width, height))
                    return GetEntryRect(CurrentEntryPosition.x, CurrentEntryPosition.y, width, height);
                return InvalidRect;
            }

            internal GroupRenderingData GetGroupRectData(float width, float height) {
                CurrentEntryPosition = NextEntryPosition;

                if (width <= 0f) width = AutomaticEntryWidth;

                var visibleRect = InvalidRect;
                if (PrepareNextRect(width, height))
                    visibleRect = GetVisibleGroupRect(CurrentEntryPosition.x, CurrentEntryPosition.y, width, height);

                return new GroupRenderingData {
                    VisibleRect = visibleRect,
                    FullContentRect = new Rect(CurrentEntryPosition, new Vector2(width, height))
                };
            }

            protected abstract bool PrepareNextRect(float width, float height);

            protected virtual Rect GetEntryRect(float x, float y, float width, float height) {
                return new Rect(x, y, width, height);
            }

            private Rect GetVisibleGroupRect(float x, float y, float width, float height) {
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

            internal void RegisterArray(float elementHeight, int count) {
                RegisterArray(AutomaticEntryWidth, elementHeight, count);
            }

            internal abstract void RegisterArray(float elemWidth, float elemHeight, int count);


            internal virtual void EndGroup(EventType eventType) {
                EndGroupModifiersRoutine(eventType);
            }

            protected void EndGroupModifiersRoutine(EventType eventType) {
                var paddedContentRect = Padding.Add(ContainerRect);

                // Left separator line
                if ((_modifier & GroupModifier.DrawLeftSeparator) == GroupModifier.DrawLeftSeparator &&
                    eventType == EventType.Repaint) {
                    var separatorLineRect = new Rect(paddedContentRect.x - Border.left, paddedContentRect.y,
                        Border.left, paddedContentRect.height);
                    EditorGUI.DrawRect(separatorLineRect, GUI.enabled ? _style.onNormal.textColor : _style.normal.textColor);
                }
            }

            public Rect GetContentRect() {
                return Margin.Add(ContainerRect);
            }
        }
    }
}