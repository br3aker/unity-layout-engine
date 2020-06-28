using UnityEngine;

using SoftKata.UnityEditor.Animations;


namespace SoftKata.UnityEditor {
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
            _expanded.OnStart += ExtendedEditor.CurrentView.RegisterRepaintRequest;
            _expanded.OnFinish += ExtendedEditor.CurrentView.UnregisterRepaintRequest;
        }
        public VerticalFadeGroup(bool expanded = false, bool ignoreConstaints = false) 
            : this(expanded, ExtendedEditor.Resources.DefaultVerticalStyle, ignoreConstaints) {}

        protected override void PreLayoutRequest() {
            base.PreLayoutRequest();

            // Clip logic can be set in _expanded.OnStart and _expanded.OnFinish events
            // But it's an extra overhead and we have layout build event virtual method anyway
            Clip = _expanded.IsAnimating;
            RequestedSize.y *= _expanded.Fade;
        }
    }
}