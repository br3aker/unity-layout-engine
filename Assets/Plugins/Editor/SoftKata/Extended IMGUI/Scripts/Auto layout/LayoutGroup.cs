using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    [Flags]
    public enum Constraints : byte {
        None = 1 << 1,
        DiscardMargin = 1 << 2,
        DiscardBorder = 1 << 3,
        DiscardPadding = 1 << 4,
        All = DiscardMargin | DiscardBorder | DiscardPadding
    }

    // TODO: fix flexible horizontal group
    // TODO: fix horizontal group
    // TODO: implement automatic height in parented to horizontal-like group
    public abstract class LayoutGroup {
        private struct GroupRenderingData {
            public Rect VisibleRect;
            public Rect FullContentRect;
        }

        protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroup).GetHashCode();
        private static readonly Rect InvalidRect = new Rect(float.MinValue, 0, -1, -1);
        private static readonly RectOffset ZeroRectOffset = new RectOffset(0, 0, 0, 0);

        internal LayoutGroup Parent { get; private set; }

        // offset settings - Padding/Border/Margin
        public RectOffset TotalOffset {get;}

        protected Rect ContainerRect;
        public Rect VisibleAreaRect { get; protected set; }

        public bool Clip {get; set;}
        private Vector2 _clipGlobalPosition;

        public float SpaceBetweenEntries { get; protected set; }

        protected int EntriesCount;
        public bool IsGroupValid {get; protected set;}

        protected bool IsLayoutEvent = true;

        // entries layout data
        protected Vector2 CurrentEntryPosition;
        protected Vector2 NextEntryPosition;

        // group layouting data
        protected float TotalHeight;
        protected float TotalWidth = -1;

        // Automatic width for entries
        public float AutomaticWidth {get; private set;}
        protected virtual float GetAutomaticWidth() => LayoutEngine.CurrentContentWidth - TotalOffset.horizontal;

        // Constructor
        protected LayoutGroup(Constraints modifier, GUIStyle style) {
            TotalOffset = new RectOffset();

            if((modifier & Constraints.DiscardMargin) != Constraints.DiscardMargin) {
                TotalOffset.Accumulate(style.margin);
            }
            if((modifier & Constraints.DiscardBorder) != Constraints.DiscardBorder) {
                TotalOffset.Accumulate(style.border);
            }
            if((modifier & Constraints.DiscardPadding) != Constraints.DiscardPadding) {
                TotalOffset.Accumulate(style.padding);
            };
        }

        // Layout
        protected abstract void PreLayoutRequest();
        private void RequestLayout() {
            IsGroupValid = EntriesCount > 0;
            if (IsGroupValid) {
                PreLayoutRequest();

                VisibleAreaRect = Parent?.GetNextEntryRect(TotalWidth, TotalHeight) 
                                        ?? LayoutEngine.GetRectFromUnityLayout(TotalHeight, TotalWidth);
            }
        }

        // Non-layout
        protected void CalculateLayoutData() {
            // Default calculations
            ContainerRect = TotalOffset.Remove(ContainerRect);
            NextEntryPosition += ContainerRect.position;

            // Clipspace extra calculations
            if(Clip) {
                GUI.BeginClip(ContainerRect);
                NextEntryPosition -= ContainerRect.position;
                _clipGlobalPosition = ContainerRect.position;
                VisibleAreaRect = new Rect(VisibleAreaRect.position - ContainerRect.position, VisibleAreaRect.size);
            }
        }
        internal void RetrieveLayoutData() {
            if (IsGroupValid) {
                if (Parent != null) {
                    var rectData = Parent.GetGroupRectData(TotalWidth, TotalHeight);
                    VisibleAreaRect = rectData.VisibleRect;
                    ContainerRect = rectData.FullContentRect;
                }
                else {
                    VisibleAreaRect = LayoutEngine.GetRectFromUnityLayout(TotalHeight, TotalWidth);
                    ContainerRect = VisibleAreaRect;
                }

                IsGroupValid = VisibleAreaRect.IsValid() && Event.current.type != EventType.Used;
                if (IsGroupValid) {
                    IsLayoutEvent = false;
                    CalculateLayoutData();
                    return;
                }
            }
        }

        // Getting rects
        public Rect GetNextEntryRect(float width, float height) {
            CurrentEntryPosition = NextEntryPosition;

            if (width < 0f) width = AutomaticWidth;
            
            if (PrepareNextRect(width, height)) {
                return new Rect(CurrentEntryPosition, new Vector2(width, height));
            }
            return InvalidRect;
        }
        public bool GetNextEntryRect(float width, float height, out Rect rect) {
            rect = GetNextEntryRect(width, height);
            return rect.IsValid();
        }

        private GroupRenderingData GetGroupRectData(float width, float height) {
            CurrentEntryPosition = NextEntryPosition;

            if (width <= 0f) width = AutomaticWidth;

            var visibleRect = InvalidRect;
            if (PrepareNextRect(width, height))
                visibleRect = Utility.RectIntersection(new Rect(CurrentEntryPosition, new Vector2(width, height)), VisibleAreaRect);

            return new GroupRenderingData {
                VisibleRect = visibleRect,
                FullContentRect = new Rect(CurrentEntryPosition, new Vector2(width, height))
            };
        }

        protected abstract bool PrepareNextRect(float width, float height);

        public void RegisterArray(float elementHeight, int count) {
            RegisterArray(LayoutEngine.AutoWidth, elementHeight, count);
        }
        public abstract void RegisterArray(float elemWidth, float elemHeight, int count);

        public virtual Rect GetContentRect(bool fullRect = false) {
            var output = ContainerRect;

            if(fullRect) {
                output = TotalOffset.Add(output);
            }

            // World pos -> Local pos
            if(Clip) {
                output.position -= _clipGlobalPosition;
            }

            return output;
        }

        public void ResetLayout() {
            IsLayoutEvent = true;

            // _automaticWidth = -1;
            TotalWidth = -1;
            TotalHeight = 0;

            NextEntryPosition = Vector2.zero;

            EntriesCount = 0;
        }
    
        internal void BeginLayout(LayoutGroup parent) {
            Parent = parent;
            AutomaticWidth = GetAutomaticWidth();
        }
        internal virtual void EndLayout() {
            RequestLayout();
        }
        internal virtual void BeginNonLayout() {
            RetrieveLayoutData();
        } 
        internal void EndNonLayout() {
            if(Clip) {
                GUI.EndClip();
            }
            EndNonLayoutRoutine();
        }
        protected virtual void EndNonLayoutRoutine() {

        }
    }
}