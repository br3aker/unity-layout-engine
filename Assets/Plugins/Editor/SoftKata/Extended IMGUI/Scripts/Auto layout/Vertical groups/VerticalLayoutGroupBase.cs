using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginVerticalGroup(LayoutGroupBase group) {
            return BeginLayoutGroup(group);
        }
        public static bool BeginVerticalGroup(Constraints modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterForLayout(new VerticalGroup(modifier, style));
            }

            return RetrieveNextGroup().IsGroupValid;
        }        
        public static bool BeginVerticalGroup(Constraints modifier = Constraints.None) {
            return BeginVerticalGroup(modifier, ExtendedEditorGUI.LayoutResources.VerticalGroup);
        }
        
        public static void EndVerticalGroup() {
            EndLayoutGroup<VerticalGroup>();
        }

        public class VerticalGroup : LayoutGroupBase {
            public VerticalGroup(Constraints modifier, GUIStyle style) : base(modifier, style) {
                SpaceBetweenEntries = style.contentOffset.y;
            }

            protected override void ModifyContainerSize() {
                // vertical "service" height addition: margin/border/padding + space between entries
                RequestedHeight += ConstraintsHeight + SpaceBetweenEntries * (EntriesCount - 1);
            }
            
            protected sealed override bool PrepareNextRect(float width, float height) {
                if (IsLayout) {
                    EntriesCount++;
                    RequestedWidth = Mathf.Max(RequestedWidth, width);
                    RequestedHeight += height;

                    return false;
                }

                if (!IsGroupValid) return false;

                NextEntryPosition.y += height + SpaceBetweenEntries;

                // occlusion
                return CurrentEntryPosition.y + height >= VisibleAreaRect.y
                       && CurrentEntryPosition.y <= VisibleAreaRect.yMax;
            }

            public override void RegisterArray(float elemWidth, float elemHeight, int count) {
                EntriesCount += count;
                RequestedWidth = Mathf.Max(RequestedWidth, elemWidth);
                RequestedHeight += elemHeight * count;
            }
        }

        public class VerticalScope : IDisposable {
            public readonly bool Valid;

            public VerticalScope(Constraints modifier, GUIStyle style) {
                Valid = BeginVerticalGroup(modifier, style);
            }

            public VerticalScope(Constraints modifier = Constraints.None) {
                Valid = BeginVerticalGroup(modifier);
            }

            public void Dispose() {
                EndVerticalGroup();
            }
        }
    }
}