using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        // Card element
        public static void StaticCard(GUIContent header, Action contentDrawer, ComplexControlsResources.CardElementData styleData) {
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.None, styleData.RootStyle)) {
                // Background
                EditorGUI.DrawRect(LayoutEngine.CurrentGroup.GetContentRect(), styleData.BackgroundColor);

                // Header
                if (LayoutEngine.GetRect(LabelHeight, -1, out var headerRect)) {
                    EditorGUI.LabelField(headerRect, header, styleData.HeaderStyle);
                }
            
                // Separator
                if (LayoutEngine.GetRect(1f, -1, out var separatorRect)) {
                    DrawSeparator(separatorRect);
                }
            
                // Content
                if (LayoutEngine.BeginVerticalGroup(GroupModifier.DiscardMargin, styleData.ContentStyle)) {
                    contentDrawer?.Invoke();
                }
                LayoutEngine.EndVerticalGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        public static void StaticCard(GUIContent header, Action contentDrawer) {
            StaticCard(header, contentDrawer, ComplexControlsResources.StaticCard);
        }
        
        public static void ExpandableCard(GUIContent header, float width, Action contentDrawer, AnimBool expanded, ComplexControlsResources.CardElementData styleData) {
            if (LayoutEngine.BeginVerticalGroup(GroupModifier.None, styleData.RootStyle)) {
                // Background
                EditorGUI.DrawRect(LayoutEngine.CurrentGroup.GetContentRect(), styleData.BackgroundColor);
                
                // Header
                if (LayoutEngine.GetRect(LabelHeight, width, out var headerRect)) {
                    expanded.target = EditorGUI.Foldout(headerRect, expanded.target, header, true, styleData.HeaderStyle);
                }
                
                // Separator
                if (expanded.faded > 0.01f && LayoutEngine.GetRect(1f, width, out var separatorRect)) {
                    DrawSeparator(separatorRect);
                }
                
                if (LayoutEngine.BeginVerticalFadeGroup(expanded.faded, GroupModifier.DiscardMargin, styleData.ContentStyle)) {
                    // Content
                    contentDrawer?.Invoke();
                }
                LayoutEngine.EndVerticalFadeGroup();
            }
            LayoutEngine.EndVerticalGroup();
        }
        public static void ExpandableCard(GUIContent header, float width, Action contentDrawer, AnimBool expanded) {
            ExpandableCard(header, width, contentDrawer, expanded, ComplexControlsResources.ExpandableCard);
        }
        

        public class ScrollableTabsHolder {
            private readonly GUIContent[] _tabHeaders;
            private readonly Action[] _contentDrawers;
            
            private readonly AnimFloat _animator;

            private int _currentTab;
            private int _previousTab;
            private Vector2 _scrollValue;

            private readonly Color _underlineColor;
            private readonly float _underlineHeight;
            private readonly GUIStyle _tabStyle;

            public ScrollableTabsHolder(int currentTab, GUIContent[] tabHeaders, Action[] contentDrawers, AnimFloat viewAnimator, Color underlineColor, GUIStyle tabStyle) {
                _currentTab = currentTab;
                _tabHeaders = tabHeaders;
                _contentDrawers = contentDrawers;
                
                _underlineColor = underlineColor;
                _underlineHeight = tabStyle.padding.bottom;
                _tabStyle = tabStyle;
                
                _animator = viewAnimator;
            }

            public ScrollableTabsHolder(int currentTab, GUIContent[] tabHeaders, Action[] contentDrawers, AnimFloat viewAnimator, Color underlineColor) 
                : this(currentTab, tabHeaders, contentDrawers, viewAnimator, underlineColor, ControlsResources.TabHeader) { }

            public int DoScrollableTabs() {
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var selectedTabRect)) {
                    EditorGUI.LabelField(selectedTabRect, $"Selected tab: {_currentTab + 1}");
                }
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var isAnimatingRect)) {
                    EditorGUI.LabelField(isAnimatingRect, $"Is animating: {_animator.isAnimating}");
                }
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var animValueRect)) {
                    EditorGUI.LabelField(animValueRect, $"Animation value: {_animator.value}");
                }
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var scrollValueRect)) {
                    EditorGUI.LabelField(scrollValueRect, $"Scroll pos: {_scrollValue}");
                }
                
                int currentSelection = _currentTab;
                float currentAnimationPosition = _animator.value / (_tabHeaders.Length - 1);

                // Tabs
                if (LayoutEngine.GetRect(22f, LayoutEngine.AutoWidth, out var toolbarRect)) {
                    // Logic
                    currentSelection = GUI.Toolbar(toolbarRect, currentSelection, _tabHeaders, _tabStyle);

                    // Underline
                    var singleTabWidth = toolbarRect.width / _tabHeaders.Length;
                    var maximumOriginOffset = singleTabWidth * (_tabHeaders.Length - 1);
                    var underlinePosX = toolbarRect.x + maximumOriginOffset * currentAnimationPosition;
                    var underlineRect = new Rect(underlinePosX, toolbarRect.yMax - _underlineHeight, singleTabWidth, _underlineHeight);
                    EditorGUI.DrawRect(underlineRect, _underlineColor);
                }

                // Content
                if (_animator.isAnimating) {
                    _scrollValue = new Vector2(currentAnimationPosition, 0f);
                    if (LayoutEngine.BeginScrollGroup(new Vector2(-1, -1), _scrollValue, false, GroupModifier.None, LayoutResources.PureScrollGroup)) {
                        if (LayoutEngine.BeginHorizontalGroup()) {
                            for (int i = 0; i < _tabHeaders.Length; i++) {
                                _contentDrawers[i].Invoke();
                            }
                        }
                        LayoutEngine.EndHorizontalGroup();
                    }
                    LayoutEngine.EndScrollGroup();
                }
                else {
                    _contentDrawers[_currentTab].Invoke();
                }

                // Change check
                if (currentSelection != _currentTab) {
                    _currentTab = currentSelection;
                    _animator.target = currentSelection;
                }

                return _currentTab;
            }
        }
    }
}