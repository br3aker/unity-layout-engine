using System;
using UnityEditor;
using UnityEngine;

using SoftKata.UnityEditor.Animations;


namespace SoftKata.UnityEditor.Controls {
    public class WindowHeaderBar {
        public const float HeaderHeight = 20;
        public const float HeaderContentHeight = HeaderHeight - 4;
        public const float WindowHeaderShadowHeight = 5;

        internal readonly FlexibleHorizontalGroup _root = new FlexibleHorizontalGroup(FlexibleHorizontalGroup.FullScreenWidth, Resources.WindowHeader.Group);

        public Button MainActionItem {get; set;}
        public IDrawableElement[] ActionItems {set; get;}

        public void OnGUI() {
            _root.Width = Layout.CurrentContentWidth;
            if(Layout.BeginLayoutScope(_root)) {
                MainActionItem?.OnGUI();
                _root.GetRect(0);
                for(int i = 0; i < ActionItems.Length; i++) {
                    ActionItems[i].OnGUI();
                }

                Layout.EndCurrentScope();
            }
        }
    
        public class SearchBar : IDrawableElement {
            private enum State {
                Folded,
                Animating,
                Expanded
            }

            private const string DefaultSearchString = "Search...";

            private readonly GUIContent _searchButtonContent = EditorGUIUtility.IconContent("d_Search Icon");
            private readonly GUIContent _cancelButtonContent = EditorGUIUtility.IconContent("d_winbtn_win_close");

            private readonly GUIStyle _buttonStyle;
            private readonly GUIStyle _searchBoxStyle;

            private readonly TweenBool _expanded;

            private readonly float _buttonWidth;

            private string _currentSearchString = DefaultSearchString;

            private State _state;

            public event Action<string> SeachQueryChanged;

            private IRepaintable _parentView;

            public SearchBar(WindowHeaderBar headerBar, Action<string> searchQueryChangedCallback) {
                _parentView = ExtendedEditor.CurrentView;

                var resources = Resources.WindowHeader;
                _buttonStyle = resources.ButtonStyle;
                _searchBoxStyle = resources.SearchBoxStyle;

                _expanded = new TweenBool() {
                    Speed = 6.5f
                };
                _expanded.OnUpdate += headerBar._root.MarkLayoutDirty;
                _expanded.OnStart += OnTransitionStart;
                _expanded.OnFinish += OnTransitionFinish;

                _buttonWidth = _buttonStyle.CalcSize(_cancelButtonContent).x;

                SeachQueryChanged += searchQueryChangedCallback;
            }

            public void OnGUI() {
                switch(_state) {
                    case State.Folded:
                        DoFolded();
                        break;
                    case State.Animating:
                        var animProgress = _expanded.Fade;
                        var searchBoxWidth = _buttonWidth + (EditorGUIUtility.currentViewWidth / 2 - 1) * animProgress;
                        DoAnimations(searchBoxWidth, _buttonWidth * animProgress);
                        break;
                    case State.Expanded:
                        DoUnfolded(_buttonWidth + EditorGUIUtility.currentViewWidth / 2 - 1, _buttonWidth);
                        break;
                }
                return;
            }
        
            private void DoFolded() {
                var buttonRect = Layout.GetRect(_buttonWidth, HeaderContentHeight);
                if(GUI.Button(buttonRect, _searchButtonContent, _searchBoxStyle)) {
                    _expanded.Target = true;
                }
                if(Event.current.type == EventType.Repaint) {
                    _buttonStyle.Draw(buttonRect, _searchButtonContent, false, false, false, false);
                }
            }
            private void DoAnimations(float searchBoxWidth, float closeButtonWidth) {
                var controlRect = Layout.GetRect(searchBoxWidth + closeButtonWidth, HeaderContentHeight);
                if(Event.current.type != EventType.Repaint) return;                

                // search box
                var searchBoxRect = controlRect;
                searchBoxRect.width = searchBoxWidth;
                _searchBoxStyle.Draw(
                    searchBoxRect,
                    _currentSearchString.Length == 0 ? DefaultSearchString : _currentSearchString,
                    false, false, false, false
                );

                // search box icon
                var searchIconRect = new Rect(controlRect.position, new Vector2(_buttonWidth, HeaderContentHeight));
                _buttonStyle.Draw(searchIconRect, _searchButtonContent, false, false, false, false);

                // cancel button
                var cancelButtonRect = new Rect(searchBoxRect.xMax, controlRect.y, closeButtonWidth, controlRect.height);
                _buttonStyle.Draw(cancelButtonRect, _cancelButtonContent, false, false, false, false);
            }
            private void DoUnfolded(float searchBoxWidth, float closeButtonWidth) {
                var controlRect = Layout.GetRect(searchBoxWidth + closeButtonWidth, HeaderContentHeight);

                // actual search box
                var searchBoxRect = controlRect;
                searchBoxRect.width = searchBoxWidth;

                string newSearchString = EditorGUI.DelayedTextField(searchBoxRect, _currentSearchString, _searchBoxStyle);
                if(_currentSearchString != newSearchString) {
                    if(newSearchString.Length != 0) {
                        _currentSearchString = newSearchString;
                        SeachQueryChanged?.Invoke(newSearchString);
                        return;
                    }
                    _currentSearchString = DefaultSearchString;
                }

                // search box icon
                if(Event.current.type == EventType.Repaint) {
                    var searchIconRect = new Rect(controlRect.position, new Vector2(_buttonWidth, HeaderContentHeight));
                    _buttonStyle.Draw(searchIconRect, _searchButtonContent, false, false, false, false);
                }

                // cancel button
                var cancelButtonRect = new Rect(searchBoxRect.xMax, controlRect.y, closeButtonWidth, controlRect.height);
                if(GUI.Button(cancelButtonRect, _cancelButtonContent, _buttonStyle)) {
                    _expanded.Target = false;
                }
            }
        
            // Tween callbacks
            private void OnTransitionStart() {
                _state = State.Animating;
                _parentView.RegisterRepaintRequest();
            }
            private void OnTransitionFinish() {
                _state = _expanded ? State.Expanded : State.Folded;
                _parentView.UnregisterRepaintRequest();
            }
        }
    }
}
