using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    // TODO: fix flexible horizontal group
    // TODO: fix horizontal group
    // TODO: implement automatic height in parented to horizontal-like group
    public abstract class LayoutGroup {
        protected static readonly int LayoutGroupControlIdHint = nameof(LayoutGroup).GetHashCode();
        
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
        protected LayoutGroup(GUIStyle style, bool ignoreConstaints) {
            TotalOffset = new RectOffset();

            if(ignoreConstaints) return;
            TotalOffset.Accumulate(style.margin);
            TotalOffset.Accumulate(style.border);
            TotalOffset.Accumulate(style.padding);
        }

        // Layout
        protected abstract void PreLayoutRequest();
        private void RequestLayout() {
            if (IsGroupValid = EntriesCount > 0) {
                PreLayoutRequest();

                if(Parent != null) {
                    Parent.PrepareNextRect(ContentRect.width, ContentRect.height);
                }
                else {
                    LayoutEngine.GetRectFromUnityLayout(ContentRect.height, ContentRect.width);
                }
            }
        }

        // Non-layout
        internal void RetrieveLayoutData() {
            if (Event.current.type != EventType.Used && IsGroupValid) {
                if (Parent != null) {
                    var requestedSize = ContentRect.size;
                    if(IsGroupValid = Parent.GetNextEntryRect(requestedSize.x, requestedSize.y, out Rect requestedRect)) {
                        // Content & container rects
                        ContentRect = TotalOffset.Remove(requestedRect);
                        ContainerRect = Utility.RectIntersection(ContentRect, Parent.ContainerRect);
                    }
                }
                else {
                    // Content & container rects
                    ContainerRect = TotalOffset.Remove(LayoutEngine.GetRectFromUnityLayout(ContentRect.height, ContentRect.width));
                    ContentRect = ContainerRect;
                }

                if (IsGroupValid) {
                    IsLayoutEvent = false;

                    // Content offset
                    NextEntryPosition += ContentRect.position;

                    // Clipspace extra calculations
                    if(Clip) {
                        GUI.BeginClip(ContainerRect);
                        // Clipspace changes world space to local space
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
    

        // experimental APi
        protected abstract void _RegisterEntry(float width, float height);

        protected abstract bool _EntryQueryCallback(Vector2 entrySize);
        private bool _QueryEntry(float width, float height, out Rect rect) {
            rect = new Rect(NextEntryPosition, new Vector2(width, height));
            return _EntryQueryCallback(rect.size);
        }

        public bool _GetRect(float height, float width, out Rect rect) {
            if(width < 0f) width = AutomaticWidth;
            if(IsLayoutEvent) {
                ++EntriesCount;
                _RegisterEntry(width, height);
                rect = new Rect();
                return false;
            }
            return _QueryEntry(width, height, out rect);
        }
        public Rect _GetRect(float height, float width) {
            _GetRect(height, width, out var rect);
            return rect;
        }
    }
}