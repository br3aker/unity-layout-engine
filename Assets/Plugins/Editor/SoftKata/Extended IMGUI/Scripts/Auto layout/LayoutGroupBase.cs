using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
//        public class LayoutGroupScope : IDisposable {
//            private LayoutGroup _group;
//            
//            private EventType _creationEventType;
//
//            private float _indentOffset;
//
//            internal LayoutGroupScope(LayoutGroup group, int indent, EventType currentEventType) {
//                _group = group;
//                _creationEventType = currentEventType;
//                
//                if (Event.current.type != EventType.Layout) {
//                    _group.PushLayoutRequest();
//                }
//                ActiveGroupStack.Push(_group);
//                
//                var deltaIndent = indent * IndentStep;
//                _indentOffset += deltaIndent;
//                Indent += deltaIndent;
//                
//                group.Parent = TopGroup;
//                TopGroup = _group;
//            }
//
//            public void Dispose() {
//                if (Event.current.type == EventType.Layout) {
//                    _group.PushLayoutRequest();
//                }
//                Indent -= _indentOffset;
//
//                TopGroup = _group.Parent;
//                _group.EndGroup();
//                ActiveGroupStack.Pop();
//            }
//        }

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

        internal abstract class LayoutGroup {
            protected readonly bool IsFixedWidth;
            protected readonly float FixedWidth;
            
            
            internal LayoutGroup Parent { get; private set; }

            internal Rect FullRect;
            protected LayoutGroupDataBase LayoutData;

            protected float _nextEntryX;
            protected float _nextEntryY;

            protected LayoutGroup(LayoutGroup parent, GUIStyle style) {
                Parent = parent;
                
                FixedWidth = style.fixedWidth;
                IsFixedWidth = !Mathf.Approximately(FixedWidth, 0f);
            }

            internal abstract void PushLayoutRequest();

            internal abstract Rect GetRect(float height);

            internal virtual void EndGroup() {
                GUI.EndClip();
            }
        }
    }
}