using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Assertions;

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

            private LayoutGroup _group = new FlexibleHorizontalGroup(FlexibleHorizontalGroup.FullScreenWidth, Resources.WindowHeader.GroupStyle);

            private Button _mainActionItem;
            private IDrawableElement[] _actionItems;

            public WindowHeaderBar(Button mainAction, params IDrawableElement[] actions) {
                _mainActionItem = mainAction;
                _actionItems = actions;
            }

            public void OnGUI() {
                if(Layout.BeginLayoutGroup(_group)) {
                    _mainActionItem?.OnGUI();
                    _group.GetRect(0);
                    for(int i = 0; i < _actionItems.Length; i++) {
                        _actionItems[i].OnGUI();
                    }
                    Layout.EndLayoutGroup();
                }
            }
        }

        // TODO: OnGUI method needs some love & refactoring
        public class WindowHeaderSearchBar : IDrawableElement {
            private readonly GUIContent _searchButtonContent = EditorGUIUtility.IconContent("d_Search Icon");
            private readonly GUIContent _cancelButtonContent = EditorGUIUtility.IconContent("d_winbtn_win_close");

            private readonly GUIStyle _buttonStyle = Resources.WindowHeader.ButtonStyle;
            private readonly GUIStyle _searchBoxStyle = Resources.WindowHeader.SearchBoxStyle;

            private readonly AnimFloat _animator = new AnimFloat(0, CurrentViewRepaint);

            private readonly float _buttonWidth;

            private const string DefaultSearchBoxText = "Search...";
            private string _currentSearchString = "";

            public event Action<string> SeachQueryUpdated;

            public WindowHeaderSearchBar() {
                _animator.speed = 3.5f;

                _buttonWidth = _buttonStyle.CalcSize(_cancelButtonContent).x;
            }

            public void OnGUI() {
                if(Mathf.Approximately(_animator.value, 0)) {
                    var buttonRect = Layout.GetRect(_buttonWidth, WindowHeaderBar.HeaderHeight);
                    if(GUI.Button(buttonRect, _searchButtonContent, _searchBoxStyle)) {
                        _animator.target = 1;
                    }
                    if(Event.current.type == EventType.Repaint) {
                        _buttonStyle.Draw(buttonRect, _searchButtonContent, false, false, false, false);
                    }
                }
                else {
                    var searchBoxWidth = _buttonWidth + EditorGUIUtility.currentViewWidth / 2 * _animator.value;
                    var closeButtonWidth = _buttonWidth * _animator.value;

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
                        _animator.target = 0;
                    }
                }
            }
        }
    }
}
