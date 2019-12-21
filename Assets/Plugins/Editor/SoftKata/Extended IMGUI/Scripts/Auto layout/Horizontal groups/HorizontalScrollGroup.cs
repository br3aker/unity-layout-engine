using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static void BeginHorizontalScroll(float elemWidth, float scrollPos) {
            BeginHorizontalScroll(elemWidth, scrollPos, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
        }
        public static void BeginHorizontalScroll(float elemWidth, float scrollPos, GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new HorizontalScrollGroup(elemWidth, scrollPos, TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;
        }
        public static void EndHorizontalScroll() {
            var eventType = Event.current.type;

            if (eventType == EventType.Layout) {
                TopGroup.PushLayoutRequest();
            }
            
            TopGroup.EndGroup();
            TopGroup = TopGroup.Parent;
            ActiveGroupStack.Pop();
        }

        private class HorizontalScrollGroup : HorizontalLayoutGroup {
            private float _elemWidth;
            private float _containerWidth;
            private readonly float _scrollPos;

            public HorizontalScrollGroup(float elemWidth, float scrollPos, LayoutGroup parent, GUIStyle style) : base(parent, style) {
                _elemWidth = elemWidth;
                _containerWidth = EditorGUIUtility.currentViewWidth;
                _scrollPos = scrollPos;
            }

            internal override void PushLayoutRequest() {
                if (LayoutData) {
                    FullRect = Parent?.GetRect(LayoutData.TotalHeight) ?? RequestIndentedRect(LayoutData.TotalHeight);
                    
                    _nextEntryX = Mathf.Lerp(0, _containerWidth - (_elemWidth * LayoutData.Count + _contentHorizontalGap * (LayoutData.Count - 1)), _scrollPos);

                    _entryWidth = _elemWidth;

                    GUI.BeginClip(FullRect);
                }
            }
            
            internal override void EndGroup() {
                GUI.EndClip();
            }
        }
    }
}