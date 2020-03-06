using System;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginVerticalFadeGroup(LayoutGroupBase group){
            return BeginLayoutGroup(group);
        }
        public static bool BeginVerticalFadeGroup(float faded, Constraints modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout)
                return RegisterForLayout(new VerticalFadeGroup(faded, modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginVerticalFadeGroup(float faded, Constraints modifier = Constraints.None) {
            return BeginVerticalFadeGroup(faded, modifier, ExtendedEditorGUI.LayoutResources.VerticalFadeGroup);
        }
        
        public static void EndVerticalFadeGroup() {
            EndLayoutGroup<VerticalFadeGroup>();
        }

        public class VerticalFadeGroup : VerticalClippingGroup {
            public float Faded {get; set;}

            public VerticalFadeGroup(float faded, Constraints modifier, GUIStyle style) : base(modifier, style) {
                Faded = faded;
            }

            protected override void ModifyContainerSize() {
                base.ModifyContainerSize();

                RequestedHeight *= Faded;
            }
        }

        public class VerticalFadeScope : IDisposable {
            public readonly bool Valid;

            public VerticalFadeScope(float faded, Constraints modifier, GUIStyle style) {
                Valid = BeginVerticalFadeGroup(faded, modifier, style);
            }

            public VerticalFadeScope(float faded, Constraints modifier = Constraints.None) {
                Valid = BeginVerticalFadeGroup(faded, modifier);
            }

            public void Dispose() {
                EndVerticalFadeGroup();
            }
        }
    }
}