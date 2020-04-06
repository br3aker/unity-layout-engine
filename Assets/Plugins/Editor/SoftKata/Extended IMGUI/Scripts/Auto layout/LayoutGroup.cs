using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    [Flags]
    public enum Constraints : byte {
        None = 1 << 1,
        Margin = 1 << 2,
        Border = 1 << 3,
        Padding = 1 << 4,
        All = Margin | Border | Padding
    }

    // TODO: fix flexible horizontal group
    // TODO: fix horizontal group
    // TODO: implement automatic height in parented to horizontal-like group
    public abstract class LayoutGroup {
        protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroup).GetHashCode();
        private static readonly Rect InvalidRect = new Rect(float.MinValue, 0, -1, -1);

        internal LayoutGroup Parent { get; private set; }

        // offset settings - Padding/Border/Margin
        public RectOffset TotalOffset {get;}

        public bool Clip {get; set;}
        private Vector2 _clipWorldPositionOffset;

        public float SpaceBetweenEntries { get; protected set; }

        protected int EntriesCount;
        public bool IsGroupValid {get; protected set;}

        protected bool IsLayoutEvent = true;

        // entries layout data
        protected Vector2 NextEntryPosition;

        protected Rect ContentRect;
        protected Rect ContainerRect;

        // Automatic width for entries
        public float AutomaticWidth {get; protected set;}
        protected virtual float GetAutomaticWidth() => AvailableWidth - TotalOffset.horizontal;
        protected float AvailableWidth => Parent?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);

        // Constructor
        protected LayoutGroup(Constraints modifier, GUIStyle style) {
            TotalOffset = new RectOffset();

            if((modifier & Constraints.Margin) == Constraints.Margin) {
                TotalOffset.Accumulate(style.margin);
            }
            if((modifier & Constraints.Border) == Constraints.Border) {
                TotalOffset.Accumulate(style.border);
            }
            if((modifier & Constraints.Padding) == Constraints.Padding) {
                TotalOffset.Accumulate(style.padding);
            };
        }

        // Layout
        protected abstract void PreLayoutRequest();
        private void RequestLayout() {
            IsGroupValid = EntriesCount > 0;
            if (IsGroupValid) {
                PreLayoutRequest();

                var iDontCareRect = Parent?.GetNextEntryRect(ContentRect.width, ContentRect.height) 
                                        ?? LayoutEngine.GetRectFromUnityLayout(ContentRect.height, ContentRect.width);
            }
        }

        // Non-layout
        internal void RetrieveLayoutData() {
            if (IsGroupValid) {
                if (Parent != null) {
                    // Content & container rects
                    var requestedSize = ContentRect.size;
                    ContentRect = TotalOffset.Remove(new Rect(Parent.NextEntryPosition, requestedSize));
                    ContainerRect = Utility.RectIntersection(Parent.GetVisibleContentRect(requestedSize.x, requestedSize.y), ContentRect);

                    // Content offset
                    NextEntryPosition += ContentRect.position;
                }
                else {
                    // Content & container rects
                    ContainerRect = TotalOffset.Remove(LayoutEngine.GetRectFromUnityLayout(ContentRect.height, ContentRect.width));
                    ContentRect = ContainerRect;

                    // Content offset
                    NextEntryPosition += ContentRect.position;
                }

                IsGroupValid = ContainerRect.IsValid() && Event.current.type != EventType.Used;
                if (IsGroupValid) {
                    IsLayoutEvent = false;

                    // Clipspace extra calculations
                    if(Clip) {
                        GUI.BeginClip(ContainerRect);
                        // Clipspace changes world space to local space
                        // Coordinates should be recalculated
                        _clipWorldPositionOffset = ContainerRect.position;

                        ContentRect.position -= ContainerRect.position;
                        NextEntryPosition -= ContainerRect.position;

                        ContainerRect.position = Vector2.zero;
                    }
                }
            }
        }

        // Getting rects
        public bool GetNextEntryRect(float width, float height, out Rect rect) {
            var currentEntryPosition = NextEntryPosition;

            if (width < 0f) width = AutomaticWidth;
            if(PrepareNextRect(width, height) && IsGroupValid) {
                rect = new Rect(currentEntryPosition, new Vector2(width, height));
                return true;
            }
            rect = new Rect();
            return false;
        }

        public Rect GetNextEntryRect(float width, float height) {
            GetNextEntryRect(width, height, out Rect rect);
            return rect;
        }

        private Rect GetVisibleContentRect(float width, float height) {
            if (width <= 0f) width = AutomaticWidth;

            if (PrepareNextRect(width, height)) {
                return ContainerRect;
            }

            return InvalidRect;
        }

        protected abstract bool PrepareNextRect(float width, float height);

        public void RegisterArray(float elementHeight, int count) {
            RegisterArray(LayoutEngine.AutoWidth, elementHeight, count);
        }
        public abstract void RegisterArray(float elemWidth, float elemHeight, int count);

        public Rect GetContentRect(bool fullRect = false) {
            var output = ContentRect;

            if(fullRect) {
                output = TotalOffset.Add(output);
            }

            // World pos -> Local pos
            if(Clip) {
                output.position -= _clipWorldPositionOffset;
            }

            return output;
        }

        public void ResetLayout() {
            ContentRect.width = -1;
            ContentRect.height = 0;

            NextEntryPosition = Vector2.zero;

            EntriesCount = 0;
        }
    
        internal void BeginLayout(LayoutGroup parent) {
            Parent = parent;
            AutomaticWidth = GetAutomaticWidth();
            IsLayoutEvent = true;
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
                ContainerRect.position = _clipWorldPositionOffset;
                ContentRect.position += _clipWorldPositionOffset;
            }
            EndNonLayoutRoutine();
        }
        protected virtual void EndNonLayoutRoutine() {

        }
    }
}