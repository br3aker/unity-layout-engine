using System;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginVerticalGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterForLayout(new VerticalGroup(modifier, style));
            }

            return RetrieveNextGroup().IsGroupValid;
        }
        
        public static bool BeginVerticalGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalGroup(modifier, ExtendedEditorGUI.LayoutResources.VerticalGroup);
        }
        public static void EndVerticalGroup() {
            EndLayoutGroup<VerticalGroup>();
        }

        public class VerticalGroup : LayoutGroupBase {
            public VerticalGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                RequestedWidth = float.MinValue;
            }

            protected sealed override bool PrepareNextRect(float width, float height) {
                if (IsLayout) {
                    EntriesCount++;
                    RequestedWidth = Mathf.Max(RequestedWidth, width);
                    RequestedHeight += height;
                    return false;
                }

                if (!IsGroupValid) return false;

                NextEntryPosition.y += height + ContentOffset.y;

                // occlusion
                return CurrentEntryPosition.y + height >= VisibleAreaRect.y
                       && CurrentEntryPosition.y <= VisibleAreaRect.yMax;
            }

            internal override void RegisterArray(float elemWidth, float elemHeight, int count) {
                EntriesCount += count;
                RequestedWidth = Mathf.Max(RequestedWidth, elemWidth);
                RequestedHeight += elemHeight * count;
            }
        }

        public class VerticalScope : IDisposable {
            public readonly bool Valid;

            public VerticalScope(GroupModifier modifier, GUIStyle style) {
                Valid = BeginVerticalGroup(modifier, style);
            }

            public VerticalScope(GroupModifier modifier = GroupModifier.None) {
                Valid = BeginVerticalGroup(modifier);
            }

            public void Dispose() {
                EndVerticalGroup();
            }
        }
    }
}