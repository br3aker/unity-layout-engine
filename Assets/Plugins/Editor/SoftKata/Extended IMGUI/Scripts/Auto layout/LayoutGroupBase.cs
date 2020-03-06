using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    [Flags]
    public enum Constraints : byte {
        None = 1 << 1,
        DiscardMargin = 1 << 2,
        DiscardBorder = 1 << 3,
        DiscardPadding = 1 << 4,
        All = DiscardMargin | DiscardBorder | DiscardPadding
    }

    public static partial class LayoutEngine {
        private static bool RegisterForLayout(LayoutGroupBase layoutGroup) {
            layoutGroup.InitializeForLayout();
            LayoutGroupQueue.Enqueue(layoutGroup);
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
        
        public static bool BeginLayoutGroup(LayoutGroupBase group){
            if(Event.current.type == EventType.Layout){
                group.ResetLayout();
                return RegisterForLayout(group);
            }
            return RetrieveNextGroup().IsGroupValid;
        }
        public static T EndLayoutGroup<T>() where T : LayoutGroupBase {
            var eventType = Event.current.type;

            var currentGroup = _topGroup;
            if (eventType == EventType.Layout)
                currentGroup.RequestLayout();
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

            internal LayoutGroupBase _parent;
            
            // registered style
            protected readonly GUIStyle _style;

            // offset settings
            protected readonly RectOffset Margin;
            protected readonly RectOffset Border;
            protected readonly RectOffset Padding;

            protected int ChildrenCount;
            public int _groupIndex;

            protected Rect ContainerRect;
            public Rect VisibleAreaRect { get; protected set; }

            public float SpaceBetweenEntries { get; protected set; }

            internal int EntriesCount;
            internal bool IsGroupValid;

            protected bool IsLayout = true;

            // entries layout data
            protected Vector2 CurrentEntryPosition;
            protected Vector2 NextEntryPosition;

            public float ConstraintsHeight {get;}
            public float ConstraintsWidth {get;}

            // group layouting data
            protected float RequestedHeight;
            protected float RequestedWidth = -1;

            protected float _automaticWidth = -1;
            protected float _visibleContentWidth;
            public float VisibleContentWidth => _visibleContentWidth;

            protected LayoutGroupBase(Constraints modifier, GUIStyle style) {
                _style = style;

                // group layout settings
                Margin = (modifier & Constraints.DiscardMargin) == Constraints.DiscardMargin
                    ? ZeroRectOffset
                    : style.margin;
                Border = (modifier & Constraints.DiscardBorder) == Constraints.DiscardBorder
                    ? ZeroRectOffset
                    : style.border;
                Padding = (modifier & Constraints.DiscardPadding) == Constraints.DiscardPadding
                    ? ZeroRectOffset
                    : style.padding;
                
                ConstraintsHeight = Margin.vertical + Border.vertical + Padding.vertical;
                ConstraintsWidth = Margin.horizontal + Border.horizontal + Padding.horizontal;
            }

            protected abstract void ModifyContainerSize();
            
            internal void RequestLayout() {
                ChildrenCount = LayoutGroupQueue.Count - _groupIndex - 1;
                
                IsGroupValid = EntriesCount > 0;
                if (IsGroupValid) {
                    ModifyContainerSize();

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
                    if (IsGroupValid) {
                        IsLayout = false;
                        _automaticWidth = _visibleContentWidth;

                        ContainerRect = Padding.Remove(Border.Remove(Margin.Remove(ContainerRect)));

                        NextEntryPosition = ContainerRect.position;
                        
                        return;
                    }
                }

                ScrapGroups(ChildrenCount);
            }

            public Rect GetNextEntryRect(float width, float height) {
                CurrentEntryPosition = NextEntryPosition;

                if (width < 0f) width = _automaticWidth;
                
                if (PrepareNextRect(width, height)) {
                    return GetEntryRect(CurrentEntryPosition.x, CurrentEntryPosition.y, width, height);
                }
                return InvalidRect;
            }
            public bool GetNextEntryRect(float width, float height, out Rect rect) {
                rect = GetNextEntryRect(width, height);
                return rect.IsValid();
            }

            internal GroupRenderingData GetGroupRectData(float width, float height) {
                CurrentEntryPosition = NextEntryPosition;

                if (width <= 0f) width = _visibleContentWidth;

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

            public void RegisterArray(float elementHeight, int count) {
                RegisterArray(_automaticWidth, elementHeight, count);
            }
            public abstract void RegisterArray(float elemWidth, float elemHeight, int count);

            internal virtual void EndGroup(EventType eventType) {
                EndGroupModifiersRoutine(eventType);
            }

            protected void EndGroupModifiersRoutine(EventType eventType) {

            }

            public virtual Rect GetContentRect(Constraints contraints = Constraints.DiscardMargin) {
                var output = ContainerRect;
                if((contraints & Constraints.DiscardMargin) != Constraints.DiscardMargin) {
                    output = Margin.Add(output);
                }
                if ((contraints & Constraints.DiscardBorder) != Constraints.DiscardBorder) {
                    output = Border.Add(output);
                }
                if ((contraints & Constraints.DiscardPadding) != Constraints.DiscardPadding) {
                    output = Padding.Add(output);
                }

                return output;
            }

            public void InitializeForLayout(){
                _groupIndex = LayoutGroupQueue.Count;
                _parent = _topGroup;
                _topGroup = this;

                _visibleContentWidth =
                    (_parent?.VisibleContentWidth ?? EditorGUIUtility.currentViewWidth) - ConstraintsWidth;
            }

            public void ResetLayout() {
                IsLayout = true;

                 _automaticWidth = -1;
                RequestedWidth = -1;
                RequestedHeight = 0;

                NextEntryPosition = Vector2.zero;

                EntriesCount = 0;
            }
        }
    }
}