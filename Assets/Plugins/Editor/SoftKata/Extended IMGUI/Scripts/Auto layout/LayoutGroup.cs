using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    // Main logic
    public abstract partial class LayoutGroup {
        protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroup).GetHashCode();


        internal LayoutGroup Parent { get; private set; }

        public readonly GUIStyle Style;

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

        protected Rect ContainerRectInternal;
        protected Rect ContentRectInternal;

        public Rect ContentRect => ContentRectInternal;

        // Background texture rendering
        private Texture _backgroundTexture;
        private int _left;
        private int _right;
        private int _bottom;
        private int _top;


        // Automatic width for entries
        public float AutomaticWidth {get; protected set;}
        protected virtual float GetAutomaticWidth() => AvailableWidth - TotalOffset.horizontal;
        protected float AvailableWidth => Parent?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);

        // Constructor
        protected LayoutGroup(GUIStyle style, bool ignoreConstaints) {
            Style = style;

            _backgroundTexture = style.normal.background;
            var overflow = style.overflow;
            _left = overflow.left;
            _right = overflow.right;
            _bottom = overflow.bottom;
            _top = overflow.top;

            TotalOffset = new RectOffset();
            if(ignoreConstaints) return;
            TotalOffset.Accumulate(style.margin);
            TotalOffset.Accumulate(style.border);
            TotalOffset.Accumulate(style.padding);
        }

        // Layout event
        protected abstract void PreLayoutRequest();
        internal void BeginLayout(LayoutGroup parent) {
            ContentRectInternal.width = -1;
            ContentRectInternal.height = 0;

            NextEntryPosition = Vector2.zero;
            EntriesCount = 0;

            Parent = parent;
            IsLayoutEvent = true;
            AutomaticWidth = GetAutomaticWidth();
        }
        internal void EndLayout() {
            if (IsGroupValid = EntriesCount > 0) {
                PreLayoutRequest();

                if(Parent != null) {
                    ++Parent.EntriesCount;
                    Parent.RegisterEntry(ContentRectInternal.width, ContentRectInternal.height);
                }
                else {
                    Layout.GetRectFromUnityLayout(ContentRectInternal.width, ContentRectInternal.height);
                }
            }
        }
        
        // Non-Layout event
        private void CalculateNonLayoutData() {
                IsLayoutEvent = false;

                // Clipspace
                if(Clip) {
                    GUI.BeginClip(ContainerRectInternal);
                    // Clipspace changes world space to local space
                    _clipWorldPositionOffset = ContainerRectInternal.position;
                    ContentRectInternal.position -= ContainerRectInternal.position;

                    ContainerRectInternal.position = Vector2.zero;
                }

                // Background image rendering
                if(Event.current.type == EventType.Repaint && _backgroundTexture) {
                    Graphics.DrawTexture(
                        TotalOffset.Add(ContainerRectInternal),
                        _backgroundTexture,
                        _left, _right, _top, _bottom
                    );
                }

                // Content offset
                NextEntryPosition = ContentRectInternal.position;
        }
        internal virtual bool BeginNonLayout() {
            if (Parent != null) {
                var requestedSize = ContentRectInternal.size;
                if(IsGroupValid = Parent.QueryEntry(requestedSize.x, requestedSize.y, out Rect requestedRect)) {
                    // Content & container rects
                    ContentRectInternal = TotalOffset.Remove(requestedRect);
                    ContainerRectInternal = Utility.RectIntersection(ContentRectInternal, Parent.ContainerRectInternal);
                }
            }
            else {
                // Content & container rects
                ContainerRectInternal = TotalOffset.Remove(Layout.GetRectFromUnityLayout(ContentRectInternal.width, ContentRectInternal.height));
                ContentRectInternal = ContainerRectInternal;
            }

            if (IsGroupValid) {
                CalculateNonLayoutData();
                return true;
            }
            return false;
        } 
        internal virtual void EndNonLayout() {
            if(Clip) {
                GUI.EndClip();
                ContainerRectInternal.position = _clipWorldPositionOffset;
                ContentRectInternal.position += _clipWorldPositionOffset;
            }
        }

        // This prepares layout group for rect querying without actual layout stage
        public void BeginAbsoluteLayout(Rect rect) {
            // Content & container rects
            ContainerRectInternal = TotalOffset.Remove(rect);
            ContentRectInternal = ContainerRectInternal;
            CalculateNonLayoutData();
        }

        // Registering entry(s)
        protected abstract void RegisterEntry(float width, float height);
        public abstract void RegisterEntriesArray(float elemWidth, float elemHeight, int count);
        public void RegisterEntriesArray(float elementHeight, int count) {
            RegisterEntriesArray(AutomaticWidth, elementHeight, count);
        }
        
        // Getting entry
        protected abstract bool QueryAndOcclude(Vector2 entrySize);
        private bool QueryEntry(float width, float height, out Rect rect) {
            rect = new Rect(NextEntryPosition, new Vector2(width, height));
            return QueryAndOcclude(rect.size);
        }

        // Getting actual rect from layout group
        public bool GetRect(float width, float height, out Rect rect) {
            if(IsLayoutEvent) {
                ++EntriesCount;
                RegisterEntry(width, height);
                rect = new Rect();
                return false;
            }
            return QueryEntry(width, height, out rect);
        }
        public bool GetRect(float height, out Rect rect) {
            return GetRect(AutomaticWidth, height, out rect);
        }
        public Rect GetRect(float width, float height) {
            GetRect(width, height, out var rect);
            return rect;
        }
        public Rect GetRect(float height) {
            return GetRect(AutomaticWidth, height);
        }
    }
}