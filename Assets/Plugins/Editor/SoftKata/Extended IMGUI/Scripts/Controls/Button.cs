using System;
using UnityEngine;


namespace SoftKata.UnityEditor.Controls {
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
