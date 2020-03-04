#define DYNAMIC_STYLING

using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        public interface IDrawableElement {
            void OnGUI();
        }

        public class DelegateElement : IDrawableElement {
            private Action _drawer;

            public DelegateElement(Action drawer) {
                _drawer = drawer;
            }

            public void OnGUI() {
                _drawer();
            }
        }

        public class CardElement : IDrawableElement  {
            // GUI content & drawers
            private GUIContent _header;
            private IDrawableElement _contentDrawer;

            // Styling
            private GUIStyle _headerStyle;
            private float _headerHeight;

            private bool _drawRootBackground;
            private Color _backgroundColor;

            // Layout groups
            private readonly LayoutEngine.LayoutGroupBase _rootGroup;
            private readonly LayoutEngine.LayoutGroupBase _contentGroup;

            public CardElement(GUIContent header, IDrawableElement contentDrawer, GUIStyle headerStyle, GUIStyle rootStyle, GUIStyle contentStyle){
                // GUI content & drawers
                _header = header;
                _contentDrawer = contentDrawer;

                // Styling 
                _headerStyle = headerStyle;
                _headerHeight = headerStyle.GetContentHeight(header);

                var rootNormalState = rootStyle.normal;
                _backgroundColor = rootNormalState.textColor;
                _drawRootBackground = _backgroundColor.a > 0f;

                // Layout groups
                _rootGroup = new LayoutEngine.VerticalGroup(GroupModifier.None, rootStyle);
                _contentGroup = new LayoutEngine.VerticalGroup(GroupModifier.None, contentStyle);
            }
            public CardElement(GUIContent header, IDrawableElement contentDrawer)
                : this(
                    header,
                    contentDrawer,
                    GUIElementsResources.CardHeader,
                    GUIElementsResources.CardRoot,
                    GUIElementsResources.CardContent
                ) {}

            public void OnGUI(){
                RecalculateStyling();

                if (LayoutEngine.BeginVerticalGroup(_rootGroup)) {
                    // Background
                    if(_drawRootBackground && Event.current.type == EventType.Repaint){
                        EditorGUI.DrawRect(_rootGroup.GetContentRect(), _backgroundColor);
                    }

                    // Header
                    if (LayoutEngine.GetRect(_headerHeight, LayoutEngine.AutoWidth, out var headerRect)) {
                        EditorGUI.LabelField(headerRect, _header, _headerStyle);
                    }
                
                    // Separator
                    if (LayoutEngine.GetRect(1f, LayoutEngine.AutoWidth, out var separatorRect)) {
                        DrawSeparator(separatorRect);
                    }

                    // Content
                    if (LayoutEngine.BeginVerticalGroup(_contentGroup)) {
                        _contentDrawer.OnGUI();
                    }
                    LayoutEngine.EndVerticalGroup();
                }
                LayoutEngine.EndVerticalGroup();
            }

            [Conditional("DYNAMIC_STYLING")]
            private void RecalculateStyling() {
                _headerHeight = _headerStyle.GetContentHeight(_header);
            }
        }

        public class FoldableCardElement : IDrawableElement {
            // Logic data
            public bool Expanded => _expandedAnimator.target;

            // GUI content & drawers
            private GUIContent _header;
            private IDrawableElement _contentDrawer;

            // Animators
            private AnimBool _expandedAnimator;

            // Styling
            private GUIStyle _headerStyle;
            private float _headerHeight;

            private bool _drawRootBackground;
            private Color _rootBackgroundColor;

            // Layout groups
            private readonly LayoutEngine.LayoutGroupBase _rootGroup;
            private readonly LayoutEngine.VerticalFadeGroup _expandingContentGroup;
            private readonly LayoutEngine.LayoutGroupBase _expandedContentGroup;

            public FoldableCardElement(GUIContent header, IDrawableElement contentDrawer, bool expanded, GUIStyle headerStyle, GUIStyle rootStyle, GUIStyle contentStyle){
                // GUI content & drawers
                _header = header;
                _contentDrawer = contentDrawer;

                // Animators
                _expandedAnimator = new AnimBool(expanded, ExtendedEditorGUI.CurrentViewRepaint);

                // Styling
                _headerStyle = headerStyle;
                _headerHeight = headerStyle.GetContentHeight(header);

                _rootBackgroundColor = rootStyle.normal.textColor;
                _drawRootBackground = _rootBackgroundColor.a > 0f;

                // Layout groups
                _rootGroup = new LayoutEngine.VerticalGroup(GroupModifier.None, GUIElementsResources.CardRoot);
                _expandingContentGroup = new LayoutEngine.VerticalFadeGroup(_expandedAnimator.faded, GroupModifier.None, GUIElementsResources.CardContent);
                _expandedContentGroup = new LayoutEngine.VerticalGroup(GroupModifier.None, GUIElementsResources.CardContent);
            }
            public FoldableCardElement(GUIContent header, IDrawableElement contentDrawer, bool expanded)
                : this(
                    header,
                    contentDrawer,
                    expanded,
                    GUIElementsResources.CardFoldoutHeader,
                    GUIElementsResources.CardRoot,
                    GUIElementsResources.CardContent
                ) {}

            public void OnGUI(){
                RecalculateStyling();
                
                if (LayoutEngine.BeginVerticalGroup(_rootGroup)) {
                    // Background
                    if(_drawRootBackground && Event.current.type == EventType.Repaint){
                        EditorGUI.DrawRect(_rootGroup.GetContentRect(), _rootBackgroundColor);
                    }
                    
                    // Header
                    var expanded = _expandedAnimator.target;
                    if (LayoutEngine.GetRect(_headerHeight, LayoutEngine.AutoWidth, out var headerRect)) {
                        expanded = EditorGUI.Foldout(headerRect, _expandedAnimator.target, _header, true, _headerStyle);
                    }
                    
                    // Separator
                    if (_expandedAnimator.faded > 0.01f && LayoutEngine.GetRect(1f, LayoutEngine.AutoWidth, out var separatorRect)) {
                        DrawSeparator(separatorRect);
                    }
                    
                    // Content
                    if(_expandedAnimator.isAnimating){
                        _expandingContentGroup.Faded = _expandedAnimator.faded;
                        if (LayoutEngine.BeginVerticalFadeGroup(_expandingContentGroup)) {
                            _contentDrawer.OnGUI();
                        }
                        LayoutEngine.EndVerticalFadeGroup();
                    }
                    else if(_expandedAnimator.target) {
                        if(LayoutEngine.BeginVerticalGroup(_expandedContentGroup)) {
                            _contentDrawer.OnGUI();
                        }
                        LayoutEngine.EndVerticalGroup();
                    }

                    // Change check
                    _expandedAnimator.target = expanded;
                }
                LayoutEngine.EndVerticalGroup();
            }

            [Conditional("DYNAMIC_STYLING")]
            private void RecalculateStyling() {
                _headerHeight = _headerStyle.GetContentHeight(_header);
            }
        }

        public class TabsElement : IDrawableElement {
            // Logic data
            public int CurrentTab {get; set;}
            
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
            private readonly LayoutEngine.ScrollGroup _scrollGroup;
            private readonly LayoutEngine.LayoutGroupBase _horizontalGroup;
            
            public TabsElement(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor, GUIStyle tabHeaderStyle) {
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
                _scrollGroup = new LayoutEngine.ScrollGroup(new Vector2(-1, -1), new Vector2(initialTab / (_tabHeaders.Length - 1), 0f), true, GroupModifier.None, ExtendedEditorGUI.EmptyStyle);
                _horizontalGroup = new LayoutEngine.HorizontalGroup(GroupModifier.None, ExtendedEditorGUI.EmptyStyle);
            }
            public TabsElement(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor) 
                : this(initialTab, tabHeaders, contentDrawers, underlineColor, GUIElementsResources.TabHeader) { }

            public void OnGUI() {
                RecalculateStyling();

                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var selectedTabRect)) {
                    EditorGUI.LabelField(selectedTabRect, $"Selected tab: {CurrentTab + 1}");
                }
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var isAnimatingRect)) {
                    EditorGUI.LabelField(isAnimatingRect, $"Is animating: {_animator.isAnimating}");
                }
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var animValueRect)) {
                    EditorGUI.LabelField(animValueRect, $"Animation value: {_animator.value}");
                }
                if (LayoutEngine.GetRect(18f, LayoutEngine.AutoWidth, out var scrollValueRect)) {
                    EditorGUI.LabelField(scrollValueRect, $"Scroll pos: {_scrollGroup.ScrollPos}");
                }
                
                int currentSelection = CurrentTab;
                float currentAnimationPosition = _animator.value / (_tabHeaders.Length - 1);

                // Tabs
                if (LayoutEngine.GetRect(_tabHeaderHeight, LayoutEngine.AutoWidth, out var toolbarRect)) {
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
                    if(LayoutEngine.BeginScrollGroup(_scrollGroup)) {
                        if(LayoutEngine.BeginLayoutGroup(_horizontalGroup)) {
                            for (int i = 0; i < _tabHeaders.Length; i++) {
                                _contentDrawers[i].OnGUI();
                            }
                        }
                        LayoutEngine.EndHorizontalGroup();
                    }
                    LayoutEngine.EndScrollGroup();
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
        
            [Conditional("DYNAMIC_STYLING")]
            private void RecalculateStyling() {
                _tabHeaderHeight = _tabHeaderStyle.GetContentHeight(_tabHeaders[0]);
                _underlineHeight = _tabHeaderStyle.margin.bottom;
            }
        }
    }
}