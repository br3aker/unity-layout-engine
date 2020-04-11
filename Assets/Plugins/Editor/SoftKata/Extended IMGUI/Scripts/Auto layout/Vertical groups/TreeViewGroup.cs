using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    // public class TreeViewGroup : VerticalGroup {
    //     private readonly Color _connectionLineColor;
    //     private readonly float _connectionLineLength;
    //     private readonly float _connectionLineWidth;

    //     private float _lastEntryHalfHeight;
    //     private float _lastEntryY;

    //     private float _entryPaddingFromConnector;
    //     private float _rootBottomPadding;

    //     public TreeViewGroup(GUIStyle style) : base(style, false) {
    //         var overflow = style.overflow;
    //         _connectionLineWidth = overflow.left;
    //         _connectionLineLength = overflow.left + overflow.right;

    //         _connectionLineColor = style.normal.textColor;

    //         _entryPaddingFromConnector = style.padding.left;
    //         _rootBottomPadding = style.padding.top;
    //     }
    //     public TreeViewGroup() : this(ExtendedEditorGUI.LayoutResources.Treeview) {}
    
    //     internal override void EndNonLayout() {
    //         var verticalLineRect = new Rect(
    //             ContainerRect.x - _entryPaddingFromConnector, ContainerRect.y,
    //             _connectionLineWidth,
    //             _lastEntryY + _lastEntryHalfHeight - ContainerRect.y + _rootBottomPadding
    //         );

    //         EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
    //     }

    //     protected override bool QueryEntry(float width, float height, out Rect rect) {
    //         var position = NextEntryPosition;
    //         rect = new Rect(position, new Vector2(width, height));

    //         _lastEntryHalfHeight = height / 2;
    //         _lastEntryY = position.y;

    //         var horizontalLine = new Rect(
    //             position.x - _entryPaddingFromConnector, position.y + _lastEntryHalfHeight,
    //             _connectionLineLength, _connectionLineWidth
    //         );
    //         EditorGUI.DrawRect(horizontalLine, _connectionLineColor);

    //         return QueryAndOcclude(rect.size);
    //     }
    // }
}