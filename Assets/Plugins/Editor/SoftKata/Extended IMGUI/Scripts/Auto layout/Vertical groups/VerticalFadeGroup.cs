using System;
using UnityEngine;
using UnityEditor.AnimatedValues;

namespace SoftKata.ExtendedEditorGUI {
    public class VerticalFadeGroup : VerticalGroup {
        private AnimBool _expanded;
        public bool Expanded {
            get => _expanded.target;
            set {
                _expanded.target = value;
            }
        }

        public VerticalFadeGroup(bool expanded, Constraints modifier, GUIStyle style) : base(modifier, style) {
            _expanded = new AnimBool(expanded, ExtendedEditorGUI.CurrentViewRepaint);
        }
        public VerticalFadeGroup(bool expanded = false, Constraints modifier = Constraints.None) 
            : this(expanded, modifier, ExtendedEditorGUI.LayoutResources.VerticalFadeGroup) {}

        protected override void PreLayoutRequest() {
            base.PreLayoutRequest();

            Clip = _expanded.isAnimating;
            ContentRect.height *= _expanded.faded;
        }
    }
}