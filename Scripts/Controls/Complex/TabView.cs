using UnityEngine;
using UnityEditor;

using SoftKata.UnityEditor.Animations;


namespace SoftKata.UnityEditor.Controls {
    public class TabView : IDrawableElement {
        public int CurrentTab { get; set; }
        public float TransitionSpeed {
            get => _animator.Speed;
            set => _animator.Speed = value;
        }

        // Headers & content
        private readonly GUIContent[] _tabHeaders;
        private readonly IDrawableElement[] _drawers;

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

        public TabView(IDrawableElement[] drawers, int selectedTab = 0) {
            // Data
            CurrentTab = selectedTab;

            // GUI content & drawers
            _drawers = drawers;

            // Animators
            _animator = new TweenFloat(selectedTab) {
                Speed = 3.25f
            };

            var currentView = ExtendedEditor.CurrentView;
            _animator.OnStart += currentView.RegisterRepaintRequest;
            _animator.OnFinish += currentView.UnregisterRepaintRequest;
            _animator.OnFinish += _root.MarkLayoutDirty;
        }

        public TabView(GUIContent[] tabHeaders, IDrawableElement[] drawers, Color underlineColor, GUIStyle tabHeaderStyle, int selectedTab = 0) 
            : this(drawers, selectedTab)
        {
            // GUI content
            _tabHeaders = tabHeaders;

            // Styling
            _tabHeaderStyle = tabHeaderStyle;
            _tabHeaderHeight = tabHeaderStyle.GetContentHeight(tabHeaders[0]);

            _underlineColor = underlineColor;
            _underlineHeight = tabHeaderStyle.margin.bottom;

            // Layout groups
            _scrollGroup = new ScrollGroup(new Vector2(-1, float.MaxValue), true, new GUIStyle(), Resources.ScrollGroupThumb, true) {
                HorizontalScroll = selectedTab / (tabHeaders.Length - 1)
            };
        }
        public TabView(GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor, int initialTab = 0)
            : this(tabHeaders, contentDrawers, underlineColor, Resources.TabHeader, initialTab) { }

        public void OnGUI() {
            if(Layout.BeginLayoutScope(_root)) {
                int currentSelection = CurrentTab;
                float currentAnimationPosition = _animator.Value / (_drawers.Length - 1);

                // Tabs
                if (_tabHeaders != null && Layout.GetRect(_tabHeaderHeight, out var toolbarRect)) {
                    currentSelection = GUI.Toolbar(toolbarRect, currentSelection, _tabHeaders, _tabHeaderStyle);

                    // Underline
                    var singleTabWidth = toolbarRect.width / _tabHeaders.Length;
                    var maximumOriginOffset = singleTabWidth * (_tabHeaders.Length - 1);
                    var underlinePosX = toolbarRect.x + maximumOriginOffset * currentAnimationPosition;
                    var underlineRect = new Rect(underlinePosX, toolbarRect.yMax - _underlineHeight, singleTabWidth, _underlineHeight);
                    EditorGUI.DrawRect(underlineRect, _underlineColor);
                }

                // Content
                if (_animator.IsAnimating) {
                    _scrollGroup.HorizontalScroll = currentAnimationPosition;
                    if(Layout.BeginLayoutScope(_scrollGroup)) {
                        if(Layout.BeginLayoutScope(_horizontalGroup)) {
                            for (int i = 0; i < _tabHeaders.Length; i++) {
                                _drawers[i].OnGUI();
                            }
                            Layout.EndCurrentScope();
                        }
                        Layout.EndCurrentScope();
                    }
                }
                else {
                    _drawers[CurrentTab].OnGUI();
                }

                // Change check
                if (currentSelection != CurrentTab) {
                    CurrentTab = currentSelection;
                    _animator.Target = currentSelection;
                    _root.MarkLayoutDirty();
                }

                Layout.EndCurrentScope();
            }
        }
    }
}