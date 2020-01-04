using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class AutoLayout {
        internal class HorizontalLayoutGroupBase : LayoutGroupBase {
            protected float ContentOffset;

            public HorizontalLayoutGroupBase(GUIStyle style) : base(style) {
                ContentOffset += style.contentOffset.x;
            }
            protected override void CalculateLayoutData() {
                TotalWidth += ContentOffset * (EntriesCount - 1);
            }

            internal override Rect GetRect(float height) {
                return GetRect(height, EditorGUIUtility.currentViewWidth);
            }

            internal override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    TotalWidth += width;
                    TotalHeight = Mathf.Max(TotalHeight, height);
                    return LayoutDummyRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }

                var entryRect = GetActualRect(height, width);
                NextEntryX += width + ContentOffset;
                return entryRect;
            }

            protected virtual Rect GetActualRect(float height, float width) {
                return new Rect(
                    NextEntryX,
                    NextEntryY,
                    width,
                    height
                );
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
            }
            
            ActiveGroupStack.Push(layoutGroup);
            TopGroup = layoutGroup;

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