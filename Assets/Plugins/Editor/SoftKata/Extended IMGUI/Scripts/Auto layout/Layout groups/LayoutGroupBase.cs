using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        internal abstract class LayoutGroupDataBase {
            protected struct LayoutEntry {
                internal float Height;
            }
            
            protected float _height;
            
            protected Queue<LayoutEntry> _entries = new Queue<LayoutEntry>();

            public int Count => _entries.Count;
            public virtual float TotalHeight => _height;

            public abstract void AddEntry(float height);

            public Rect FetchNextRect(float x, float y, float width, float height) {
                _entries.Dequeue();
                return new Rect(x, y, width, height);
            }

            public static implicit operator bool(LayoutGroupDataBase layoutGroupData) {
                return layoutGroupData._entries.Count > 0;
            }
        }

        internal abstract class LayoutGroupBase {
            protected readonly bool IsFixedWidth;
            protected readonly float FixedWidth;
            
            
            internal LayoutGroupBase Parent { get; private set; }

            protected EventType _eventType;

            internal Rect FullRect;
            protected LayoutGroupDataBase LayoutData;
            private bool _notEmptyGroup;

            protected float _nextEntryX;
            protected float _nextEntryY;

            protected LayoutGroupBase(LayoutGroupBase parent, GUIStyle style) {
                Parent = parent;

                _eventType = EventType.Layout;
                
                FixedWidth = style.fixedWidth;
                IsFixedWidth = !Mathf.Approximately(FixedWidth, 0f);
            }

            internal void PushLayoutRequest() {
                _eventType = Event.current.type;
                _notEmptyGroup = LayoutData;
                if (_notEmptyGroup) {
                    PushLayoutEntries();
                    GUI.BeginClip(FullRect);
                }
            }

            protected abstract void PushLayoutEntries();

            internal abstract Rect GetRect(float height);

            internal void EndGroup() {
                if (_notEmptyGroup) {
                    EndGroupRoutine();
                    GUI.EndClip();
                }
            }

            protected virtual void EndGroupRoutine() { }
        }
        
        internal static LayoutGroupBase EndLayoutGroup() {
            var eventType = Event.current.type;

            var currentGroup = TopGroup;
            if (eventType == EventType.Layout) {
                currentGroup.PushLayoutRequest();
            }
            
            currentGroup.EndGroup();
            TopGroup = currentGroup.Parent;
            ActiveGroupStack.Pop();

            return currentGroup;
        }
    }
}