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
        public class Tabs : IDrawableElement {
            // Logic data
            public int CurrentTab { get; set; }

            // GUI content & drawers
            private readonly GUIContent[] _tabHeaders;
            private readonly IDrawableElement[] _contentDrawers;

            // Animators
            private readonly AnimFloat _animator;

            // Styling
            private readonly GUIStyle _tabHeaderStyle;
            private float _tabHeaderHeight;

            private readonly Color _underlineColor;
            private float _underlineHeight;

            // Layout groups
            private readonly ScrollGroup _scrollGroup;
            private readonly LayoutGroup _horizontalGroup;

            public Tabs(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor, GUIStyle tabHeaderStyle) {
                // Data
                CurrentTab = initialTab;

                // GUI content & drawers
                _tabHeaders = tabHeaders;
                _contentDrawers = contentDrawers;

                // Animators
                _animator = new AnimFloat(initialTab, ExtendedEditorGUI.CurrentViewRepaint);

                // Styling
                _tabHeaderStyle = tabHeaderStyle;
                _tabHeaderHeight = tabHeaderStyle.GetContentHeight(tabHeaders[0]);

                _underlineColor = underlineColor;
                _underlineHeight = tabHeaderStyle.margin.bottom;

                // Layout groups
                _scrollGroup = new ScrollGroup(new Vector2(-1, float.MaxValue), new Vector2(initialTab / (_tabHeaders.Length - 1), 0f), true, true);
                _horizontalGroup = new HorizontalGroup(true);
            }
            public Tabs(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor)
                : this(initialTab, tabHeaders, contentDrawers, underlineColor, ElementsResources.TabHeader) { }

            public void OnGUI() {
                int currentSelection = CurrentTab;
                float currentAnimationPosition = _animator.value / (_tabHeaders.Length - 1);

                // Tabs
                if (Layout.GetRect(_tabHeaderHeight, out var toolbarRect)) {
                    // Tab control
                    currentSelection = GUI.Toolbar(toolbarRect, currentSelection, _tabHeaders, _tabHeaderStyle);

                    // Underline
                    var singleTabWidth = toolbarRect.width / _tabHeaders.Length;
                    var maximumOriginOffset = singleTabWidth * (_tabHeaders.Length - 1);
                    var underlinePosX = toolbarRect.x + maximumOriginOffset * currentAnimationPosition;
                    var underlineRect = new Rect(underlinePosX, toolbarRect.yMax - _underlineHeight, singleTabWidth, _underlineHeight);
                    EditorGUI.DrawRect(underlineRect, _underlineColor);
                }

                // Content
                if (_animator.isAnimating) {
                    _scrollGroup.ScrollPosX = currentAnimationPosition;
                    if(Layout.BeginLayoutGroup(_scrollGroup)) {
                        if(Layout.BeginLayoutGroup(_horizontalGroup)) {
                            for (int i = 0; i < _tabHeaders.Length; i++) {
                                _contentDrawers[i].OnGUI();
                            }
                            Layout.EndLayoutGroup();
                        }
                        Layout.EndLayoutGroup();
                    }
                }
                else {
                    _contentDrawers[CurrentTab].OnGUI();
                }

                // Change check
                if (currentSelection != CurrentTab) {
                    CurrentTab = currentSelection;
                    _animator.target = currentSelection;
                }
            }
        }
    }
}