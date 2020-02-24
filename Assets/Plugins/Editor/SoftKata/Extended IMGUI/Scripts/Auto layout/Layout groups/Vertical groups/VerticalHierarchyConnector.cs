using System;
using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class LayoutEngine {
        public static bool BeginVerticalHierarchyGroup(GroupModifier modifier, GUIStyle style) {
            if (Event.current.type == EventType.Layout)
                return RegisterForLayout(new VerticalHierarchyGroup(modifier, style));

            return RetrieveNextGroup().IsGroupValid;
        }
        public static bool BeginVerticalHierarchyGroup(GroupModifier modifier = GroupModifier.None) {
            return BeginVerticalHierarchyGroup(modifier,
                ExtendedEditorGUI.Resources.LayoutGroups.VerticalHierarchyGroup);
        }
        public static void EndVerticalHierarchyGroup() {
            EndLayoutGroup<VerticalHierarchyGroup>()
                .DrawMajorConnectionType();
        }

        private class VerticalHierarchyGroup : VerticalGroup {
            private readonly Color _connectionLineColor;
            private readonly float _connectionLineLength;
            private readonly float _connectionLineWidth;

            private float _lastEntryHeight;
            private float _lastEntryY;

            public VerticalHierarchyGroup(GroupModifier modifier, GUIStyle style) : base(modifier, style) {
                _connectionLineWidth = Border.left;
                _connectionLineLength = Padding.left + Border.right;

                _connectionLineColor = style.normal.textColor;
            }

            protected override Rect GetEntryRect(float x, float y, float width, float height) {
                _lastEntryHeight = height;
                _lastEntryY = y;

                var horizontalLine = new Rect(
                    ContainerRect.x - Padding.left, y + height / 2,
                    _connectionLineLength, _connectionLineWidth
                );
                EditorGUI.DrawRect(horizontalLine, _connectionLineColor);

                return new Rect(x, y, width, height);
            }

            internal void DrawMajorConnectionType() {
                var verticalLineRect = new Rect(
                    ContainerRect.x - Padding.left, ContainerRect.y - Padding.top,
                    _connectionLineWidth,
                    _lastEntryY - ContainerRect.y + _lastEntryHeight
                );

                EditorGUI.DrawRect(verticalLineRect, _connectionLineColor);
            }
        }

        public class VerticalHierarchyScope : IDisposable {
            public readonly bool Valid;

            public VerticalHierarchyScope(GroupModifier modifier, GUIStyle style) {
                Valid = BeginVerticalHierarchyGroup(modifier, style);
            }

            public VerticalHierarchyScope(GroupModifier modifier = GroupModifier.None) {
                Valid = BeginVerticalHierarchyGroup(modifier);
            }

            public void Dispose() {
                EndVerticalHierarchyGroup();
            }
        }
    }
}