using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public class VerticalClippingGroup : VerticalGroup {
            public VerticalClippingGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) { }

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
                        
                        ContainerRect = Padding.Remove(Border.Remove(Margin.Remove(ContainerRect)));
                        VisibleAreaRect = Utility.RectIntersection(VisibleAreaRect, ContainerRect);

                        NextEntryPosition += ContainerRect.position - VisibleAreaRect.position;

                        GUI.BeginClip(VisibleAreaRect);
                        VisibleAreaRect.position = Vector2.zero;

                        return;
                    }
                }

                ScrapGroups(ChildrenCount);
            }

            internal sealed override void EndGroup(EventType eventType) {
                GUI.EndClip();
                EndGroupModifiersRoutine(eventType);
            }
        }
    }
}