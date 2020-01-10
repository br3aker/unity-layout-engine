using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalLayoutGroupBase : LayoutGroupBase {
            protected float ContentOffset;

            public VerticalLayoutGroupBase(GUIStyle style) : base(style) {
                ContentOffset = style.contentOffset.y;
            }

            internal sealed override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    TotalHeight += height;
                    TotalWidth = Mathf.Max(TotalWidth, width);
                    return InvalidRect;
                    return LayoutDummyRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }
                
                
                var entryRect = GetActualRect(height, width);
                NextEntryY += height + ContentOffset;
                return entryRect;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalHeight += elementHeight * count;
                TotalWidth = Mathf.Max(TotalWidth, elementWidth);
            }

            protected virtual Rect GetActualRect(float height, float width) {
                if (NextEntryY + height < 0 || NextEntryY > FullRect.height) {
                    return InvalidRect;
                }

                return new Rect(NextEntryX, NextEntryY, width, height);
            }
        }

        public static bool BeginVerticalGroup(GUIStyle style) {
            var eventType = Event.current.type;
            LayoutGroupBase layoutGroup;
            if (eventType == EventType.Layout) {
                layoutGroup = new VerticalLayoutGroupBase(style);
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
        public static bool BeginVerticalGroup() {
            return BeginVerticalGroup(ExtendedEditorGUI.Resources.LayoutGroup.VerticalGroup);
        }

        public static void EndVerticalGroup() {
            EndLayoutGroup();
        }
    }
}