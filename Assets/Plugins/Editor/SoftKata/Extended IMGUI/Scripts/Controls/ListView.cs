using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace SoftKata.UnityEditor.Controls {
    public abstract class ListViewBase<TData, TDrawer> : IDrawableElement where TDrawer : IAbsoluteDrawableElement, new() {
        // Generated with "ListViewControl" string with .net GetHashCode method
        private const int ListViewControlIdHint = 124860903;

        // Constants
        private const float EmptyListIconSize = 56;
        private const string EmptyListLabel = "Empty list";


        // Layout
        public readonly ScrollGroup Root;

        // Behaviour
        private int _currentControlId;
        private bool _isReordering;
        private readonly UnityAction _currentViewRepaint = ExtendedEditor.CurrentView.Repaint;

        // Data source indexers
        public abstract int Count {
            get;
        }
        public abstract TData this[int index] {
            get;
        }

        // Rendering
        private readonly List<IAbsoluteDrawableElement> _drawers = new List<IAbsoluteDrawableElement>();
        private readonly DataDrawerBinder _bindDataToDrawer;

        private readonly float _elementHeight;
        private readonly float _spaceBetweenElements;
        private readonly float _elementHeightWithSpace;
        private readonly float _visibleHeight;
        private float _totalElementsHeight;
        private float _visibleContentOffset;
        private int _firstVisibleIndex;
        private int _visibleElementsCount;

        public float ReorderingTintAlpha {
            set => _reorderableElementTint.a = value;
        }
        private Color _reorderableElementTint = Color.white;

        // Element selection
        private int _activeDrawerIndex;
        private float _activeDrawerPosY;
        private int _activeDataIndex = -1;
        private int _activeDataOriginalIndex;
        private readonly HashSet<int> _selectedIndices = new HashSet<int>();

        private double _lastClickTime;
        private const double DoubleClickTimingWindow = 0.25; // 1/4 second time window

        public bool DeselectOnGapClick = false;

        // Drag & drop
        private DragAndDropVisualMode _dragOperationType;
        public Func<DragAndDropVisualMode> ValidateDragData;
        
        // Public events
        public event DrawerActionCallback OnElementSelected;
        public event DrawerActionCallback OnElementDeselected;
        public event DrawerActionCallback OnElementDoubleClick;

        public event Action<int, int> OnElementsReorder;

        public delegate void DataDrawerBinder(int dataIndex, TData data, IAbsoluteDrawableElement drawer, bool isSelected);
        public delegate void DrawerActionCallback(int dataIndex, TData data, IAbsoluteDrawableElement drawer);

        // Empty list default texture & label
        private readonly Texture _emptyListIcon;
        private readonly GUIStyle _labelStyle = ExtendedEditor.Resources.CenteredGreyHeader;
        private readonly GUIContent _emptyListLabel = new GUIContent(EmptyListLabel);
        private readonly float _emptyListLabelHeight;


        // ctor
        public ListViewBase(Vector2 container, float elementHeight, DataDrawerBinder dataBinder) {
            _bindDataToDrawer = dataBinder;

            _emptyListLabelHeight = _labelStyle.GetContentHeight(_emptyListLabel);
            _emptyListIcon = ExtendedEditor.Resources.ListView.EmptyIcon;

            Root = new ScrollGroup(container, false);

            _elementHeight = elementHeight;
            _spaceBetweenElements = Root.SpaceBetweenEntries;
            _elementHeightWithSpace = _elementHeight + _spaceBetweenElements;
            _visibleHeight = container.y;

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

        // Core
        public void OnGUI() {
            var preScrollPos = Root.VerticalScroll;
            if (Layout.BeginLayoutGroup(Root)) {
                if(Count != 0) {
                    DoContent();
                }
                else {
                    DoEmptyContent();
                }
                Layout.EndLayoutGroup();
            }

            // if scroll pos changed => recalculate visible elements & rebind drawers if needed
            if(!Mathf.Approximately(preScrollPos, Root.VerticalScroll) && Event.current.type != EventType.Layout) {
                RebindDrawers();
            }
        }
        private void DoContent() {
            var eventType = Event.current.type;

            // register full array of elements
            if (eventType == EventType.Layout) {
                Root.RegisterEntriesArray(_elementHeight, Count);
                return;
            }

            // skip invisible elements
            if(_firstVisibleIndex > 0) {
                var totalSkipHeight = _firstVisibleIndex * _elementHeightWithSpace - _spaceBetweenElements;
                Root.GetRect(totalSkipHeight);
            }

            if(_isReordering) {
                DoReorderingContent();
                HandleReorderingEvents();
            }
            else {
                DoVisibleContent();
                HandleDefaultEvents();
            }
        }
        private void DoVisibleContent() {
            for (int i = 0; i < _visibleElementsCount; i++) {
                _drawers[i].OnGUI(Root.GetRect(_elementHeight));
            }
        }
        private void DoReorderingContent() {
            var reorderableDrawerIndex = _activeDrawerIndex;
            // drawing before held element
            for (int i = 0; i < reorderableDrawerIndex; i++) {
                _drawers[i].OnGUI(Root.GetRect(_elementHeight));
            }

            // Requesting held element space
            var initialHeldRect = Root.GetRect(_elementHeight);

            // drawing after held element
            for (int i = reorderableDrawerIndex + 1; i < _visibleElementsCount; i++) {
                _drawers[i].OnGUI(Root.GetRect(_elementHeight));
            }

            var color = GUI.color;
            GUI.color *= _reorderableElementTint;
            _drawers[reorderableDrawerIndex].OnGUI(
                new Rect(new Vector2(initialHeldRect.x, _activeDrawerPosY - _visibleContentOffset), 
                initialHeldRect.size)
            );
            GUI.color = color;
        }
        private void DoEmptyContent() {
            if(Root.GetRect(EmptyListIconSize, out var iconRect)) {
                iconRect.x += (iconRect.width / 2) - EmptyListIconSize / 2;
                iconRect.width = EmptyListIconSize;

                GUI.DrawTexture(iconRect, _emptyListIcon);
            }
            if(Root.GetRect(_emptyListLabelHeight, out var labelRect)) {
                global::UnityEditor.EditorGUI.LabelField(labelRect, _emptyListLabel, _labelStyle);
            }

            HandleDefaultEvents();
        }

        // Event handling
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
        private void HandleDefaultEvents() {
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
        private void HandleReorderingEvents() {
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
        private void HandleMouseDown(Event evt) {
            // context click
            if(evt.button == 1) {
                HandleMouseContextClick();
            }
            // selection click
            else {
                if(PositionToDataIndex(evt.mousePosition.y, out int clickIndex)) {
                    GUIUtility.hotControl = _currentControlId;
                    _activeDrawerPosY = clickIndex * _elementHeightWithSpace;
                    MouseSelectIndex(clickIndex, GetDrawerIndexFromDataIndex(clickIndex));
                    _activeDataOriginalIndex = clickIndex;
                }
                else if(DeselectOnGapClick) {
                    DeselectEverything();
                }
            }
            evt.Use();
        }           
        private void HandleMouseUp(Event evt) {
            if(GUIUtility.hotControl == _currentControlId) {
                _isReordering = false;
                GUIUtility.hotControl = 0;
                evt.Use();

                if(_activeDataOriginalIndex != _activeDataIndex) {
                    MoveElement(_activeDataOriginalIndex, _activeDataIndex);
                    OnElementsReorder?.Invoke(_activeDataOriginalIndex, _activeDataIndex);
                    _selectedIndices.Remove(_activeDataOriginalIndex);
                    _selectedIndices.Add(_activeDataIndex);
                }
            }
        }
        private void HandleMouseDrag(Event evt) {
            var draggableStartPos = _activeDataIndex * _elementHeightWithSpace;
            var movementDelta = evt.delta.y;
            _activeDrawerPosY += movementDelta;

            // going up
            if(movementDelta < 0 && _activeDataIndex > 0) {
                var reorderBoundary = (_activeDataIndex - 1) * _elementHeightWithSpace + _elementHeight / 2;
                if(_activeDrawerPosY <= reorderBoundary && _activeDrawerIndex > 0) {
                    _activeDataIndex -= 1;
                    SwapDrawers(_activeDrawerIndex, --_activeDrawerIndex);
                }
            }
            // doing down
            else if(_activeDataIndex < Count - 1) {
                var reorderBoundary = (_activeDataIndex + 1) * _elementHeightWithSpace - _elementHeight / 2;
                if(_activeDrawerPosY >= reorderBoundary && _activeDrawerIndex < _drawers.Count - 1) {
                    _activeDataIndex += 1;
                    SwapDrawers(_activeDrawerIndex, ++_activeDrawerIndex);
                }
            }

            _currentViewRepaint();
        }
        private void HandleMouseContextClick() {
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
        private void HandleDragUpdated(Event evt) {
            if(_dragOperationType == DragAndDropVisualMode.None) {
                _dragOperationType = ValidateDragData();
            }
            else {
                DragAndDrop.visualMode = _dragOperationType;
            }
            evt.Use();
        }
        private void HandleDragPerform(Event evt) {
            AcceptDragData();
            RebindDrawers();

            evt.Use();
        }
        private void HandleDragExited(Event evt) {
            _dragOperationType = DragAndDropVisualMode.None;
            evt.Use();
        }

        // Rendering utility
        private void CalculateVisibleData(out int firstVisibleIndex, out int visibleCount) {
            _totalElementsHeight = Count * _elementHeightWithSpace - _spaceBetweenElements;

            if (_totalElementsHeight <= _visibleHeight) {           
                _visibleContentOffset = 0;
                firstVisibleIndex = 0;
                visibleCount = Count;
            }
            else {
                _visibleContentOffset = (_totalElementsHeight - _visibleHeight) * Root.VerticalScroll;

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

        // Element selection
        private void MouseSelectIndex(int dataIndex, int drawerIndex) {
            var currentTime = EditorApplication.timeSinceStartup;
            if(dataIndex == _activeDataIndex && currentTime - _lastClickTime <= DoubleClickTimingWindow) {
                OnElementDoubleClick?.Invoke(dataIndex, this[dataIndex], _drawers[GetDrawerIndexFromDataIndex(dataIndex)]);
            }
            else {
                switch(Event.current.modifiers) {
                    case EventModifiers.None:
                        GreedySelection(dataIndex, drawerIndex);
                        break; 
                    case EventModifiers.Shift:
                        ShiftSelection(dataIndex);
                        break; 
                    case EventModifiers.Control:
                        ControlSelection(dataIndex, drawerIndex);
                        break; 
                }
                _activeDataIndex = dataIndex;
                _activeDrawerIndex = drawerIndex;
            }
            _lastClickTime = currentTime;
        }
        private void GreedySelection(int index, int drawerIndex) {
            _isReordering = true;
            if(index == _activeDataIndex) return;

            DeselectEverything();
            OnElementSelected?.Invoke(index, this[index], _drawers[drawerIndex]);
            _selectedIndices.Add(index);
        }
        private void ShiftSelection(int index) {
            // Math behind this indices
            // First, we assume that [start] is lesser than [end] => [start..end]
            // We don't need to add _activeDataIndex index to selected indices set
            // so we do +1
            // If our assumption about start/end relationship is false
            // then we do -1 to compensate +1 we added previously
            // and also we do -1 because we don't need to add _activeDataIndex
            var start = _activeDataIndex + 1;
            var end = index;
            if(start > end) {
                start -= 2;
                
                // inlined Swap method
                var tmp = start;
                start = end;
                end = tmp;
            }
            for(int i = start; i <= end; i++) {
                _selectedIndices.Add(i);
            }
            if(OnElementSelected == null) return;
            for(int i = start; i <= end; i++) {
                OnElementSelected.Invoke(i, this[i], GetDataDrawer(i));
            }
        }
        private void ControlSelection(int index, int drawerIndex) {
            var activeDrawer = _drawers[drawerIndex];
            if(_selectedIndices.Contains(index)) {
                OnElementDeselected?.Invoke(index, this[index], activeDrawer);
                _selectedIndices.Remove(index);
            }
            else {
                OnElementSelected?.Invoke(index, this[index], activeDrawer);
                _selectedIndices.Add(index);
            }
        }
        private void DeselectEverything() {
            if(_activeDataIndex == -1) return;

            if(OnElementDeselected != null) {
                foreach(var index in _selectedIndices) {
                    OnElementDeselected(index, this[index], GetDataDrawer(index));
                }
            }
            _selectedIndices.Clear();
            _activeDataIndex = -1;
        }

        // Utility methods
        private void RemoveSelected() {
            RemoveSelectedIndices(_selectedIndices.OrderByDescending(i => i));

            _activeDataIndex = -1;
            _selectedIndices.Clear();

            RebindAllDrawers();
        }
        private bool PositionToDataIndex(float position, out int index) {
            var absolutePosition = position + _visibleContentOffset;
            index = (int)(absolutePosition / _elementHeightWithSpace);

            float indexElementBottomBorder = index * _elementHeightWithSpace + _elementHeight;
            return indexElementBottomBorder > absolutePosition;
        }
        private void SwapDrawers(int firstIndex, int secondIndex) {
            var tmp = _drawers[firstIndex];
            _drawers[firstIndex] = _drawers[secondIndex];
            _drawers[secondIndex] = tmp;
        }

        // Implementation dependent methods
        protected abstract void ClearDataArray();
        protected abstract void MoveElement(int from, int to);
        protected abstract void AcceptDragData();
        protected abstract void RemoveSelectedIndices(IOrderedEnumerable<int> indices);

        // Public interface
        public void Clear() {
            ClearDataArray();
            _selectedIndices.Clear();
            _activeDataIndex = -1;
        }
        public void GoTo(int index) {
            var indexScrollPos = Mathf.Clamp01(index * _elementHeightWithSpace / (_totalElementsHeight - _visibleHeight));
            Root.VerticalScroll = indexScrollPos;
            RebindDrawers();
        }
    }
    
    public class SerializedListView<TDrawer> : ListViewBase<SerializedProperty, TDrawer> where TDrawer : IAbsoluteDrawableElement, new() {
        // Data source
        public SerializedObject _serializedObject;
        public SerializedProperty _serializedArray;

        // Data source indexers
        public override int Count => _serializedArray.arraySize;
        public override SerializedProperty this[int index] => _serializedArray.GetArrayElementAtIndex(index);

        // Public delegates
        public Action<SerializedProperty> AddDragDataToArray;

        // ctor
        public SerializedListView(SerializedProperty source, Vector2 container, float elementHeight, DataDrawerBinder bind) : base(container, elementHeight, bind) {
            _serializedObject = source.serializedObject;
            _serializedArray = source;

            RebindAllDrawers();
        }
        public SerializedListView(SerializedProperty source, float height, float elementHeight, DataDrawerBinder bind)
            : this(source, new Vector2(Layout.FlexibleWidth, height), elementHeight, bind) { }

        // Implementation dependent overrides
        protected override void ClearDataArray() {
            _serializedArray.ClearArray();
            _serializedObject.ApplyModifiedProperties();
        }
        protected override void MoveElement(int srcIndex, int dstIndex) {
            _serializedArray.MoveArrayElement(srcIndex, dstIndex);
            _serializedObject.ApplyModifiedProperties();
        }
        protected override void AcceptDragData() {
            AddDragDataToArray(_serializedArray);
            _serializedObject.ApplyModifiedProperties();
        }
        protected override void RemoveSelectedIndices(IOrderedEnumerable<int> indices) {
            foreach(var index in indices) {
                _serializedArray.DeleteArrayElementAtIndex(index);
            }
            _serializedObject.ApplyModifiedProperties();
        }
    }
    
    public class ListView<TData, TDrawer> : ListViewBase<TData, TDrawer> where TDrawer : IAbsoluteDrawableElement, new() {
        // Data source
        private readonly IList<TData> _sourceList;

        // Data source indexers
        public override int Count => _sourceList.Count;
        public override TData this[int index] => _sourceList[index];

        // Public delegates
        public Action<IList<TData>> AddDragDataToArray;

        // ctor
        public ListView(IList<TData> source, Vector2 container, float elementHeight, DataDrawerBinder bind) : base(container, elementHeight, bind) {
            _sourceList = source;

            RebindAllDrawers();
        }
        public ListView(IList<TData> source, float height, float elementHeight, DataDrawerBinder bind)
            : this(source, new Vector2(Layout.FlexibleWidth, height), elementHeight, bind) { }

        // Implementation dependent overrides
        protected override void ClearDataArray() {
            _sourceList.Clear();
        }
        protected override void MoveElement(int srcIndex, int dstIndex) {
            TData item = _sourceList[srcIndex];
            _sourceList.RemoveAt(srcIndex);
            _sourceList.Insert(dstIndex, item);
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