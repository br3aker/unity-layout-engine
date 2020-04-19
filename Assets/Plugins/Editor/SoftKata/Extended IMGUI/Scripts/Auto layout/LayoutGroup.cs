using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace SoftKata.ExtendedEditorGUI {
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
                    Layout.GetRectFromUnityLayout(ContentRectInternal.height, ContentRectInternal.width);
                }
            }
        }
        
        // non-Layout event
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
                ContainerRectInternal = TotalOffset.Remove(Layout.GetRectFromUnityLayout(ContentRectInternal.height, ContentRectInternal.width));
                ContentRectInternal = ContainerRectInternal;
            }

            if (IsGroupValid) {
                IsLayoutEvent = false;

                // Clipspace
                if(Clip) {
                    GUI.BeginClip(ContainerRectInternal);
                    // Clipspace changes world space to local space
                    _clipWorldPositionOffset = ContainerRectInternal.position;
                    ContentRectInternal.position -= ContainerRectInternal.position;

                    ContainerRectInternal.position = Vector2.zero;
                }

                // Content offset
                NextEntryPosition += ContentRectInternal.position;

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

    public partial class LayoutGroup {
        private static Resources _resources;
        protected static Resources StyleResources => _resources ?? (_resources = new Resources());

        protected class Resources {
            private const string LightSkinSubPath = "/Light Layout Engine skin.guiskin";
            private const string DarkSkinSubPath = "/Dark Layout Engine skin.guiskin";

            public GUIStyle VerticalGroup;
            public GUIStyle VerticalFadeGroup;
            public GUIStyle Treeview;

            public GUIStyle HorizontalGroup;
            public GUIStyle HorizontalRestrictedGroup;

            public GUIStyle ScrollGroup;
            
            internal Resources() {
                var skinPath = ExtendedEditorGUI.PluginPath + (EditorGUIUtility.isProSkin ? DarkSkinSubPath : LightSkinSubPath);
                var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
                
                VerticalGroup = skin.GetStyle("Vertical group");
                VerticalFadeGroup = skin.GetStyle("Vertical fade group");
                ScrollGroup = skin.GetStyle("Scroll group");
                Treeview = skin.GetStyle("Treeview");
                HorizontalGroup = skin.GetStyle("Horizontal group");
                HorizontalRestrictedGroup = skin.GetStyle("Horizontal restricted group");
            }
        }
    }
}