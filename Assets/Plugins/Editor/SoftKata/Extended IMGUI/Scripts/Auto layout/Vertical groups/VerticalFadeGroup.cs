using UnityEngine;

using SoftKata.ExtendedEditorGUI.Animations;


namespace SoftKata.ExtendedEditorGUI {
    public class VerticalFadeGroup : VerticalGroup {
        private readonly TweenBool _expanded;
        public bool Expanded {
            get => _expanded.Target;
            set => _expanded.Target = value;
        }
        public bool Visible => _expanded;

        public VerticalFadeGroup(bool expanded, GUIStyle style, bool ignoreConstaints = false) : base(style, ignoreConstaints) {
            _expanded = new TweenBool(expanded) {
                Speed = 4
            };
            _expanded.OnUpdate += MarkLayoutDirty;
            _expanded.OnStart += ExtendedEditorGUI.CurrentView.RegisterRepaintRequest;
            _expanded.OnFinish += ExtendedEditorGUI.CurrentView.UnregisterRepaintRequest;
        }
        public VerticalFadeGroup(bool expanded = false, bool ignoreConstaints = false) 
            : this(expanded, ExtendedEditorGUI.Resources.VerticalFadeGroup, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            base.PreLayoutRequest();

            // Clip logic can be set in _expanded.OnStart and _expanded.OnFinish events
            // But it's an extra overhead and we have layout build event virtual method anyway
            Clip = _expanded.IsAnimating;
            EntriesRequestedSize.y *= _expanded.Fade;
        }
    }
}