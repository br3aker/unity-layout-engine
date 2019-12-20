using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static LayoutGroupScope HorizontalScrollScope(float elemWidth, float scrollPos, int indent = 1) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new HorizontalScrollGroup(elemWidth, scrollPos, ExtendedEditorGUI.Resources.LayoutGroup.HorizontalGroup);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
            }
            
            return new LayoutGroupScope(group, indent, eventType);
        }

        private class HorizontalScrollGroup : HorizontalLayoutGroup {
            private float _elemWidth;
            private float _containerWidth;
            private readonly float _scrollPos;

            public HorizontalScrollGroup(float elemWidth, float scrollPos, GUIStyle style) : base(style) {
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