using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class HorizontalLayoutGroupBase : LayoutGroupBase {
            public HorizontalLayoutGroupBase(GUIStyle style) : this(EditorGUIUtility.currentViewWidth, style) { }
            
            public HorizontalLayoutGroupBase(float defaultEntryWidth, GUIStyle style) : base(defaultEntryWidth, style) {}

            internal override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    TotalContainerWidth += width;
                    TotalContainerHeight = Mathf.Max(TotalContainerHeight, height);
                    return InvalidRect;
                    return LayoutDummyRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }

                var entryRect = GetActualRect(height, width);
                NextEntryX += width + ContentOffset.x;
                return entryRect;
            }
            
            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalContainerHeight = Mathf.Max(TotalContainerHeight, elementHeight);
                TotalContainerWidth += elementWidth * count;
            }

            protected virtual Rect GetActualRect(float height, float width) {
                if (NextEntryX + width < 0 || NextEntryX > FullRect.width) {
                    return InvalidRect;
                }
                
                return new Rect(NextEntryX, NextEntryY, width, height);
            }
        }

        public static bool BeginHorizontalGroup(GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new HorizontalLayoutGroupBase(style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
                layoutGroup.RegisterDebugData();
            }
            
            _topGroup = layoutGroup;

            return layoutGroup.IsGroupValid;
        }
        public static bool BeginHorizontalGroup() {
            return BeginHorizontalGroup(ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
        }

        public static void EndHorizontalGroup() {
            EndLayoutGroup();
        }
    }
}