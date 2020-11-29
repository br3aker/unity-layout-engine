using System.Text;
using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {

    public abstract class LayoutGroup {
        // Generated with "LayoutGroup" string with .net GetHashCode method
        protected const int LayoutGroupControlIdHint = -1416898402;

        // Parent
        internal LayoutGroup Parent { get; private set; }

        // Background texture
        public GUIStyle Style {get;}
        private readonly bool _hasBackground;

        // Layout settings
        protected RectOffset ContentOffset;
        public float SpaceBetweenEntries {get; protected set;}

        // Clip
        public bool Clip {get; set;}
        protected Vector2 ClipWorldPositionOffset;

        // entries layout data
        protected int EntriesCount;
        protected Vector2 RequestedSize;
        protected Vector2 NextEntryPosition;

        // Actual occluded rect
        protected Rect ContainerRectInternal;
        // Requested rect
        protected Rect ContentRectInternal;

        // Layout build flags
        protected bool IsLayoutEvent = true;

        public enum LayoutRebuildingOption {
            Cached,
            NodeReduild,
            FullRebuild
        }

        internal LayoutRebuildingOption LayoutState = LayoutRebuildingOption.FullRebuild;


        // Automatic width for entries
        protected virtual float CalculateAutomaticContentWidth() {
            return AvailableWidth - ContentOffset.horizontal;
        }
        protected float AvailableWidth => Parent?.AutomaticWidth ?? (EditorGUIUtility.currentViewWidth - 2);

        
        public Rect ContentRect => ContentRectInternal;
        
        public float AutomaticWidth {get; protected set;}

        // Constructor
        protected LayoutGroup(GUIStyle style, bool ignoreConstaints) {
            Style = style;

            _hasBackground = style.normal.background != null;

            ContentOffset = new RectOffset();
            if(ignoreConstaints) return;
            ContentOffset.Accumulate(style.padding);
        }

        // Layout event
        protected abstract void PreLayoutRequest();

        // Returns [true] if layout must be recalculated
        // Returns [false] if layout can be skipped
        internal bool BeginLayout(LayoutGroup parent) {
            var hasParent = parent != null;
            if(hasParent && parent.LayoutState == LayoutRebuildingOption.FullRebuild) {
                LayoutState = LayoutRebuildingOption.FullRebuild;
            }

            if (LayoutState > LayoutRebuildingOption.Cached) {
                BeginLayoutInternal(parent);
                return true;
            }

            if(hasParent) {
                Parent = parent;
                ++parent.EntriesCount;
                parent.RegisterEntry(RequestedSize.x, RequestedSize.y);
            }
            else {
                Layout.GetRectFromUnityLayout(RequestedSize.x, RequestedSize.y);
            }
            return false;
        }
        internal void BeginLayoutInternal(LayoutGroup parent) {
            RequestedSize = Vector2.zero;

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
                    Parent.RegisterEntry(RequestedSize.x, RequestedSize.y);
                }
                else {
                    Layout.GetRectFromUnityLayout(RequestedSize.x, RequestedSize.y);
                }
            }

            LayoutState = LayoutRebuildingOption.Cached;
        }

        // Non-Layout event
        private void CalculateNonLayoutData() {
            IsLayoutEvent = false;

            // Background image rendering
            if(Event.current.type == EventType.Repaint && _hasBackground) {
                Style.Draw(
                    ContentOffset.Add(ContentRectInternal),
                    false, false, false, false
                );
            }

            // Clipspace
            if(Clip) {
                GUI.BeginClip(ContainerRectInternal);
                // Clipspace changes world space to local space
                ClipWorldPositionOffset = ContainerRectInternal.position;
                ContentRectInternal.position -= ContainerRectInternal.position;

                ContainerRectInternal.position = Vector2.zero;
            }

            // Content offset
            NextEntryPosition = ContentRectInternal.position;
        }
        internal virtual bool BeginNonLayout() {
            if (Parent != null) {
                var isGroupValid = Parent.QueryEntry(RequestedSize.x, RequestedSize.y, out Rect requestedRect);
                if(!isGroupValid) return false;
                
                // Content & container rects
                ContentRectInternal = ContentOffset.Remove(requestedRect);
                ContainerRectInternal = ContentRectInternal.Intersection(Parent.ContainerRectInternal);
            }
            else {
                // Content & container rects
                ContainerRectInternal = ContentOffset.Remove(Layout.GetRectFromUnityLayout(RequestedSize.x, RequestedSize.y));
                ContentRectInternal = ContainerRectInternal;
            }

            CalculateNonLayoutData();
            return true;
        } 
        internal virtual void EndNonLayout() {
            if(Clip) {
                GUI.EndClip();
            }
        }

        // This prepares layout group for rect querying without actual layout stage
        public void BeginAbsoluteLayout(Rect rect) {
            // Content & container rects
            ContainerRectInternal = ContentOffset.Remove(rect);
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
    
        private void MarkLayoutDirty(LayoutRebuildingOption option) {
            LayoutState = option;
            Parent?.MarkLayoutDirty(LayoutRebuildingOption.NodeReduild);
        }

        public void MarkLayoutDirty() {
            MarkLayoutDirty(LayoutRebuildingOption.FullRebuild);
        }
    }
}