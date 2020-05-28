using UnityEngine;

using SoftKata.UnityEditor.Animations;


namespace SoftKata.UnityEditor.Controls {
    public class TabView : IDrawableElement {
        public int CurrentTab { get; set; }

        // Headers & content
        private readonly GUIContent[] _tabHeaders;
        private readonly IDrawableElement[] _contentDrawers;

        // Animators
        private readonly TweenFloat _animator;

        // Styling
        private readonly GUIStyle _tabHeaderStyle;
        private float _tabHeaderHeight;

        private readonly Color _underlineColor;
        private float _underlineHeight;

        // Layout groups
        private readonly LayoutGroup _root = new VerticalGroup();
        private readonly LayoutGroup _horizontalGroup = new HorizontalGroup(true);
        private readonly ScrollGroup _scrollGroup;

        public TabView(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor, GUIStyle tabHeaderStyle) {
            // Data
            CurrentTab = initialTab;

            // GUI content & drawers
            _tabHeaders = tabHeaders;
            _contentDrawers = contentDrawers;

            // Styling
            _tabHeaderStyle = tabHeaderStyle;
            _tabHeaderHeight = tabHeaderStyle.GetContentHeight(tabHeaders[0]);

            _underlineColor = underlineColor;
            _underlineHeight = tabHeaderStyle.margin.bottom;

            // Layout groups
            _scrollGroup = new ScrollGroup(new Vector2(-1, float.MaxValue), true, new GUIStyle(), ExtendedEditor.Resources.ScrollGroupThumb, true) {
                HorizontalScroll = initialTab / (tabHeaders.Length - 1)
            };

            // Animators
            _animator = new TweenFloat(initialTab) {
                Speed = 3.25f
            };

            _animator.OnStart += ExtendedEditor.CurrentView.RegisterRepaintRequest;
            _animator.OnFinish += ExtendedEditor.CurrentView.UnregisterRepaintRequest;
            _animator.OnFinish += _root.MarkLayoutDirty;
        }
        public TabView(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor)
            : this(initialTab, tabHeaders, contentDrawers, underlineColor, ExtendedEditor.Resources.TabHeader) { }

        public void OnGUI() {
            if(Layout.BeginLayoutGroup(_root)) {
                int currentSelection = CurrentTab;
                float currentAnimationPosition = _animator.Value / (_tabHeaders.Length - 1);

                // Tabs
                if (Layout.GetRect(_tabHeaderHeight, out var toolbarRect)) {
                    currentSelection = GUI.Toolbar(toolbarRect, currentSelection, _tabHeaders, _tabHeaderStyle);

                    // Underline
                    var singleTabWidth = toolbarRect.width / _tabHeaders.Length;
                    var maximumOriginOffset = singleTabWidth * (_tabHeaders.Length - 1);
                    var underlinePosX = toolbarRect.x + maximumOriginOffset * currentAnimationPosition;
                    var underlineRect = new Rect(underlinePosX, toolbarRect.yMax - _underlineHeight, singleTabWidth, _underlineHeight);
                    global::UnityEditor.EditorGUI.DrawRect(underlineRect, _underlineColor);
                }

                // Content
                if (_animator.IsAnimating) {
                    _scrollGroup.HorizontalScroll = currentAnimationPosition;
                    if(Layout.BeginLayoutGroup(_scrollGroup)) {
                        if(Layout.BeginLayoutGroup(_horizontalGroup)) {
                            for (int i = 0; i < _tabHeaders.Length; i++) {
                                _contentDrawers[i].OnGUI();
                            }
                            Layout.EndLayoutGroup();
                        }
                        Layout.EndLayoutGroup();
                    }
                }
                else {
                    _contentDrawers[CurrentTab].OnGUI();
                }

                // Change check
                if (currentSelection != CurrentTab) {
                    CurrentTab = currentSelection;
                    _animator.Target = currentSelection;
                    _root.MarkLayoutDirty();
                }

                Layout.EndLayoutGroup();
            }
        }
    }
}