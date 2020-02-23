using System;
using UnityEditor;
using UnityEngine;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        // TODO [optimization]: if faded equals to zero, this group behaves like a normal one
        internal class VerticalFadeGroup : VerticalClippingGroup {
            private float _faded;

            public VerticalFadeGroup(float faded, GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                _faded = faded;
            }

            protected override void PreLayoutRequest() {
                TotalRequestedHeight *= _faded;
            }
        }
        
        public class VerticalFadeGroupScope : IDisposable {
            public readonly bool Valid;
            
            public VerticalFadeGroupScope(float faded, GroupModifier modifier, GUIStyle style) {
                Valid = BeginVerticalFadeGroup(faded, modifier, style);
            }
            public VerticalFadeGroupScope(float faded, GroupModifier modifier = GroupModifier.None) {
                Valid = BeginVerticalFadeGroup(faded, modifier);
            }
            
            public void Dispose() {
                EndVerticalFadeGroup();
            }
        }

        public static bool BeginVerticalFadeGroup(float faded, GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout) {
                return RegisterGroup(new VerticalFadeGroup(faded, modifier, style));
            }

            return GatherGroup().IsGroupValid;
        }
        public static bool BeginVerticalFadeGroup(float faded, GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalFadeGroup(faded, modifier, ExtendedEditorGUI.Resources.LayoutGroups.VerticalFadeGroup);
        }

        public static void EndVerticalFadeGroup() {
            EndLayoutGroup<VerticalFadeGroup>();
        }
    }
}