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
        public class TabView : IDrawableElement {
            public int CurrentTab { get; set; }

            // Headers & content
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

            public TabView(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor, GUIStyle tabHeaderStyle) {
                // Data
                CurrentTab = initialTab;

                // GUI content & drawers
                _tabHeaders = tabHeaders;
                _contentDrawers = contentDrawers;

                // Styling
                _tabHeaderStyle = tabHeaderStyle;
                _tabHeaderHeight = tabHeaderStyle.GetContentHeight(tabHeaders[0]);

                _underlineColor = underlineColor;
                _underlineHeight = tabHeaderStyle.margin.bottom;

                // Layout groups
                _scrollGroup = new ScrollGroup(new Vector2(-1, float.MaxValue), true, true) {
                    HorizontalScroll = initialTab / (tabHeaders.Length - 1)
                };
                _horizontalGroup = new HorizontalGroup(true);

                // Animators
                _animator = new AnimFloat(initialTab);
                _animator.valueChanged.AddListener(ExtendedEditorGUI.CurrentViewRepaint);
                _animator.valueChanged.AddListener(_scrollGroup.MarkLayoutDirty);
            }
            public TabView(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor)
                : this(initialTab, tabHeaders, contentDrawers, underlineColor, Resources.TabHeader) { }

            // TODO: Retained mode throws exception due to invalid layout dirtying
            // This can be fixed by wrapping entire code in 'master' vertical layout group without any margins/paddings/borders
            // Then animator value can mark this group dirty and rebult layout
            // Furthermore, this group can draw background image
            public void OnGUI() {
                int currentSelection = CurrentTab;
                float currentAnimationPosition = _animator.value / (_tabHeaders.Length - 1);

                // Tabs
                if (Layout.GetRect(_tabHeaderHeight, out var toolbarRect)) {
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
                    _scrollGroup.HorizontalScroll = currentAnimationPosition;
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