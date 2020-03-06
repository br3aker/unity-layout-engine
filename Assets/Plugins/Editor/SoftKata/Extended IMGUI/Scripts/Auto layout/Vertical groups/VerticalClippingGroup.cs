using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public class VerticalClippingGroup : VerticalGroup {
            private Vector2 _globalSpacePosition;

            protected readonly RectOffset ClipSpacePadding;

            public VerticalClippingGroup(Constraints modifier, GUIStyle style) : base(modifier, style) { 
                ClipSpacePadding = new RectOffset();
            }

            internal override void RetrieveLayoutData() {
                if (IsGroupValid) {
                    if (_parent != null) {
                        var rectData = _parent.GetGroupRectData(RequestedWidth, RequestedHeight);
                        VisibleAreaRect = rectData.VisibleRect;
                        ContainerRect = rectData.FullContentRect;
                    }
                    else {
                        ContainerRect = GetRectFromRoot(RequestedHeight, RequestedWidth);
                        VisibleAreaRect = ContainerRect;
                    }

                    IsGroupValid = VisibleAreaRect.IsValid() && Event.current.type != EventType.Used;
                    if (IsGroupValid) {
                        IsLayout = false;
                        _automaticWidth = _visibleContentWidth - ClipSpacePadding.horizontal;

                        ContainerRect = Padding.Remove(Border.Remove(Margin.Remove(ContainerRect)));
                        VisibleAreaRect = ClipSpacePadding.Remove(Utility.RectIntersection(VisibleAreaRect, ContainerRect));

                        NextEntryPosition += ContainerRect.position - VisibleAreaRect.position;

                        GUI.BeginClip(VisibleAreaRect);
                        _globalSpacePosition = VisibleAreaRect.position;
                        VisibleAreaRect = new Rect(Vector2.zero, VisibleAreaRect.size);

                        return;
                    }
                }

                ScrapGroups(ChildrenCount);
            }

            internal sealed override void EndGroup(EventType eventType) {
                GUI.EndClip();
                EndGroupModifiersRoutine(eventType);
            }

            public override Rect GetContentRect(Constraints contraints = Constraints.DiscardMargin) {
                var output = base.GetContentRect();
                output.position -= _globalSpacePosition;

                return output;
            }
        }
    }
}