#define DYNAMIC_STYLING

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Assertions;

using Debug = UnityEngine.Debug;


namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        public interface IDrawableElement {
            void OnGUI();
        }

        public interface IAbsoluteDrawableElement : IDrawableElement {
            void OnGUI(Vector2 position);
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

        // TODO: develop this
        private class SerializedPropertyElement : IDrawableElement {
            private GUIContent _header;

            private SerializedProperty _property;
            private SerializedProperty[] _children;

            private bool _hasChildren;

            private LayoutEngine.VerticalGroup _childrenContentGroup;

            public SerializedPropertyElement(GUIContent header, SerializedProperty property) {
                _property = property;

                _hasChildren = property.hasChildren;
                if(_hasChildren){
                    var list = new List<SerializedProperty>();
                    var it = _property.Copy();
                    while(it.Next(false)){
                        list.Add(it);
                    }
                    _children = list.ToArray();
                }
            }
            public SerializedPropertyElement(SerializedProperty property)
                : this(new GUIContent(property.displayName), property) {}

            public void OnGUI() {

            }
        }

        public class Card : IDrawableElement  {
            // GUI content & drawers
            private GUIContent _header;
            private IDrawableElement _contentDrawer;

            // Styling
            private GUIStyle _headerStyle;
            private float _headerHeight;

            private bool _drawRootBackground;
            private Color _backgroundColor;

            private int _gradientIndex;

            // Layout groups
            private readonly LayoutEngine.LayoutGroupBase _rootGroup;
            private readonly LayoutEngine.LayoutGroupBase _contentGroup;

            public Card(GUIContent header, IDrawableElement contentDrawer, GUIStyle headerStyle, GUIStyle rootStyle, GUIStyle contentStyle){
                // GUI content & drawers
                _header = header;
                _contentDrawer = contentDrawer;

                // Styling
                _headerStyle = headerStyle;
                _headerHeight = headerStyle.GetContentHeight(header);

                var rootNormalState = rootStyle.normal;
                _backgroundColor = rootNormalState.textColor;
                _drawRootBackground = _backgroundColor.a > 0f;

                _gradientIndex = rootStyle.fontSize;

                // Layout groups
                _rootGroup = new LayoutEngine.VerticalGroup(Constraints.None, rootStyle);
                _contentGroup = new LayoutEngine.VerticalGroup(Constraints.None, contentStyle);
            }
            public Card(GUIContent header, IDrawableElement contentDrawer)
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
                    if (_drawRootBackground && Event.current.type == EventType.Repaint){
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

        public class FoldableCard : IDrawableElement {
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

            private int _gradientIndex;

            // Layout groups
            private readonly LayoutEngine.LayoutGroupBase _rootGroup;
            private readonly LayoutEngine.VerticalFadeGroup _expandingContentGroup;
            private readonly LayoutEngine.LayoutGroupBase _expandedContentGroup;

            public FoldableCard(GUIContent header, IDrawableElement contentDrawer, bool expanded, GUIStyle headerStyle, GUIStyle rootStyle, GUIStyle contentStyle){
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

                _gradientIndex = rootStyle.fontSize;

                // Layout groups
                _rootGroup = new LayoutEngine.VerticalGroup(Constraints.None, rootStyle);
                _expandingContentGroup = new LayoutEngine.VerticalFadeGroup(_expandedAnimator.faded, Constraints.None, contentStyle);
                _expandedContentGroup = new LayoutEngine.VerticalGroup(Constraints.None, contentStyle);
            }
            public FoldableCard(GUIContent header, IDrawableElement contentDrawer, bool expanded)
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

        public class Tabs : IDrawableElement {
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
                _scrollGroup = new LayoutEngine.ScrollGroup(new Vector2(-1, -1), new Vector2(initialTab / (_tabHeaders.Length - 1), 0f), true, Constraints.None, ExtendedEditorGUI.EmptyStyle);
                _horizontalGroup = new LayoutEngine.HorizontalGroup(Constraints.None, ExtendedEditorGUI.EmptyStyle);
            }
            public Tabs(int initialTab, GUIContent[] tabHeaders, IDrawableElement[] contentDrawers, Color underlineColor)
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

        // TODO: develop this
        public class SerializedPropertyList : IList {
            public object this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public bool IsFixedSize => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public int Count => throw new NotImplementedException();

            public bool IsSynchronized => throw new NotImplementedException();

            public object SyncRoot => throw new NotImplementedException();

            public int Add(object value) {
                throw new NotImplementedException();
            }

            public void Clear() {
                throw new NotImplementedException();
            }

            public bool Contains(object value) {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            public IEnumerator GetEnumerator() {
                throw new NotImplementedException();
            }

            public int IndexOf(object value) {
                throw new NotImplementedException();
            }

            public void Insert(int index, object value) {
                throw new NotImplementedException();
            }

            public void Remove(object value) {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index) {
                throw new NotImplementedException();
            }
        }

        // MAYBE
        // TODO: rebind only changed data
        // TODO: drag and drop between lists
        // TODO: context scrolling
        public class ListView<TData, TDrawer> : IDrawableElement where TDrawer : IDrawableElement, new() {
            // Hash for control id generation
            private int ListViewControlIdHint = "ListView".GetHashCode();

            /* State for FSM */
            private enum State {
                Default,
                Reordering,
                ScrollingToIndex
            }
            private State _state;

            /* Control id */
            private int _currentControlId;

            /* Source list */
            private IList<TData> _sourceList;
            public int Count => _sourceList.Count;

            /* Layout scroll group */
            private readonly LayoutEngine.ScrollGroup _contentScrollGroup;

            /* Drawers */
            private readonly List<IDrawableElement> _drawers = new List<IDrawableElement>();
            private readonly Action<TData, IDrawableElement, bool> _bindDataToDrawer;
            private readonly bool _canDrawInAbsoluteCoords;

            /* Animated scrolling */
            private readonly AnimFloat _animator = new AnimFloat(0f, CurrentViewRepaint);

            /* Element selection */
            private int _activeElementIndex = -1;
            private readonly HashSet<int> _selectedIndices = new HashSet<int>();
            // Selection & Deselection callback
            public Action<int, IDrawableElement> OnElementSelected;
            public Action<int, IDrawableElement> OnElementDeselected;
            // Double click callback logic
            public Action<int, TData> OnElementDoubleClick;
            private double _lastClickTime;
            private const double DoubleClickTimingWindow = 0.25; // 1/4 second time window

            /* Drag & drop */
            private bool _isDragDataValidated;
            private DragAndDropVisualMode _dragOperationType;
            public Func<DragAndDropVisualMode> ValidateDragData;
            public Func<IEnumerable<TData>> ExtractDragData;

            /* Reordering */
            public Action<int, int> OnElementsReorder;
            private float _activeElementPosY;
            private Color _reorderableElementTint = Color.white;
            public float ReorderableElementAlpha {
                set => _reorderableElementTint.a = value;
            }

            /* Rendering & occlusion */
            private readonly float _elementHeight;
            private readonly float _spaceBetweenElements;
            private readonly float _elementHeightWithSpace;
            private readonly float _visibleHeight;
            private float _totalElementsHeight;
            private float _visibleContentOffset;
            private int _firstVisibleIndex;
            private int _visibleElementsCount;

            /* Empty list drawing data */
            // Texture
            private const float IconSize = 56;
            private readonly Texture _emptyListIcon;
            // Label
            private readonly GUIStyle _labelStyle;
            private float _emptyListLabelHeight;
            private GUIContent _emptyListLabel;
            public GUIContent EmptyListLabel {
                set {
                    _emptyListLabel = value;
                    _emptyListLabelHeight = _labelStyle.GetContentHeight(value);
                }
            }


            public ListView(IList<TData> source, Vector2 container, float elementHeight, Action<TData, IDrawableElement, bool> bind) {
                _bindDataToDrawer = bind;

                _sourceList = source;

                _labelStyle = _controlsResources.CenteredGreyHeader;
    
                EmptyListLabel = new GUIContent("This list is empty");
                _emptyListIcon = _guiElementsResources.EmptyListIcon;

                _contentScrollGroup = new LayoutEngine.ScrollGroup(container, Vector2.zero, false, Constraints.None, LayoutResources.ScrollGroup);

                _elementHeight = elementHeight;
                _spaceBetweenElements = _contentScrollGroup.SpaceBetweenEntries;
                _elementHeightWithSpace = _elementHeight + _spaceBetweenElements;
                _visibleHeight = container.y - _contentScrollGroup.ConstraintsHeight;
                Assert.AreNotApproximatelyEqual(_visibleHeight, 0f);

                var maxVisibleElements = Mathf.CeilToInt(_visibleHeight / _elementHeightWithSpace);
                var nextElementStart = maxVisibleElements * _elementHeightWithSpace;
                var heightToNextElement = nextElementStart - _visibleHeight;
                if(_elementHeight > heightToNextElement) {
                    maxVisibleElements += 1;
                }
                else {
                    maxVisibleElements = Mathf.Min(maxVisibleElements, source.Count);
                }

                // creating new drawers
                for (int i = 0; i < maxVisibleElements; i++) {
                    _drawers.Add(new TDrawer());
                }
                
                _canDrawInAbsoluteCoords = typeof(TDrawer).GetInterfaces().Contains(typeof(IAbsoluteDrawableElement));

                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();
            }
            public ListView(IList<TData> source, float height, float elementHeight, Action<TData, IDrawableElement, bool> bind)
                : this(source, new Vector2(-1, height), elementHeight, bind){}

            public void OnGUI() {
                var preScrollPos = _contentScrollGroup.ScrollPosY;
                if (LayoutEngine.BeginScrollGroup(_contentScrollGroup)) {
                    if(_sourceList.Count != 0) {
                        DoContent();
                    }
                    else {
                        DoEmptyContent();
                    }
                }
                var postScrollPos = LayoutEngine.EndScrollGroup().y;

                // if scroll pos changed => recalculate visible elements & rebind drawers if needed
                if(!Mathf.Approximately(preScrollPos, postScrollPos) && Event.current.type != EventType.Layout) {
                    CalculateVisibleElements();
                    RebindDrawers();
                }
            }

            /* Top-level GUI methods */
            private void DoContent() {
                var eventType = Event.current.type;

                // register full array of elements
                if (eventType == EventType.Layout) {
                    _contentScrollGroup.RegisterArray(_elementHeight, _sourceList.Count);
                    return;
                }

                // skip invisible elements
                if(_firstVisibleIndex > 0) {
                    var totalSkipHeight = _firstVisibleIndex * _elementHeightWithSpace - _spaceBetweenElements;
                    _contentScrollGroup.GetNextEntryRect(LayoutEngine.AutoWidth, totalSkipHeight);
                }

                switch(_state) {
                    case State.Default:
                        DoVisibleContent();
                        HandleEventsAtDefault();
                        break;
                    case State.Reordering:
                        if(_canDrawInAbsoluteCoords) {
                            DoReorderingContent();
                        }
                        else {
                            DoVisibleContent();
                        }
                        HandleEventsAtReordering();
                        break;
                    case State.ScrollingToIndex:
                        if(Event.current.type == EventType.Repaint) {
                            DoVisibleContent();
                            _contentScrollGroup.ScrollPosY = _animator.value;
                        }

                        if(!_animator.isAnimating) {
                            _state = State.Default;
                        }

                        break;
                }
            }
            private void DoVisibleContent() {
                for (int i = 0; i < _visibleElementsCount; i++) {
                    _drawers[i].OnGUI();
                }
            }
            private void DoReorderingContent() {
                var reorderableDrawerIndex = GetDrawerIndexFromDataIndex(_activeElementIndex);
                // drawing before held element
                for (int i = 0; i < reorderableDrawerIndex; i++) {
                    _drawers[i].OnGUI();
                }

                // Requesting held element space
                var initialHeldRect = _contentScrollGroup.GetNextEntryRect(LayoutEngine.AutoWidth, _elementHeight);

                // drawing after held element
                for (int i = reorderableDrawerIndex + 1; i < _visibleElementsCount; i++) {
                    _drawers[i].OnGUI();
                }

                var color = GUI.color;
                GUI.color *= _reorderableElementTint;
                (_drawers[reorderableDrawerIndex] as IAbsoluteDrawableElement).OnGUI(new Vector2(initialHeldRect.x, _activeElementPosY - _visibleContentOffset));
                GUI.color = color;
            }
            private void DoEmptyContent() {
                if(_contentScrollGroup.GetNextEntryRect(LayoutEngine.AutoWidth, IconSize, out var iconRect)) {
                    iconRect.x += (iconRect.width / 2) - IconSize / 2;
                    iconRect.width = IconSize;

                    GUI.DrawTexture(iconRect, _emptyListIcon);
                }
                if(_contentScrollGroup.GetNextEntryRect(LayoutEngine.AutoWidth, _emptyListLabelHeight, out var labelRect)) {
                    EditorGUI.LabelField(labelRect, _emptyListLabel, _labelStyle);
                }

                HandleEventsAtDefault();
            }

            /* Event handling */
            private EventType FilterCurrentEvent(Event evt) {
                _currentControlId = GUIUtility.GetControlID(ListViewControlIdHint, FocusType.Passive);
                var type = evt.GetTypeForControl(_currentControlId);

                switch(type) {
                    case EventType.DragUpdated:
                        if(ValidateDragData == null || ExtractDragData == null) {
                            type = EventType.Ignore;
                        }
                        break;
                    default:
                    break;
                }
                
                return type;
            }
            private void HandleEventsAtDefault() {
                var evt = Event.current;
                switch (FilterCurrentEvent(evt)) {
                    // Selection, reordering & context click
                    case EventType.MouseDown:
                        HandleMouseDown(evt);
                        break;
                    // Drag and drop
                    case EventType.DragUpdated:
                        HandleDragUpdated(evt);
                        break;
                    case EventType.DragExited:
                        HandleDragExited(evt);
                        break;
                    case EventType.DragPerform:
                        HandleDragPerform(evt);
                        break;
                }
            }
            private void HandleEventsAtReordering() {
                var evt = Event.current;
                switch (FilterCurrentEvent(evt)) {
                    // Selection & reordering
                    case EventType.MouseUp:
                        HandleMouseUp(evt);
                        break;
                    case EventType.MouseDrag:
                        HandleMouseDrag(evt);
                        break;
                }
            }
            // Mouse clicks & movement
            private void HandleMouseDown(Event evt) {
                // context click
                if(evt.button == 1) {
                    HandleContextClick();
                }
                // selection click
                else {
                    if(PositionToDataIndex(evt.mousePosition.y, out int clickIndex)) {
                        GUIUtility.hotControl = _currentControlId;
                        _state = State.Reordering;
                        _activeElementPosY = clickIndex * _elementHeightWithSpace;
                        MouseSelectIndex(clickIndex);
                    }
                    else {
                        DeselectEverything();
                    }
                }
                evt.Use();
            }           
            private void HandleMouseUp(Event evt) {
                if(GUIUtility.hotControl == _currentControlId) {
                    _state = State.Default;
                    GUIUtility.hotControl = 0;
                    evt.Use();
                }
            }
            private void HandleMouseDrag(Event evt) {
                var draggableStartPos = _activeElementIndex * _elementHeightWithSpace;
                    var movementDelta = evt.delta.y;
                    _activeElementPosY += movementDelta;

                    // going up
                    if(movementDelta < 0 && _activeElementIndex > 0) {
                        var reorderBoundary = (_activeElementIndex - 1) * _elementHeightWithSpace + _elementHeight / 2;
                        if(_activeElementPosY <= reorderBoundary) {
                            HandleReorder(_activeElementIndex, _activeElementIndex - 1);
                        }
                    }
                    // doing down
                    else if(_activeElementIndex < _sourceList.Count - 1) {
                        var reorderBoundary = (_activeElementIndex + 1) * _elementHeightWithSpace - _elementHeight / 2;
                        if(_activeElementPosY >= reorderBoundary) {
                            HandleReorder(_activeElementIndex, _activeElementIndex + 1);
                        }
                    }

                    CurrentViewRepaint();
            }
            private void HandleContextClick() {
                var menu = new GenericMenu();
                // Delete selected
                var deleteSelectedContent = new GUIContent("Delete selected");
                if(_selectedIndices.Count != 0) menu.AddItem(deleteSelectedContent, false, RemoveSelected);
                else menu.AddDisabledItem(deleteSelectedContent);
                // Delete all
                var deleteAllContent = new GUIContent("Delete all");
                if(_sourceList.Count != 0) menu.AddItem(deleteAllContent, false, Clear);
                else menu.AddDisabledItem(deleteAllContent);

                menu.ShowAsContext();
            }

            // Drag & Drop
            private void HandleDragUpdated(Event evt) {
                if(!_isDragDataValidated) {
                    _isDragDataValidated = true;
                    _dragOperationType = ValidateDragData();
                }
                if(_isDragDataValidated){
                    DragAndDrop.visualMode = _dragOperationType;
                }
                evt.Use();
            }
            private void HandleDragPerform(Event evt) {
                foreach(var element in ExtractDragData()) {
                    _sourceList.Add(element);
                }
                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();

                _state = State.Default;
                evt.Use();
            }
            private void HandleDragExited(Event evt) {
                _state = State.Default;
                _isDragDataValidated = false;
                evt.Use();
            }

            /* Helpers */
            private void CalculateTotalHeight() {
                _totalElementsHeight = _sourceList.Count * _elementHeightWithSpace - _spaceBetweenElements;
            }
            private bool PositionToDataIndex(float position, out int index) {
                var absolutePosition = position + _visibleContentOffset;
                index = (int)(absolutePosition / _elementHeightWithSpace);

                float indexElementBottomBorder = index * _elementHeightWithSpace + _elementHeight;
                return indexElementBottomBorder > absolutePosition;
            }
            private void CalculateVisibleElements() {
                if(_totalElementsHeight <= _visibleHeight) {
                    _visibleContentOffset = 0;
                    _firstVisibleIndex = 0;
                    _visibleElementsCount = _sourceList.Count;
                    return;
                }

                _visibleContentOffset = (_totalElementsHeight - _visibleHeight) * _contentScrollGroup.ScrollPosY;

                // First index
                if(!PositionToDataIndex(0, out _firstVisibleIndex)) {
                    _firstVisibleIndex += 1;
                }

                // Last  index
                var lastIndex = (int)((_visibleHeight + _visibleContentOffset) / _elementHeightWithSpace);
                _visibleElementsCount = lastIndex - _firstVisibleIndex + 1;
            }
            
            /* Drawers */
            private void RebindDrawers() {
                for(int i = 0, dataIndex = _firstVisibleIndex; i < _visibleElementsCount; i++, dataIndex++) {
                    var selected = _selectedIndices.Contains(dataIndex);
                    _bindDataToDrawer(_sourceList[dataIndex], _drawers[i], selected);
                }
            }
            private int GetDrawerIndexFromDataIndex(int index) {
                var shiftedIndex = index - _firstVisibleIndex;
                var drawerIndexInVisibleRange = shiftedIndex >= 0 && shiftedIndex < _visibleElementsCount;
                return drawerIndexInVisibleRange ? shiftedIndex : -1;
            }
            private bool DoesDataHasVisibleDrawer(int index) {
                return GetDrawerIndexFromDataIndex(index) != -1;
            }
            private IDrawableElement GetDataDrawer(int index) {
                var drawerIndex = GetDrawerIndexFromDataIndex(index);
                if(drawerIndex != -1) {
                    return _drawers[drawerIndex];
                }
                return null;
            }
            // Reordering
            private void HandleReorder(int activeIndex, int passiveIndex) {
                _sourceList.SwapElementsInplace(activeIndex, passiveIndex);
                _activeElementIndex += activeIndex > passiveIndex ? -1 : 1;

                var activeDrawerIndex = GetDrawerIndexFromDataIndex(activeIndex);
                var passiveDrawerIndex = GetDrawerIndexFromDataIndex(passiveIndex);

                if(activeDrawerIndex != -1 && passiveDrawerIndex != -1) {
                    _drawers.SwapElementsInplace(activeDrawerIndex, passiveDrawerIndex);
                    _selectedIndices.Remove(activeIndex);
                    _selectedIndices.Add(passiveIndex);
                }
                else {
                    _state = State.Default;
                }
                OnElementsReorder?.Invoke(activeIndex, passiveIndex);
            }
            
            /* Mouse clicks */
            // Element selection
            private void MouseSelectIndex(int index) {
                var currentTime = EditorApplication.timeSinceStartup;
                if(currentTime - _lastClickTime <= DoubleClickTimingWindow && index == _activeElementIndex) {
                    OnElementDoubleClick?.Invoke(index, _sourceList[index]);
                }
                else {
                    switch(Event.current.modifiers) {
                        case EventModifiers.None:
                            GreedySelection(index);
                            break; 
                        case EventModifiers.Shift:
                            ShiftSelection(index);
                            break; 
                        case EventModifiers.Control:
                            ControlSelection(index);
                            break; 
                    }
                    _activeElementIndex = index;
                }
                _lastClickTime = currentTime;
            }
            private void GreedySelection(int index) {
                if(_selectedIndices.Contains(index)) {
                    _selectedIndices.Remove(index);
                }
                else {
                    OnElementSelected?.Invoke(index, GetDataDrawer(index));
                }
                DeselectEverything();
                _selectedIndices.Add(index);
            }
            private void ShiftSelection(int index) {
                var start = _activeElementIndex + 1;
                var end = index;
                if(start > end) {
                    start -= 2;
                    Utility.Swap(ref start, ref end);
                }
                for(int i = start; i <= end; i++) {
                    _selectedIndices.Add(i);
                }
                if(OnElementSelected != null) {
                    for(int i = start; i <= end; i++) {
                        OnElementSelected.Invoke(i, GetDataDrawer(i));
                    }
                }
            }
            private void ControlSelection(int index) {
                if(_selectedIndices.Contains(index)) {
                    OnElementDeselected?.Invoke(index, GetDataDrawer(index));
                    _selectedIndices.Remove(index);
                }
                else {
                    OnElementSelected?.Invoke(index, GetDataDrawer(index));
                    _selectedIndices.Add(index);
                }
            }
            private void DeselectEverything() {
                if(OnElementDeselected != null) {
                    foreach(var index in _selectedIndices) {
                        OnElementDeselected(index, GetDataDrawer(index));
                    }
                }
                _selectedIndices.Clear();
                _activeElementIndex = -1;
            }
            // Context menu
            private void RemoveSelected() {
                foreach(var index in _selectedIndices.OrderByDescending(i => i)) {
                    _sourceList.RemoveAt(index);
                }
                _selectedIndices.Clear();
                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();
            }

            /* Scroll to values */
            public void ScrollTo(int index) {
                var indexScrollPos = Mathf.Clamp01(index * _elementHeightWithSpace / (_totalElementsHeight - _visibleHeight));
                _contentScrollGroup.ScrollPosY = indexScrollPos;
                CalculateVisibleElements();
                RebindDrawers();
            }
            public void ScrollToValue(TData item) {
                int indexOfItem = _sourceList.IndexOf(item);
                if(indexOfItem != -1) {
                    ScrollTo(indexOfItem);
                }
            }
            public void ScrollToAnim(int index) {
                var indexScrollPos = Mathf.Clamp01(index * _elementHeightWithSpace / (_totalElementsHeight - _visibleHeight));
                _animator.value = _contentScrollGroup.ScrollPosY;
                _animator.target = indexScrollPos;

                _state = State.ScrollingToIndex;
            }
            public void ScrollToValueAnim(TData item) {
                int indexOfItem = _sourceList.IndexOf(item);
                if(indexOfItem != -1) {
                    ScrollToAnim(indexOfItem);
                }
            }
            
            /* Basic IList operations */
            public void Add(TData element) {
                _sourceList.Add(element);
                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();
            }
            public void Insert(int index, TData element) {
                _sourceList.Insert(index, element);
                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();
            }
            public void RemoveAt(int index) {
                _sourceList.RemoveAt(index);
                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();
            }
            public void Clear() {
                _sourceList.Clear();
                _selectedIndices.Clear();
                CalculateTotalHeight();
                CalculateVisibleElements();
                RebindDrawers();
            }
        }
    }
}