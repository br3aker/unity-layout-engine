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

            private LayoutGroup _group = new FlexibleHorizontalGroup(FlexibleHorizontalGroup.FullScreenWidth, LayoutGroup.LayoutResources.WindowHeaderGroup);

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
    }
}