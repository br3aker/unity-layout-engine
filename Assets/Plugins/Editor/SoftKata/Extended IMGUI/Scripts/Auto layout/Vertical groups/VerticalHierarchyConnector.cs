using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginTreeViewGroup(LayoutGroupBase group) {
            return BeginLayoutGroup(group);
        }
        public static bool BeginTreeViewGroup(Constraints modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout)
                return RegisterForLayout(new TreeViewGroup(modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginTreeViewGroup(Constraints modifier = Constraints.None) {
            return BeginTreeViewGroup(modifier, ExtendedEditorGUI.LayoutResources.Treeview);
        }
        
        public static void EndTreeView() {
            var group = EndLayoutGroup<TreeViewGroup>();
            // TODO: move this to BeginTreeViewGroup(...) for consistency
            group.DrawMajorConnectionType();
        }

        private class TreeViewGroup : VerticalGroup {
            private readonly Color _connectionLineColor;
            private readonly float _connectionLineLength;
            private readonly float _connectionLineWidth;

            private float _lastEntryHalfHeight;
            private float _lastEntryY;

            private float _entryPaddingFromConnector;

            public TreeViewGroup(Constraints modifier, GUIStyle style) : base(modifier, style) {
                var overflow = style.overflow;
                _connectionLineWidth = overflow.left;
                _connectionLineLength = overflow.left + overflow.right;

                _connectionLineColor = style.normal.textColor;

                _entryPaddingFromConnector = Padding.left;
            }

            protected override Rect GetEntryRect(float x, float y, float width, float height) {
                _lastEntryHalfHeight = height / 2;
                _lastEntryY = y;

                var horizontalLine = new Rect(
                    x - _entryPaddingFromConnector, y + _lastEntryHalfHeight,
                    _connectionLineLength, _connectionLineWidth
                );
                EditorGUI.DrawRect(horizontalLine, _connectionLineColor);

                return new Rect(x, y, width, height);
            } 

            internal void DrawMajorConnectionType() {
                if (!IsGroupValid) return;
                
                var verticalLineRect = new Rect(
                    ContainerRect.x - _entryPaddingFromConnector, ContainerRect.y,
                    _connectionLineWidth,
                    _lastEntryY + _lastEntryHalfHeight - ContainerRect.y + Padding.top
                );

                EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
            }
        }

        public class TreeViewScope : IDisposable {
            public readonly bool Valid;

            public TreeViewScope(Constraints modifier, GUIStyle style) {
                Valid = BeginTreeViewGroup(modifier, style);
            }

            public TreeViewScope(Constraints modifier = Constraints.None) {
                Valid = BeginTreeViewGroup(modifier);
            }

            public void Dispose() {
                EndTreeView();
            }
        }
    }
}