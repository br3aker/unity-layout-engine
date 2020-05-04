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
                Layout.BeginLayoutGroup(_group);
                _mainActionItem?.OnGUI();
                _group.GetRect(0);
                for(int i = 0; i < _actionItems.Length; i++) {
                    _actionItems[i].OnGUI();
                }
                Layout.EndLayoutGroup();
            }
        }

        public class WindowHeaderSearchBar : IDrawableElement {
            private GUIContent _searchButtonContent = EditorGUIUtility.IconContent("d_Search Icon");
            private GUIContent _cancelButtonContent = EditorGUIUtility.IconContent("d_winbtn_win_close");

            private GUIStyle _buttonStyle = Resources.WindowHeader.ButtonStyle;
            private GUIStyle _searchBoxStyle = Resources.WindowHeader.SearchBoxStyle;

            private AnimFloat _animator = new AnimFloat(0, CurrentViewRepaint);

            private float _buttonWidth;

            private const string DefaultSearchBoxText = "Search...";
            private string _currentSearchString = "";

            public event Action<string> SeachQueryUpdated;

            public WindowHeaderSearchBar() {
                _buttonWidth = _buttonStyle.CalcSize(_cancelButtonContent).x;
            }

            public void OnGUI() {
                if(Mathf.Approximately(_animator.value, 0)) {
                    if(GUI.Button(Layout.GetRect(_buttonWidth, WindowHeaderBar.HeaderHeight), _searchButtonContent, _buttonStyle)) {
                        _animator.target = 1;
                    }
                }
                else {
                    var inputFieldWidth = EditorGUIUtility.currentViewWidth / 2 * _animator.value;
                    var closeButtonWidth = _buttonWidth * _animator.value;

                    var startPos = Layout.GetRect(_buttonWidth + inputFieldWidth + closeButtonWidth, WindowHeaderBar.HeaderHeight).position;

                    // search box button
                    var currentRect = new Rect(startPos, new Vector2(_buttonWidth, WindowHeaderBar.HeaderHeight));
                    if(Event.current.type == EventType.Repaint) {
                        _buttonStyle.Draw(currentRect, _searchButtonContent, false, false, false, false);
                    }

                    // actual search box
                    currentRect.x = currentRect.xMax;
                    currentRect.width = inputFieldWidth;
                    EditorGUI.BeginChangeCheck();
                    string newSearchString = EditorGUI.DelayedTextField(currentRect, _currentSearchString.Length == 0 ? DefaultSearchBoxText : _currentSearchString, _searchBoxStyle);
                    if(EditorGUI.EndChangeCheck() && newSearchString != DefaultSearchBoxText) {
                        _currentSearchString = newSearchString;
                        SeachQueryUpdated?.Invoke(newSearchString);
                    }

                    // cancel button
                    currentRect.x = currentRect.xMax;
                    currentRect.width = closeButtonWidth;
                    if(GUI.Button(currentRect, _cancelButtonContent, _buttonStyle)) {
                        _animator.target = 0;
                    }
                }
            }
        }
    }
}