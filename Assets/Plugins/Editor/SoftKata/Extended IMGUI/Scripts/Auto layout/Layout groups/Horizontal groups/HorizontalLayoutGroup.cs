using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        public static void BeginHorizontalScope(GUIStyle style) {
            var eventType = Event.current.type;

            LayoutGroupBase group;
            if (eventType == EventType.Layout) {
                group = new HorizontalLayoutGroup(style);
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
            EndLayoutGroup();
        }

        internal class HorizontalLayoutGroupData : LayoutGroupDataBase {
            public override void AddEntry(float height) {
                _height = Mathf.Max(_height, height);
                _entries.Enqueue(new LayoutEntry{Height = height});
            }
        }

        private class HorizontalLayoutGroup : LayoutGroupBase {
            protected float _contentHorizontalGap;

            protected float _entryWidth;
            
            public HorizontalLayoutGroup(GUIStyle style) : base(style) {
                _contentHorizontalGap = style.contentOffset.x;
                LayoutData = new HorizontalLayoutGroupData();
            }

            protected virtual float GetContainerWidth() {
                return FullRect.width;
            }
            
            protected override void PushLayoutEntries() {
                float containerHeight = LayoutData.TotalHeight;
                FullRect = Parent?.GetRect(containerHeight) ?? RequestIndentedRect(containerHeight);

                int entriesCount = LayoutData.Count;
                _entryWidth = (FullRect.width - _contentHorizontalGap * (entriesCount - 1)) / entriesCount;
                FullRect.width = GetContainerWidth();
            }

            internal override Rect GetRect(float height) {
                if (_eventType == EventType.Layout) {
                    LayoutData.AddEntry(height);
                    return LayoutDummyRect;
                }

                var entryRect = LayoutData.FetchNextRect(_nextEntryX, _nextEntryY, _entryWidth, height);
                
                _nextEntryX += _entryWidth + _contentHorizontalGap;

                return _nextEntryX > 0f && entryRect.x < FullRect.width ? entryRect : InvalidDummyRect;
            }
        }
    }
}