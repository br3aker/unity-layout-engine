using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        internal class VerticalFadeGroup : VerticalLayoutGroupBase {
            private float _faded;

            public VerticalFadeGroup(float faded, GUIStyle style) : base(style) {
                _faded = faded;
            }

            protected override void CalculateLayoutData() {
                if (CurrentEventType == EventType.Layout) {
                    TotalHeight += ContentOffset * (EntriesCount - 1);

                    TotalHeight *= _faded;
                }
            }

            internal override Rect GetRect(float height) {
                return GetRect(height, EditorGUIUtility.currentViewWidth);
            }

            protected override Rect GetActualRect(float height, float width) {
                if (NextEntryY > TotalHeight) {
                    return InvalidRect;
                }
                return new Rect(
                    NextEntryX,
                    NextEntryY,
                    width,
                    height
                );
            }
        }
        
        public static bool BeginVerticalFadeGroup(float faded, GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalFadeGroup(faded, style);
                SubscribedForLayout.Enqueue(layoutGroup);
            }
            else {
                layoutGroup = SubscribedForLayout.Dequeue();
                layoutGroup.RetrieveLayoutData(eventType);
            }
            
            ActiveGroupStack.Push(layoutGroup);
            _topGroup = layoutGroup;

//            return !Mathf.Approximately(faded, 0f) && layoutGroup.IsGroupValid;
            return layoutGroup.IsGroupValid;
        }
        public static bool BeginVerticalFadeGroup(float faded) {
            return BeginVerticalFadeGroup(faded, ExtendedEditorGUI.Resources.LayoutGroup.VerticalFadeGroup);
        }

        public static void EndVerticalFadeGroup() {
            var topGroup = EndLayoutGroup();
            if(!(topGroup is VerticalFadeGroup)) throw new Exception($"Group type mismatch: Expected {nameof(VerticalFadeGroup)} | Got {topGroup.GetType().Name}");
        }
    }
}