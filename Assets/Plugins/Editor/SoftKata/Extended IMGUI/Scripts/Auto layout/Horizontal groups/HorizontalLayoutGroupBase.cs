using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static void BeginHorizontalScope(GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroup group;
            if (eventType == EventType.Layout) {
                group = new HorizontalLayoutGroup(TopGroup, style);
                SubscribedForLayout.Enqueue(group);
            }
            else {
                group = SubscribedForLayout.Dequeue();
                group.PushLayoutRequest();
            }

            ActiveGroupStack.Push(group);
            TopGroup = group;
        }
        public static void EndHorizontalScope() {
            var eventType = Event.current.type;

            if (eventType == EventType.Layout) {
                TopGroup.PushLayoutRequest();
            }
            
            TopGroup.EndGroup();
            TopGroup = TopGroup.Parent;
            ActiveGroupStack.Pop();
        }

        internal class HorizontalLayoutGroupData : LayoutGroupDataBase {
            public override void AddEntry(float height) {
                _height = Mathf.Max(_height, height);
                _entries.Enqueue(new LayoutEntry{Height = height});
            }
        }

        private class HorizontalLayoutGroup : LayoutGroup {
            protected float _contentHorizontalGap;

            protected float _entryWidth;
            
            public HorizontalLayoutGroup(LayoutGroup parent, GUIStyle style) : base(parent, style) {
                _contentHorizontalGap = style.contentOffset.x;
                LayoutData = new HorizontalLayoutGroupData();
            }

            protected virtual float GetContainerWidth() {
                return FullRect.width;
            }
            
            internal override void PushLayoutRequest() {
                if (LayoutData) {
                    float containerHeight = LayoutData.TotalHeight;
                    FullRect = Parent?.GetRect(containerHeight) ?? RequestIndentedRect(containerHeight);

                    int entriesCount = LayoutData.Count;
                    _entryWidth = (FullRect.width - _contentHorizontalGap * (entriesCount - 1)) / entriesCount;
                    FullRect.width = GetContainerWidth();
                }
                GUI.BeginClip(FullRect);
            }

            internal override Rect GetRect(float height) {
                if (Event.current.type == EventType.Layout) {
                    LayoutData.AddEntry(height);
                    return LayoutDummyRect;
                }

                var entryRect = LayoutData.FetchNextRect(_nextEntryX, _nextEntryY, _entryWidth, height);
                
                _nextEntryX += _entryWidth + _contentHorizontalGap;

                return _nextEntryX > 0f && entryRect.x < FullRect.width ? entryRect : InvalidDummyRect;
            }

            internal override void EndGroup() {
                GUI.EndClip();
            }
        }
    }
}