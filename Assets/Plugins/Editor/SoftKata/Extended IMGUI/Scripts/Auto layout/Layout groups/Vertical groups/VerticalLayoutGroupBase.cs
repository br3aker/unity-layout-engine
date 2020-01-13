using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        internal class VerticalLayoutGroupBase : LayoutGroupBase {
            public VerticalLayoutGroupBase(GUIStyle style) : this(EditorGUIUtility.currentViewWidth, style) { }
            
            public VerticalLayoutGroupBase(float defaultEntryWidth, GUIStyle style) : base(defaultEntryWidth, style) { }

            internal sealed override Rect GetRect(float height, float width) {
                if (CurrentEventType == EventType.Layout) {
                    EntriesCount++;
                    TotalContainerHeight += height;
                    TotalContainerWidth = Mathf.Max(TotalContainerWidth, width);
                    return InvalidRect;
                    return LayoutDummyRect;
                }

                if (!IsGroupValid) {
                    return InvalidRect;
                }
                
                
                var entryRect = GetActualRect(height, width);
                NextEntryY += height + ContentOffset.y;
                return entryRect;
            }

            internal override void RegisterRectArray(float elementHeight, float elementWidth, int count) {
                EntriesCount += count;
                TotalContainerHeight += elementHeight * count;
                TotalContainerWidth = Mathf.Max(TotalContainerWidth, elementWidth);
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