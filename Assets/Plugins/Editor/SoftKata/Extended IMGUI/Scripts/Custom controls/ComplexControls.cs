using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Assertions;

using SoftKata.ExtendedEditorGUI.Animations;

using Debug = UnityEngine.Debug;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        public interface IDrawableElement {
            void OnGUI();
        }
        public interface IAbsoluteDrawableElement {
            void OnGUI(Rect position);
        }

        public class DelegateElement : IDrawableElement {
            private Action _drawer;

            public DelegateElement(Action drawer) {
                _drawer = drawer;
            }

            public void OnGUI() {
                _drawer();
            }
        }

        public class Button : IDrawableElement {
            private GUIContent _content;
            private GUIStyle _style;

            private Action _action;

            private float _height;
            private float _width;

            public Button(GUIContent content, GUIStyle style, Action action) {
                _content = content;
                _style = style;

                _action = action;

                var size = style.CalcSize(content);
                _height = size.y;
                _width = size.x;
            } 
            public Button(Texture icon, GUIStyle style, Action action) : this(new GUIContent(icon), style, action) {}

            public void OnGUI() {
                if(Layout.GetRect(_width, _height, out var rect) && GUI.Button(rect, _content, _style)) {
                    _action();
                }
            }
        }

        public class WindowHeaderBar {
            public const float HeaderHeight = 20;

            private FlexibleHorizontalGroup _root = new FlexibleHorizontalGroup(FlexibleHorizontalGroup.FullScreenWidth, Resources.WindowHeader.GroupStyle);

            private Button _mainActionItem;
            private IDrawableElement[] _actionItems;

            public WindowHeaderBar(Button mainAction, params IDrawableElement[] actions) {
                _mainActionItem = mainAction;
                _actionItems = actions;
            }

            public void OnGUI() {
                _root.Width = Layout.CurrentContentWidth;
                if(Layout.BeginLayoutGroup(_root)) {
                    _mainActionItem?.OnGUI();
                    _root.GetRect(0);
                    for(int i = 0; i < _actionItems.Length; i++) {
                        _actionItems[i].OnGUI();
                    }
                    Layout.EndLayoutGroup();
                }
            }
        }

        public class WindowHeaderSearchBar : IDrawableElement {
            private enum State {
                Folded,
                Animating,
                Unfolded
            }

            private const string DefaultSearchBoxText = "Search...";

            private readonly GUIContent _searchButtonContent = EditorGUIUtility.IconContent("d_Search Icon");
            private readonly GUIContent _cancelButtonContent = EditorGUIUtility.IconContent("d_winbtn_win_close");

            private readonly GUIStyle _buttonStyle = Resources.WindowHeader.ButtonStyle;
            private readonly GUIStyle _searchBoxStyle = Resources.WindowHeader.SearchBoxStyle;

            private readonly TweenFloat _animator;

            private readonly float _buttonWidth;

            private string _currentSearchString = "";

            private State _state;

            public event Action<string> SeachQueryUpdated;

            public WindowHeaderSearchBar() {
                _animator = new TweenFloat() {
                    Speed = 6.5f
                };
                _animator.OnBegin += () => _state = State.Animating;
                _animator.OnFinish += () => {
                    _state = Mathf.Approximately(_animator.Value, 1) ? State.Unfolded : State.Folded;
                };

                _buttonWidth = _buttonStyle.CalcSize(_cancelButtonContent).x;
            }

            public void OnGUI() {
                switch(_state) {
                    case State.Folded:
                        DoFolded();
                        break;
                    case State.Animating:
                        var animProgress = _animator.Value;
                        var searchBoxWidth = _buttonWidth + (EditorGUIUtility.currentViewWidth / 2 - 1) * _animator.Value;
                        DoAnimations(searchBoxWidth, _buttonWidth * animProgress);
                        break;
                    case State.Unfolded:
                        DoUnfolded(_buttonWidth + EditorGUIUtility.currentViewWidth / 2 - 1, _buttonWidth);
                        break;
                }
                return;
            }
        
            private void DoFolded() {
                var buttonRect = Layout.GetRect(_buttonWidth, WindowHeaderBar.HeaderHeight);
                if(GUI.Button(buttonRect, _searchButtonContent, _searchBoxStyle)) {
                    _animator.Target = 1;

                    var parentGroup = Layout._currentGroup;
                    _animator.OnUpdate += () => parentGroup.MarkLayoutDirty();
                }
                if(Event.current.type == EventType.Repaint) {
                    _buttonStyle.Draw(buttonRect, _searchButtonContent, false, false, false, false);
                }
            }
            private void DoAnimations(float searchBoxWidth, float closeButtonWidth) {
                var controlRect = Layout.GetRect(searchBoxWidth + closeButtonWidth, WindowHeaderBar.HeaderHeight);
                if(Event.current.type != EventType.Repaint) return;                

                // search box
                var searchBoxRect = controlRect;
                searchBoxRect.width = searchBoxWidth;
                _searchBoxStyle.Draw(
                    searchBoxRect,
                    _currentSearchString.Length == 0 ? DefaultSearchBoxText : _currentSearchString,
                    false, false, false, false
                );

                // search box icon
                var searchIconRect = new Rect(controlRect.position, new Vector2(_buttonWidth, WindowHeaderBar.HeaderHeight));
                _buttonStyle.Draw(searchIconRect, _searchButtonContent, false, false, false, false);

                // cancel button
                var cancelButtonRect = new Rect(searchBoxRect.xMax, controlRect.y, closeButtonWidth, controlRect.height);
                _buttonStyle.Draw(cancelButtonRect, _cancelButtonContent, false, false, false, false);
            }
            private void DoUnfolded(float searchBoxWidth, float closeButtonWidth) {
                var controlRect = Layout.GetRect(searchBoxWidth + closeButtonWidth, WindowHeaderBar.HeaderHeight);

                // actual search box
                EditorGUI.BeginChangeCheck();
                var searchBoxRect = controlRect;
                searchBoxRect.width = searchBoxWidth;
                string newSearchString = EditorGUI.DelayedTextField(searchBoxRect, _currentSearchString.Length == 0 ? DefaultSearchBoxText : _currentSearchString, _searchBoxStyle);
                if(EditorGUI.EndChangeCheck() && newSearchString != DefaultSearchBoxText) {
                    _currentSearchString = newSearchString;
                    SeachQueryUpdated?.Invoke(newSearchString);
                }

                // search box icon
                if(Event.current.type == EventType.Repaint) {
                    var searchIconRect = new Rect(controlRect.position, new Vector2(_buttonWidth, WindowHeaderBar.HeaderHeight));
                    _buttonStyle.Draw(searchIconRect, _searchButtonContent, false, false, false, false);
                }

                // cancel button
                var cancelButtonRect = new Rect(searchBoxRect.xMax, controlRect.y, closeButtonWidth, controlRect.height);
                if(GUI.Button(cancelButtonRect, _cancelButtonContent, _buttonStyle)) {
                    _animator.Target = 0;

                    var parentGroup = Layout._currentGroup;
                    _animator.OnUpdate += () => parentGroup.MarkLayoutDirty();
                }
            }
        }
    }
}
