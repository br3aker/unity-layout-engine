using System;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginVerticalFadeGroup(float faded, GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout)
                return RegisterForLayout(new VerticalFadeGroup(faded, modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginVerticalFadeGroup(float faded, GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalFadeGroup(faded, modifier, ExtendedEditorGUI.Resources.LayoutGroups.VerticalFadeGroup);
        }
        public static void EndVerticalFadeGroup() {
            EndLayoutGroup<VerticalFadeGroup>();
        }

        internal class VerticalFadeGroup : VerticalClippingGroup {
            private readonly float _faded;

            public VerticalFadeGroup(float faded, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                _faded = faded;
            }

            protected override void PreLayoutRequest() {
                RequestedHeight *= _faded;
            }
        }

        public class VerticalFadeScope : IDisposable {
            public readonly bool Valid;

            public VerticalFadeScope(float faded, GroupModifier modifier, GUIStyle style) {
                Valid = BeginVerticalFadeGroup(faded, modifier, style);
            }

            public VerticalFadeScope(float faded, GroupModifier modifier = GroupModifier.None) {
                Valid = BeginVerticalFadeGroup(faded, modifier);
            }

            public void Dispose() {
                EndVerticalFadeGroup();
            }
        }
    }
}