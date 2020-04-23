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
        public interface IDrawableElement {
            void OnGUI();
        }
        public interface IAbsoluteDrawableElement {
            void OnGUI(Rect position);
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

        public abstract class ListViewBase<TData, TDrawer> : IDrawableElement where TDrawer : IAbsoluteDrawableElement, new() {
            // Hash for control id generation
            private int ListViewControlIdHint = "ListView".GetHashCode();

            public delegate void DataDrawerBinder(int dataIndex, TData data, IAbsoluteDrawableElement drawer, bool isSelected);
            public delegate void DrawerActionCallback(int dataIndex, TData data, IAbsoluteDrawableElement drawer);

            /* Source */
            public abstract int Count {
                get;
            }
            public abstract TData this[int index] {
                get;
            }

            /* State for FSM */
            private enum State {
                Default,
                Reordering,
                ScrollingToIndex
            }
            private State _state;

            /* Control id */
            private int _currentControlId;

            /* Layout scroll group */
            private readonly ScrollGroup _contentScrollGroup;

            /* Drawers */
            private readonly List<IAbsoluteDrawableElement> _drawers = new List<IAbsoluteDrawableElement>();
            private readonly DataDrawerBinder _bindDataToDrawer;

            /* Animated scrolling */
            private readonly AnimFloat _animator = new AnimFloat(0f, CurrentViewRepaint);

            /* Element selection */
            public bool DeselectOnGapClick = false;
            private int _activeDataIndex = -1;
            private readonly HashSet<int> _selectedIndices = new HashSet<int>();
            // Selection & Deselection callbacks
            public event DrawerActionCallback OnElementSelected;
            public event DrawerActionCallback OnElementDeselected;
            // Double click callback
            public event DrawerActionCallback OnElementDoubleClick;
            private double _lastClickTime;
            private const double DoubleClickTimingWindow = 0.25; // 1/4 second time window

            /* Drag & drop */
            private bool _isDragDataValidated;
            private DragAndDropVisualMode _dragOperationType;
            public Func<DragAndDropVisualMode> ValidateDragData;

            /* Reordering */
            public event Action<int, int> OnElementsReorder;
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


            /* Constructors */
            public ListViewBase(Vector2 container, float elementHeight, DataDrawerBinder bind) {
                _bindDataToDrawer = bind;

                _labelStyle = ElementsResources.CenteredGreyHeader;
    
                EmptyListLabel = new GUIContent("This list is empty");
                _emptyListIcon = ElementsResources.ListView.EmptyIcon;

                _contentScrollGroup = new ScrollGroup(container, Vector2.zero, false);

                _elementHeight = elementHeight;
                _spaceBetweenElements = _contentScrollGroup.SpaceBetweenEntries;
                _elementHeightWithSpace = _elementHeight + _spaceBetweenElements;
                // TODO: this must use "pure" visible container size without TotalOffset.verical which is calculated dynamically
                _visibleHeight = container.y;// - _contentScrollGroup.TotalOffset.vertical;

                var maxVisibleElements = Mathf.CeilToInt(_visibleHeight / _elementHeightWithSpace);
                var nextElementStart = maxVisibleElements * _elementHeightWithSpace;
                var heightToNextElement = nextElementStart - _visibleHeight;
                if(_elementHeight > heightToNextElement) {
                    maxVisibleElements += 1;
                }

                // creating new drawers
                for (int i = 0; i < maxVisibleElements; i++) {
                    _drawers.Add(new TDrawer());
                }
            }
            public ListViewBase(float height, float elementHeight, DataDrawerBinder bind)
                : this(new Vector2(-1, height), elementHeight, bind){}

            /* Core method for rendering */
            public void OnGUI() {
                EditorGUI.LabelField(Layout.GetRect(16), $"_activeIndex: {_activeDataIndex}");
                EditorGUI.LabelField(Layout.GetRect(16), $"_selectedIndices[{_selectedIndices.Count}]: {string.Join("|", _selectedIndices)}");

                var preScrollPos = _contentScrollGroup.ScrollPosY;
                if (Layout.BeginLayoutGroup(_contentScrollGroup)) {
                    if(Count != 0) {
                        DoContent();
                    }
                    else {
                        DoEmptyContent();
                    }
                    Layout.EndLayoutGroup();
                }

                // if scroll pos changed => recalculate visible elements & rebind drawers if needed
                if(!Mathf.Approximately(preScrollPos, _contentScrollGroup.ScrollPosY) && Event.current.type != EventType.Layout) {
                    RebindDrawers();
                }
            }

            /* Top-level GUI methods */
            private void DoContent() {
                var eventType = Event.current.type;

                // register full array of elements
                if (eventType == EventType.Layout) {
                    _contentScrollGroup.RegisterEntriesArray(_elementHeight, Count);
                    return;
                }

                // skip invisible elements
                if(_firstVisibleIndex > 0) {
                    var totalSkipHeight = _firstVisibleIndex * _elementHeightWithSpace - _spaceBetweenElements;
                    _contentScrollGroup.GetRect(totalSkipHeight);
                }

                switch(_state) {
                    case State.Default:
                        DoVisibleContent();
                        HandleEventsAtDefault();
                        break;
                    case State.Reordering:
                        DoReorderingContent();
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
                    _drawers[i].OnGUI(_contentScrollGroup.GetRect(_elementHeight));
                }
            }
            private void DoReorderingContent() {
                var reorderableDrawerIndex = _activeDrawerIndex;
                // drawing before held element
                for (int i = 0; i < reorderableDrawerIndex; i++) {
                    _drawers[i].OnGUI(_contentScrollGroup.GetRect(_elementHeight));
                }

                // Requesting held element space
                var initialHeldRect = _contentScrollGroup.GetRect(_elementHeight);

                // drawing after held element
                for (int i = reorderableDrawerIndex + 1; i < _visibleElementsCount; i++) {
                    _drawers[i].OnGUI(_contentScrollGroup.GetRect(_elementHeight));
                }

                var color = GUI.color;
                GUI.color *= _reorderableElementTint;
                _drawers[reorderableDrawerIndex].OnGUI(
                    new Rect(new Vector2(initialHeldRect.x, _activeElementPosY - _visibleContentOffset), 
                    initialHeldRect.size)
                );
                GUI.color = color;
            }
            private void DoEmptyContent() {
                if(_contentScrollGroup.GetRect(IconSize, out var iconRect)) {
                    iconRect.x += (iconRect.width / 2) - IconSize / 2;
                    iconRect.width = IconSize;

                    GUI.DrawTexture(iconRect, _emptyListIcon);
                }
                if(_contentScrollGroup.GetRect(_emptyListLabelHeight, out var labelRect)) {
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
                        if(ValidateDragData == null) {
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
                        _activeDataOriginalIndex = clickIndex;
                        _activeDrawerIndex = GetDrawerIndexFromDataIndex(clickIndex);
                    }
                    else if(DeselectOnGapClick) {
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

                    if(_activeDataOriginalIndex != _activeDataIndex) {
                        MoveElement(_activeDataOriginalIndex, _activeDataIndex);
                        OnElementsReorder?.Invoke(_activeDataOriginalIndex, _activeDataIndex);
                        // _selectedIndices.Remove(_activeDataOriginalIndex);
                        // _selectedIndices.Add(_activeDataIndex);
                    }
                }
            }
            
            private int _activeDataOriginalIndex;
            private int _activeDrawerIndex;
            private void HandleMouseDrag(Event evt) {
                var draggableStartPos = _activeDataIndex * _elementHeightWithSpace;
                var movementDelta = evt.delta.y;
                _activeElementPosY += movementDelta;

                // going up
                if(movementDelta < 0 && _activeDataIndex > 0) {
                    var reorderBoundary = (_activeDataIndex - 1) * _elementHeightWithSpace + _elementHeight / 2;
                    if(_activeElementPosY <= reorderBoundary && _activeDrawerIndex > 0) {
                        _activeDataIndex -= 1;
                        _drawers.SwapElementsInplace(_activeDrawerIndex, --_activeDrawerIndex);
                    }
                }
                // doing down
                else if(_activeDataIndex < Count - 1) {
                    var reorderBoundary = (_activeDataIndex + 1) * _elementHeightWithSpace - _elementHeight / 2;
                    if(_activeElementPosY >= reorderBoundary && _activeDrawerIndex < _drawers.Count) {
                        _activeDataIndex += 1;
                        _drawers.SwapElementsInplace(_activeDrawerIndex, ++_activeDrawerIndex);
                    }
                }

                CurrentViewRepaint();
            }
            protected abstract void MoveElement(int from, int to);
            private void HandleContextClick() {
                var menu = new GenericMenu();
                // Delete selected
                var deleteSelectedContent = new GUIContent("Delete selected");
                if(_activeDataIndex != -1) menu.AddItem(deleteSelectedContent, false, RemoveSelected);
                else menu.AddDisabledItem(deleteSelectedContent);
                // Delete all
                var deleteAllContent = new GUIContent("Delete all");
                if(Count != 0) menu.AddItem(deleteAllContent, false, Clear);
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
                AcceptDragData();
                RebindDrawers();

                _state = State.Default;
                evt.Use();
            }
            private void HandleDragExited(Event evt) {
                _state = State.Default;
                _isDragDataValidated = false;
                evt.Use();
            }
            protected abstract void AcceptDragData();

            /* Helpers */
            private bool PositionToDataIndex(float position, out int index) {
                var absolutePosition = position + _visibleContentOffset;
                index = (int)(absolutePosition / _elementHeightWithSpace);

                float indexElementBottomBorder = index * _elementHeightWithSpace + _elementHeight;
                return indexElementBottomBorder > absolutePosition;
            }

            /* Drawers */
            private void CalculateVisibleData(out int firstVisibleIndex, out int visibleCount) {
                _totalElementsHeight = Count * _elementHeightWithSpace - _spaceBetweenElements;

                if (_totalElementsHeight <= _visibleHeight) {           
                    _visibleContentOffset = 0;
                    firstVisibleIndex = 0;
                    visibleCount = Count;
                }
                else {
                    _visibleContentOffset = (_totalElementsHeight - _visibleHeight) * _contentScrollGroup.ScrollPosY;

                    if(!PositionToDataIndex(0, out firstVisibleIndex)) {
                        firstVisibleIndex += 1;
                    }

                    int lastIndex = (int)((_visibleHeight + _visibleContentOffset) / _elementHeightWithSpace);
                    visibleCount = lastIndex - firstVisibleIndex + 1;
                }
            }
            protected void RebindDrawers() {
                int initialIndex = _firstVisibleIndex;
                int initialCount = _visibleElementsCount;
                CalculateVisibleData(out int newIndex, out int newCount);

                // Rebinding if needed
                int lastBindedDrawerIndex = initialCount - 1;
                if(newIndex > initialIndex) {
                    var newFirstDrawer = _drawers[0];
                    _drawers.RemoveAt(0);
                    _drawers.Add(newFirstDrawer);
                }
                else if(newIndex < initialIndex) {
                    var lastDrawerIndex = _drawers.Count - 1;
                    var newFirstDrawer = _drawers[lastDrawerIndex];
                    _drawers.RemoveAt(lastDrawerIndex);
                    _drawers.Insert(0, newFirstDrawer);
                    _bindDataToDrawer(newIndex, this[newIndex], newFirstDrawer, _selectedIndices.Contains(newIndex));

                    lastBindedDrawerIndex += 1;
                }

                int totalCountDiff = newCount - initialCount;
                for(int i = lastBindedDrawerIndex + 1; i < newCount; i++) {
                    var dataIndex = newIndex + i;
                    _bindDataToDrawer(dataIndex, this[dataIndex], _drawers[i], _selectedIndices.Contains(dataIndex));
                }

                
                _firstVisibleIndex = newIndex;
                _visibleElementsCount = newCount;
            }
            protected void RebindAllDrawers() {
                CalculateVisibleData(out _firstVisibleIndex, out _visibleElementsCount);
                int dataFirstVisibleIndex = _firstVisibleIndex;
                for(int i = 0; i < _visibleElementsCount; i++) {
                    var dataIndex = dataFirstVisibleIndex + i;
                    _bindDataToDrawer(dataIndex, this[dataIndex], _drawers[i], _selectedIndices.Contains(dataIndex));
                }
            }
            
            private int GetDrawerIndexFromDataIndex(int index) {
                var shiftedIndex = index - _firstVisibleIndex;
                var drawerIndexInVisibleRange = shiftedIndex >= 0 && shiftedIndex < _visibleElementsCount;
                return drawerIndexInVisibleRange ? shiftedIndex : -1;
            }
            private IAbsoluteDrawableElement GetDataDrawer(int index) {
                var drawerIndex = GetDrawerIndexFromDataIndex(index);
                if(drawerIndex != -1) {
                    return _drawers[drawerIndex];
                }
                return null;
            }

            /* Mouse clicks */
            // Element selection
            private void MouseSelectIndex(int index) {
                var currentTime = EditorApplication.timeSinceStartup;
                if(currentTime - _lastClickTime <= DoubleClickTimingWindow && index == _activeDataIndex) {
                    OnElementDoubleClick?.Invoke(index, this[index], _drawers[GetDrawerIndexFromDataIndex(index)]);
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
                    _activeDataIndex = index;
                }
                _lastClickTime = currentTime;
            }
            private void GreedySelection(int index) {
                DeselectEverything();
                if(index != _activeDataIndex) {
                    OnElementSelected?.Invoke(index, this[index], GetDataDrawer(index));
                }
            }
            private void ShiftSelection(int index) {
                var start = _activeDataIndex + 1;
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
                        OnElementSelected.Invoke(i, this[i], GetDataDrawer(i));
                    }
                }
            }
            private void ControlSelection(int index) {
                if(_selectedIndices.Contains(index)) {
                    OnElementDeselected?.Invoke(index, this[index], GetDataDrawer(index));
                    _selectedIndices.Remove(index);
                }
                else {
                    OnElementSelected?.Invoke(index, this[index], GetDataDrawer(index));
                    _selectedIndices.Add(_activeDataIndex);
                }
            }
            private void DeselectEverything() {
                if(_activeDataIndex == -1) return;

                if(OnElementDeselected != null) {
                    foreach(var index in _selectedIndices) {
                        OnElementDeselected(index, this[index], GetDataDrawer(index));
                    }
                    OnElementDeselected(_activeDataIndex, this[_activeDataIndex], GetDataDrawer(_activeDataIndex));
                }
                _selectedIndices.Clear();
                _activeDataIndex = -1;
            }
            // Context menu
            private void RemoveSelected() {
                _selectedIndices.Add(_activeDataIndex);
                RemoveSelectedIndices(_selectedIndices.OrderByDescending(i => i));

                _activeDataIndex = -1;
                _selectedIndices.Clear();

                RebindAllDrawers();
            }
            protected abstract void RemoveSelectedIndices(IOrderedEnumerable<int> indices);

            /* Scroll to values */
            public void GoTo(int index) {
                var indexScrollPos = Mathf.Clamp01(index * _elementHeightWithSpace / (_totalElementsHeight - _visibleHeight));
                _contentScrollGroup.ScrollPosY = indexScrollPos;
                RebindDrawers();
            }
            public void ScrollTo(int index) {
                var indexScrollPos = Mathf.Clamp01(index * _elementHeightWithSpace / (_totalElementsHeight - _visibleHeight));
                _animator.value = _contentScrollGroup.ScrollPosY;
                _animator.target = indexScrollPos;

                _state = State.ScrollingToIndex;
            }

            /* Basic IList operations */
            public void Clear() {
                ClearUnderlyingArray();
                _selectedIndices.Clear();
                _activeDataIndex = -1;
            }
            protected abstract void ClearUnderlyingArray();
        }
        public class SerializedListView<TDrawer> : ListViewBase<SerializedProperty, TDrawer> where TDrawer : IAbsoluteDrawableElement, new() {
            /* Source list */
            public SerializedObject _serializedObject;
            public SerializedProperty _serializedArray;
            public override int Count => _serializedArray.arraySize;
            public override SerializedProperty this[int index] => _serializedArray.GetArrayElementAtIndex(index);

            /* Callbacks */
            public Action<SerializedProperty> AddDragDataToArray;

            /* Constructors */
            public SerializedListView(SerializedProperty source, Vector2 container, float elementHeight, DataDrawerBinder bind) : base(container, elementHeight, bind) {
                _serializedObject = source.serializedObject;
                _serializedArray = source;

                RebindDrawers();
            }
            public SerializedListView(SerializedProperty source, float height, float elementHeight, DataDrawerBinder bind)
                : this(source, new Vector2(-1, height), elementHeight, bind) { }

            /* Overrides */
            protected override void ClearUnderlyingArray() {
                _serializedArray.ClearArray();
                _serializedArray.serializedObject.ApplyModifiedProperties();
            }
            protected override void MoveElement(int from, int to) {
                _serializedArray.MoveArrayElement(from, to);
            }
            protected override void AcceptDragData() {
                AddDragDataToArray(_serializedArray);
            }
            protected override void RemoveSelectedIndices(IOrderedEnumerable<int> indices) {
                foreach(var index in indices) {
                    _serializedArray.DeleteArrayElementAtIndex(index);
                }
                _serializedArray.serializedObject.ApplyModifiedProperties();
            }
        }
        public class ListView<TData, TDrawer> : ListViewBase<TData, TDrawer> where TDrawer : IAbsoluteDrawableElement, new() {
            /* Source list */
            private readonly IList<TData> _sourceList;
            public override int Count => _sourceList.Count;
            public override TData this[int index] => _sourceList[index];

            /* Callbacks */
            public Action<IList<TData>> AddDragDataToArray;

            /* Constructors */
            public ListView(IList<TData> source, Vector2 container, float elementHeight, DataDrawerBinder bind) : base(container, elementHeight, bind) {
                _sourceList = source;

                RebindAllDrawers();
            }
            public ListView(IList<TData> source, float height, float elementHeight, DataDrawerBinder bind)
                : this(source, new Vector2(-1, height), elementHeight, bind) { }

            /* Overrides */
            protected override void ClearUnderlyingArray() {
                _sourceList.Clear();
            }
            protected override void MoveElement(int srcIndex, int dstIndex) {
                _sourceList.MoveElement(srcIndex, dstIndex);
            }
            protected override void AcceptDragData() {
                AddDragDataToArray(_sourceList);
            }
            protected override void RemoveSelectedIndices(IOrderedEnumerable<int> indices) {
                foreach(var index in indices) {
                    _sourceList.RemoveAt(index);
                }
            }
        }
    }
}