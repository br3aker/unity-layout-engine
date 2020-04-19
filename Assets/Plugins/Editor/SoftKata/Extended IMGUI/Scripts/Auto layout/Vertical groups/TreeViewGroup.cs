using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class TreeViewGroup : VerticalGroup {
        private readonly Color _connectionLineColor;
        private readonly float _connectionLineWidth;
        private readonly float _connectorContentOffset;
        private readonly float _leftPadding;

        private bool _notRepaintEvent;

        private float _lastConnectionPoint;
        private float _connectorOrigin;
        private float _connectorOriginWithOffset;
        

        public TreeViewGroup(GUIStyle style) : base(style, false) {
            var overflow = style.overflow;
            _connectionLineWidth = overflow.left;
            _connectorContentOffset = overflow.right;

            _connectionLineColor = style.normal.textColor;

            _leftPadding = style.padding.left;
        }
        public TreeViewGroup() : this(StyleResources.Treeview) {}
    
        internal override bool BeginNonLayout() {
            if(base.BeginNonLayout()) {
                _notRepaintEvent = Event.current.type != EventType.Repaint;
                _connectorOrigin = ContentRectInternal.x - _leftPadding;
                _connectorOriginWithOffset = _connectorOrigin + _connectorContentOffset;
                return true;
            }
            return false;
        }
        internal override void EndNonLayout() {
            base.EndNonLayout();

            if(_notRepaintEvent) return;
            var verticalLineRect = new Rect(
                _connectorOrigin, 
                ContentRectInternal.y,
                _connectionLineWidth,
                _lastConnectionPoint - ContentRectInternal.y
            );

            EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
        }

        private void DrawConnectionLine(Vector2 position, float height) {
            if(_notRepaintEvent) return;
            _lastConnectionPoint = position.y + height / 2;

            var horizontalLine = new Rect(
                _connectorOrigin, 
                _lastConnectionPoint,
                position.x - _connectorOriginWithOffset,
                _connectionLineWidth
            );
            EditorGUI.DrawRect(horizontalLine, _connectionLineColor);
        }

        public bool GetLeafRect(float height, float width, out Rect rect) {
            DrawConnectionLine(NextEntryPosition, height);
            return GetRect(height, width, out rect);
        }
        public Rect GetLeafRect(float height, float width) {
            GetLeafRect(height, width, out var rect);
            return rect;
        }

        public void DrawConnection(Rect rect) {
            DrawConnectionLine(rect.position, rect.height);
        }
    }
}