using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.Editor {
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

        protected bool IsLayoutEvent = true;

        // entries layout data
        protected Vector2 EntriesRequestedSize;

        protected Vector2 NextEntryPosition;

        protected Rect ContainerRectInternal;
        protected Rect ContentRectInternal;

        public Rect ContentRect => ContentRectInternal;

        private bool _isLayoutDirty = true;

        // Background texture rendering
        private Texture _backgroundTexture;
        private int _left;
        private int _right;
        private int _bottom;
        private int _top;


        // Automatic width for entries
        public float AutomaticWidth {get; protected set;}
        protected virtual float CalculateAutomaticContentWidth() {
            return AvailableWidth - TotalOffset.horizontal;
        }
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

        // Returns [true] if layout must be recalculated
        // Returns [false] if layout can be skipped
        internal bool BeginLayout(LayoutGroup parent) {
            if(parent == null) {
                if(_isLayoutDirty) {
                    BeginLayoutInternal(parent);
                    return true;
                }
                Layout.GetRectFromUnityLayout(EntriesRequestedSize.x, EntriesRequestedSize.y);
                return false;
            }
            else if(parent._isLayoutDirty) {
                _isLayoutDirty = true;
                BeginLayoutInternal(parent);
                return true;
            }

            return false;
        }
        internal void BeginLayoutInternal(LayoutGroup parent) {
            EntriesRequestedSize = Vector2.zero;

            EntriesCount = 0;

            Parent = parent;
            IsLayoutEvent = true;
            
            AutomaticWidth = CalculateAutomaticContentWidth();
        }
        internal void EndLayout() {
            if (EntriesCount > 0) {
                PreLayoutRequest();

                if(Parent != null) {
                    ++Parent.EntriesCount;
                    Parent.RegisterEntry(EntriesRequestedSize.x, EntriesRequestedSize.y);
                }
                else {
                    Layout.GetRectFromUnityLayout(EntriesRequestedSize.x, EntriesRequestedSize.y);
                }
            }
        
            _isLayoutDirty = false;
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
                        TotalOffset.Add(ContentRectInternal),
                        _backgroundTexture,
                        _left, _right, _top, _bottom
                    );
                }

                // Content offset
                NextEntryPosition = ContentRectInternal.position;
        }
        internal virtual bool BeginNonLayout() {
            if (Parent != null) {
                var isGroupValid = Parent.QueryEntry(EntriesRequestedSize.x, EntriesRequestedSize.y, out Rect requestedRect);
                if(!isGroupValid) return false;
                
                // Content & container rects
                ContentRectInternal = TotalOffset.Remove(requestedRect);
                ContainerRectInternal = Utility.RectIntersection(ContentRectInternal, Parent.ContainerRectInternal);
            }
            else {
                // Content & container rects
                ContainerRectInternal = TotalOffset.Remove(Layout.GetRectFromUnityLayout(EntriesRequestedSize.x, EntriesRequestedSize.y));
                ContentRectInternal = ContainerRectInternal;
            }

            CalculateNonLayoutData();
            return true;
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
    
        public void MarkLayoutDirty() {
            _isLayoutDirty = true;
            Parent?.MarkLayoutDirty();
        }
    }
}