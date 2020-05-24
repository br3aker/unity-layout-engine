using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Assertions;

using SoftKata.Editor.Animations;

using Debug = UnityEngine.Debug;


namespace SoftKata.Editor.Controls {
    public interface IDrawableElement {
        void OnGUI();
    }
    public interface IAbsoluteDrawableElement {
        void OnGUI(Rect position);
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

        public void OnGUI() {
            if(Layout.GetRect(_width, _height, out var rect) && GUI.Button(rect, _content, _style)) {
                _action();
            }
        }
    }
}
