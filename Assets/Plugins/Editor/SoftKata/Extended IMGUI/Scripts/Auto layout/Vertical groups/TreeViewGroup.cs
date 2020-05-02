using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class TreeViewGroup : VerticalGroup {
        private readonly Color _connectionLineColor;
        private readonly float _connectionLineWidth;
        private readonly float _connectorContentOffset;
        private readonly float _connectionLineOffset;

        private bool _notRepaintEvent;

        private float _lastConnectionPointY;
        private float _connectionsPositionStart;
        private float _connectionsPositionEnd;
        

        public TreeViewGroup(GUIStyle style) : base(style, false) {
            _connectionLineWidth = style.border.left;
            _connectorContentOffset = style.contentOffset.x;

            _connectionLineColor = style.normal.textColor;

            _connectionLineOffset = style.padding.left + _connectionLineWidth;
        }
        public TreeViewGroup() : this(ExtendedEditorGUI.Resources.Treeview) {}
    
        internal override bool BeginNonLayout() {
            if(base.BeginNonLayout()) {
                _notRepaintEvent = Event.current.type != EventType.Repaint;
                _connectionsPositionStart = ContentRectInternal.x - _connectionLineOffset;
                _connectionsPositionEnd = _connectionsPositionStart + _connectorContentOffset;
                return true;
            }
            return false;
        }
        internal override void EndNonLayout() {
            base.EndNonLayout();

            if(_notRepaintEvent) return;
            var verticalLineRect = new Rect(
                _connectionsPositionStart, 
                ContentRectInternal.y,
                _connectionLineWidth,
                _lastConnectionPointY - ContentRectInternal.y
            );

            EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
        }

        private void DrawConnectionLine(Vector2 position, float height) {
            if(_notRepaintEvent) return;
            _lastConnectionPointY = position.y + height / 2;

            var horizontalLine = new Rect(
                _connectionsPositionStart, 
                _lastConnectionPointY,
                position.x - _connectionsPositionEnd,
                _connectionLineWidth
            );
            EditorGUI.DrawRect(horizontalLine, _connectionLineColor);
        }

        public bool GetLeafRect(float width, float height, out Rect rect) {
            DrawConnectionLine(NextEntryPosition, height);
            return GetRect(width, height, out rect);
        }
        public bool GetLeafRect(float height, out Rect rect) {
            return GetLeafRect(AutomaticWidth, height, out rect);
        }
        public Rect GetLeafRect(float width, float height) {
            GetLeafRect(width, height, out var rect);
            return rect;
        }
        public Rect GetLeafRect(float height) {
            return GetLeafRect(AutomaticWidth, height);
        }

        public void DrawConnection(Rect rect) {
            DrawConnectionLine(rect.position, rect.height);
        }
    }
}