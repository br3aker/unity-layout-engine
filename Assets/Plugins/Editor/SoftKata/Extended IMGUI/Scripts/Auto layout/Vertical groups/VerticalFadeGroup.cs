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

        public VerticalFadeGroup(bool expanded, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _expanded = new AnimBool(expanded, ExtendedEditorGUI.CurrentViewRepaint);
        }
        public VerticalFadeGroup(bool expanded = false, bool ignoreConstaints = false) 
            : this(expanded, ExtendedEditorGUI.LayoutResources.VerticalFadeGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            base.PreLayoutRequest();

            Clip = _expanded.isAnimating;
            ContentRect.height *= _expanded.faded;
        }
    }
}