using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        public interface IDrawableElement {
            void OnGUI();
        }

        public class DelegateElemenet : IDrawableElement {
            public delegate void Drawer();

            private Drawer _drawer;

            public DelegateElemenet(Drawer drawer) {
                _drawer = drawer;
            }

            public void OnGUI(){
                _drawer();
            }
        }

        public class CardElement {
            private GUIContent _header;
            private Action _contentDrawer;

            private GUIStyle _headerStyle;
            private GUIStyle _rootContainerStyle;
            private GUIStyle _contentContainerStyle;

            private bool _drawRootBackground;
            private Color _rootBackgroundColor;

            private readonly LayoutEngine.LayoutGroupBase _rootGroup;
            private readonly LayoutEngine.LayoutGroupBase _contentGroup;

            public CardElement(GUIContent header, Action contentDrawer, GUIStyle headerStyle, GUIStyle rootStyle, GUIStyle contentStyle){
                _header = header;
                _contentDrawer = contentDrawer;

                _headerStyle = headerStyle;
                _rootContainerStyle = rootStyle;
                _contentContainerStyle = contentStyle;

                _rootBackgroundColor = rootStyle.normal.textColor;
                _drawRootBackground = _rootBackgroundColor.a > 0f;

                _rootGroup = new LayoutEngine.VerticalGroup(GroupModifier.None, _rootContainerStyle);
                _contentGroup = new LayoutEngine.VerticalGroup(GroupModifier.None, EmptyStyle);
            }
            public CardElement(GUIContent header, Action contentDrawer)
                : this(
                    header,
                    contentDrawer,
                    ControlsResources.Label,
                    GUIElementsResources.CardRoot,
                    GUIElementsResources.CardContent
                ) {}

            public void OnGUI(){
                // if (LayoutEngine.BeginVerticalGroup(GroupModifier.None, _rootContainerStyle)) {
                if (LayoutEngine.BeginVerticalGroup(_rootGroup)) {
                    // Background
                    if(_drawRootBackground && Event.current.type == EventType.Repaint){
                        EditorGUI.DrawRect(LayoutEngine.CurrentGroup.GetContentRect(), _rootBackgroundColor);
                    }

                    // Header
                    if (LayoutEngine.GetRect(LabelHeight, LayoutEngine.AutoWidth, out var headerRect)) {
                        EditorGUI.LabelField(headerRect, _header, _headerStyle);
                    }
                
                    // Separator
                    if (LayoutEngine.GetRect(1f, LayoutEngine.AutoWidth, out var separatorRect)) {
                        DrawSeparator(separatorRect);
                    }

                    // Content
                    if (LayoutEngine.BeginVerticalGroup(_contentGroup)) {
                        _contentDrawer?.Invoke();
                    }
                    LayoutEngine.EndVerticalGroup();
                }
                LayoutEngine.EndVerticalGroup();
            }
        }

        public class FoldableCardElement {
            private GUIContent _header;
            private Action _contentDrawer;

            private AnimBool _expanded;

            private GUIStyle _headerContentStyle;
            private GUIStyle _rootContainerStyle;
            private GUIStyle _contentContainerStyle;

            public FoldableCardElement(GUIContent header, Action contentDrawer, bool expanded, GUIStyle headerStyle, GUIStyle rootStyle, GUIStyle contentStyle){
                _header = header;
                _contentDrawer = contentDrawer;

                _expanded = new AnimBool(expanded, ExtendedEditorGUI.CurrentViewRepaint);

                _headerContentStyle = headerStyle;
                _rootContainerStyle = rootStyle;
                _contentContainerStyle = contentStyle;
            }

            public void OnGUI(){
                if (LayoutEngine.BeginVerticalGroup(GroupModifier.None, _rootContainerStyle)) {
                    // Background
                    // if(_drawRootBackground && Event.current.type == EventType.Repaint){
                    //     EditorGUI.DrawRect(LayoutEngine.CurrentGroup.GetContentRect(), _rootBackgroundColor);
                    // }
                    
                    // Header
                    if (LayoutEngine.GetRect(LabelHeight, LayoutEngine.AutoWidth, out var headerRect)) {
                        _expanded.target = EditorGUI.Foldout(headerRect, _expanded.target, _header, true, _headerContentStyle);
                    }
                    
                    // Separator
                    if (_expanded.faded > 0.01f && LayoutEngine.GetRect(1f, LayoutEngine.AutoWidth, out var separatorRect)) {
                        DrawSeparator(separatorRect);
                    }
                    
                    if (LayoutEngine.BeginVerticalFadeGroup(_expanded.faded, GroupModifier.DiscardMargin, _contentContainerStyle)) {
                        // Content
                        _contentDrawer?.Invoke();
                    }
                    LayoutEngine.EndVerticalFadeGroup();
                }
                LayoutEngine.EndVerticalGroup();
            }
        }

        public class TabsElement {
            private readonly GUIContent[] _tabHeaders;
            private readonly Action[] _contentDrawers;
            
            private readonly AnimFloat _animator;

            private int _currentTab;
            private int _previousTab;

            private readonly Color _underlineColor;
            private readonly float _underlineHeight;

            private readonly GUIStyle _tabStyle;

            private readonly LayoutEngine.ScrollGroup _scrollGroup;
            private readonly LayoutEngine.LayoutGroupBase _horizontalGroup;
            

            public TabsElement(int currentTab, GUIContent[] tabHeaders, Action[] contentDrawers, Color underlineColor, GUIStyle tabStyle) {
                _currentTab = currentTab;
                _tabHeaders = tabHeaders;
                _contentDrawers = contentDrawers;
                
                _underlineColor = underlineColor;
                _underlineHeight = tabStyle.padding.bottom;
                _tabStyle = tabStyle;
                
                _animator = new AnimFloat(currentTab, ExtendedEditorGUI.CurrentViewRepaint);

                _scrollGroup = new LayoutEngine.ScrollGroup(new Vector2(-1, -1), new Vector2(currentTab / (_tabHeaders.Length - 1), 0f), true, GroupModifier.None, ExtendedEditorGUI.EmptyStyle);
                _horizontalGroup = new LayoutEngine.HorizontalGroup(GroupModifier.None, ExtendedEditorGUI.EmptyStyle);
            }
            public TabsElement(int currentTab, GUIContent[] tabHeaders, Action[] contentDrawers, Color underlineColor) 
                : this(currentTab, tabHeaders, contentDrawers, underlineColor, ControlsResources.TabHeader) { }

            public int OnGUI() {
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
                    EditorGUI.LabelField(scrollValueRect, $"Scroll pos: {_scrollGroup.ScrollPos}");
                }
                
                int currentSelection = _currentTab;
                float currentAnimationPosition = _animator.value / (_tabHeaders.Length - 1);

                // Tabs
                if (LayoutEngine.GetRect(22f, LayoutEngine.AutoWidth, out var toolbarRect)) {
                    // Tab control
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
                    _scrollGroup.ScrollPos = new Vector2(currentAnimationPosition, 0f);
                    if(LayoutEngine.BeginScrollGroup(_scrollGroup)) {
                        if(LayoutEngine.BeginLayoutGroup(_horizontalGroup)) {
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