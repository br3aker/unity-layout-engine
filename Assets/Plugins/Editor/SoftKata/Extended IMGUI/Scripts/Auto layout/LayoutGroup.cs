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
        [Obsolete]
        private static readonly RectOffset ZeroRectOffset = new RectOffset(0, 0, 0, 0);

        internal LayoutGroup Parent { get; private set; }

        // offset settings - Padding/Border/Margin
        public RectOffset TotalOffset {get;}

        // protected Rect ContainerRect;
        // public Rect VisibleAreaRect { get; protected set; }

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
        protected Vector2 ContentPosition;

        protected Rect _ContentRect;
        protected Rect _ContainerRect;

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

                var iDontCareRect = Parent?.GetNextEntryRect(TotalWidth, TotalHeight) 
                                        ?? LayoutEngine.GetRectFromUnityLayout(TotalHeight, TotalWidth);
            }
        }

        // Non-layout
        internal void RetrieveLayoutData() {
            if (IsGroupValid) {
                if (Parent != null) {
                    _ContainerRect = Parent.GetVisibleContentRect(TotalWidth, TotalHeight, out var offset);
                    _ContentRect = TotalOffset.Remove(new Rect(NextEntryPosition += offset, new Vector2(TotalWidth, TotalHeight)));
                }
                else {
                    _ContentRect = _ContainerRect = TotalOffset.Remove(LayoutEngine.GetRectFromUnityLayout(TotalHeight, TotalWidth));
                }

                IsGroupValid = _ContainerRect.IsValid() && Event.current.type != EventType.Used;
                if (IsGroupValid) {
                    // Default calculations
                    IsLayoutEvent = false;

                    // Visualization
                    EditorGUI.DrawRect(TotalOffset.Add(_ContentRect), new Color(0, 1, 0, 0.25f));
                    EditorGUI.DrawRect(_ContentRect, new Color(0, 0, 1, 0.25f));

                    // Clipspace extra calculations
                    if(Clip) {
                        GUI.BeginClip(_ContainerRect);
                        _ContentRect.position -= _ContainerRect.position;
                        _clipGlobalPosition = _ContainerRect.position;
                        _ContainerRect.position = Vector2.zero;
                    }
                    else {
                        NextEntryPosition += new Vector2(TotalOffset.left, TotalOffset.top);
                    }
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

        private Rect GetVisibleContentRect(float width, float height, out Vector2 originOffset) {
            CurrentEntryPosition = NextEntryPosition;
            originOffset = CurrentEntryPosition;

            if (width <= 0f) width = AutomaticWidth;

            // Debug.Log($"[{GetType().Name}] GetVisibleContentRect | w: {width} h: {height}");
            if (PrepareNextRect(width, height)) {
                var visibleGroupRect = Utility.RectIntersection(new Rect(CurrentEntryPosition, new Vector2(width, height)), _ContainerRect);
                return visibleGroupRect;
            }

            return InvalidRect;
        }

        protected abstract bool PrepareNextRect(float width, float height);

        public void RegisterArray(float elementHeight, int count) {
            RegisterArray(LayoutEngine.AutoWidth, elementHeight, count);
        }
        public abstract void RegisterArray(float elemWidth, float elemHeight, int count);

        public Rect GetContentRect(bool fullRect = false) {
            var output = _ContainerRect;

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
                _ContainerRect.position = _clipGlobalPosition;
                _ContentRect.position = _clipGlobalPosition;
            }
            EndNonLayoutRoutine();
        }
        protected virtual void EndNonLayoutRoutine() {

        }
    }
}