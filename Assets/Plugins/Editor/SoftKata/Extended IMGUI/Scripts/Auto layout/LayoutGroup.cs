using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
    public abstract class LayoutGroup {
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

        protected Rect ContentRect;
        protected Rect ContainerRect;

        // Automatic width for entries
        public float AutomaticWidth {get; protected set;}
        protected virtual float GetAutomaticWidth() => AvailableWidth - TotalOffset.horizontal;
        protected float AvailableWidth => Parent?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);

        // Constructor
        protected LayoutGroup(GUIStyle style, bool ignoreConstaints) {
            Style = style;

            TotalOffset = new RectOffset();
            if(ignoreConstaints) return;
            TotalOffset.Accumulate(style.margin);
            TotalOffset.Accumulate(style.border);
            TotalOffset.Accumulate(style.padding);
        }

        public Rect GetContentRect(bool fullRect = false) {
            var output = ContentRect;

            // World pos -> Local pos
            if(Clip) {
                output.position -= _clipWorldPositionOffset;
            }

            return output;
        }
    
        // Layout event
        protected abstract void PreLayoutRequest();
        internal void BeginLayout(LayoutGroup parent) {
            ContentRect.width = -1;
            ContentRect.height = 0;

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
                    Parent.RegisterEntry(ContentRect.width, ContentRect.height);
                }
                else {
                    Layout.GetRectFromUnityLayout(ContentRect.height, ContentRect.width);
                }
            }
        }
        
        // non-Layout event
        internal virtual void BeginNonLayout() {
            if (Event.current.type != EventType.Used && IsGroupValid) {
                if (Parent != null) {
                    var requestedSize = ContentRect.size;
                    if(IsGroupValid = Parent.QueryEntry(requestedSize.x, requestedSize.y, out Rect requestedRect)) {
                        // Content & container rects
                        ContentRect = TotalOffset.Remove(requestedRect);
                        ContainerRect = Utility.RectIntersection(ContentRect, Parent.ContainerRect);
                    }
                }
                else {
                    // Content & container rects
                    ContainerRect = TotalOffset.Remove(Layout.GetRectFromUnityLayout(ContentRect.height, ContentRect.width));
                    ContentRect = ContainerRect;
                }

                if (IsGroupValid) {
                    IsLayoutEvent = false;

                    // Clipspace
                    if(Clip) {
                        GUI.BeginClip(ContainerRect);
                        // Clipspace changes world space to local space
                        _clipWorldPositionOffset = ContainerRect.position;
                        ContentRect.position -= ContainerRect.position;

                        ContainerRect.position = Vector2.zero;
                    }

                    // Content offset
                    NextEntryPosition += ContentRect.position;
                }
            }
        } 
        internal virtual void EndNonLayout() {
            if(Clip) {
                GUI.EndClip();
                ContainerRect.position = _clipWorldPositionOffset;
                ContentRect.position += _clipWorldPositionOffset;
            }
        }
    
        // Registering entry(s)
        protected abstract void RegisterEntry(float width, float height);
        public abstract void RegisterEntriesArray(float elemWidth, float elemHeight, int count);
        public void RegisterEntriesArray(float elementHeight, int count) {
            RegisterEntriesArray(Layout.AutoWidth, elementHeight, count);
        }
        
        // Getting entry
        protected abstract bool QueryAndOcclude(Vector2 entrySize);
        private bool QueryEntry(float width, float height, out Rect rect) {
            rect = new Rect(NextEntryPosition, new Vector2(width, height));
            return QueryAndOcclude(rect.size);
        }

        // Getting actual rect from layout group
        public bool GetRect(float height, float width, out Rect rect) {
            if(width < 0f) width = AutomaticWidth;
            if(IsLayoutEvent) {
                ++EntriesCount;
                RegisterEntry(width, height);
                rect = new Rect();
                return false;
            }
            return QueryEntry(width, height, out rect);
        }
        public Rect GetRect(float height, float width) {
            GetRect(height, width, out var rect);
            return rect;
        }
    }
}