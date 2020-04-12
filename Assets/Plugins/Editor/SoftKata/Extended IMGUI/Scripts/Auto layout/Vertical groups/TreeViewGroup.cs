using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class TreeViewGroup : VerticalGroup {
        private readonly Color _connectionLineColor;
        private readonly float _connectionLineWidth;

        private float _lastConnectionPoint;

        private float _leftPadding;
        private float _connectorOrigin;
        private float _connectorOriginWithOffset;
        private float _connectorContentOffset;

        private bool _notRepaint;
        

        public TreeViewGroup(GUIStyle style) : base(style, false) {
            var overflow = style.overflow;
            _connectionLineWidth = overflow.left;
            _connectorContentOffset = overflow.right;

            _connectionLineColor = style.normal.textColor;

            _leftPadding = style.padding.left;
        }
        public TreeViewGroup() : this(ExtendedEditorGUI.LayoutResources.Treeview) {}
    
        internal override void BeginNonLayout() {
            base.BeginNonLayout();

            _connectorOrigin = ContentRect.x - _leftPadding;
            _connectorOriginWithOffset = _connectorOrigin + _connectorContentOffset;

            _notRepaint = Event.current.type != EventType.Repaint;
        }

        internal override void EndNonLayout() {
            base.EndNonLayout();

            if(_notRepaint) return;
            var verticalLineRect = new Rect(
                _connectorOrigin, 
                ContentRect.y,
                _connectionLineWidth,
                _lastConnectionPoint - ContentRect.y
            );

            EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
        }

        private void DrawConnectionLine(Vector2 position, float height) {
            if(_notRepaint) return;
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