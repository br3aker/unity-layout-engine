using System;
using UnityEngine;
using UnityEditor.AnimatedValues;

namespace SoftKata.ExtendedEditorGUI {
    public class VerticalFadeGroup : VerticalGroup {
        public AnimBool _expanded;
        public bool Expanded {
            get => _expanded.target;
            set => _expanded.target = value;
        }
        public bool Visible => !Mathf.Approximately(_expanded.faded, 0);

        public VerticalFadeGroup(bool expanded, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _expanded = new AnimBool(expanded, ExtendedEditorGUI.CurrentViewRepaint);
        }
        public VerticalFadeGroup(bool expanded = false, bool ignoreConstaints = false) 
            : this(expanded, StyleResources.VerticalFadeGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            base.PreLayoutRequest();

            Clip = _expanded.isAnimating;
            ContentRectInternal.height *= _expanded.faded;
        }
    }
}