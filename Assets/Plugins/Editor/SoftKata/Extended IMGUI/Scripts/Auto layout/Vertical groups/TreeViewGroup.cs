using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class TreeViewGroup : VerticalGroup {
        private readonly Color _connectionLineColor;
        private readonly float _connectionLineLength;
        private readonly float _connectionLineWidth;

        private float _lastEntryHalfHeight;
        private float _lastEntryY;

        private float _entryPaddingFromConnector;

        // public TreeViewGroup(Constraints modifier, GUIStyle style) : base(modifier, style) {
        //     // var overflow = style.overflow;
        //     // _connectionLineWidth = overflow.left;
        //     // _connectionLineLength = overflow.left + overflow.right;

        //     // _connectionLineColor = style.normal.textColor;

        //     // _entryPaddingFromConnector = Padding.left;
        // }

        // protected override Rect GetEntryRect(float x, float y, float width, float height) {
        //     _lastEntryHalfHeight = height / 2;
        //     _lastEntryY = y;

        //     var horizontalLine = new Rect(
        //         x - _entryPaddingFromConnector, y + _lastEntryHalfHeight,
        //         _connectionLineLength, _connectionLineWidth
        //     );
        //     EditorGUI.DrawRect(horizontalLine, _connectionLineColor);

        //     return new Rect(x, y, width, height);
        // } 
    
        // internal override void EndNonLayout() {
        //     var verticalLineRect = new Rect(
        //         ContainerRect.x - _entryPaddingFromConnector, ContainerRect.y,
        //         _connectionLineWidth,
        //         _lastEntryY + _lastEntryHalfHeight - ContainerRect.y + Padding.top
        //     );

        //     EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
        // }
    }
}