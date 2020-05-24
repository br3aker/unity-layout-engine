using System;
using UnityEngine;
using UnityEditor.AnimatedValues;
using SoftKata.ExtendedEditorGUI.Animations;

namespace SoftKata.ExtendedEditorGUI {
    public class VerticalFadeGroup : VerticalGroup {
        private TweenBool _expanded;

        public bool Expanded {
            get => _expanded.Target;
            set => _expanded.Target = value;
        }
        public bool Visible => _expanded;

        public VerticalFadeGroup(bool expanded, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _expanded = new TweenBool(expanded) {
                Speed = 4
            };
            _expanded.OnUpdate += () => {
                MarkLayoutDirty();
                ExtendedEditorGUI.CurrentViewRepaint();
            };
        }
        public VerticalFadeGroup(bool expanded = false, bool ignoreConstaints = false) 
            : this(expanded, ExtendedEditorGUI.Resources.VerticalFadeGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            base.PreLayoutRequest();

            Clip = _expanded.IsAnimating;
            EntriesRequestedSize.y *= _expanded.Fade;
        }
    }
}