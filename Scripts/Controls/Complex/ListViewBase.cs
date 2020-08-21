using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace SoftKata.UnityEditor.Controls {
    public abstract class ListViewBase<TData, TDrawer> : IDrawableElement 
        where TDrawer : class, IAbsoluteDrawableElement, IListBindable<TData>, new()
    {
        // Generated with "ListViewControl" string with .net GetHashCode method
        private const int ListViewControlIdHint = 124860903;

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
        private readonly List<TDrawer> _drawers = new List<TDrawer>();

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

        public delegate void DrawerActionCallback(int dataIndex, TData data, IAbsoluteDrawableElement drawer);

        // Empty list default texture & label
        private const float EmptyListIconSize = 56;
        private const string EmptyListLabel = "List is empty";
        private readonly Texture _emptyListIcon = Resources.ListEmptyIcon;
        private readonly GUIStyle _labelStyle = Resources.CenteredGreyHeader;
        private readonly GUIContent _emptyListLabel = new GUIContent(EmptyListLabel);
        private readonly float _emptyListLabelHeight;


        // ctor
        protected ListViewBase(Vector2 container, float elementHeight, GUIStyle containerStyle, GUIStyle thumbStyle) {
            _emptyListLabelHeight = _labelStyle.GetContentHeight(_emptyListLabel);

            Root = new ScrollGroup(container, false, containerStyle, thumbStyle);

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
        protected ListViewBase(Vector2 container, float elementHeight) 
            : this(container, elementHeight, Resources.ScrollGroup, Resources.ScrollGroupThumb) {}
        protected ListViewBase(float height, float elementHeight, GUIStyle containerStyle, GUIStyle thumbStyle)
            : this(new Vector2(-1, height), elementHeight, containerStyle, thumbStyle){}
        protected ListViewBase(float height, float elementHeight)
            : this(new Vector2(-1, height), elementHeight) { }

        // Core
        public void OnGUI() {
            var preScrollPos = Root.VerticalScroll;
            if (Layout.BeginLayoutScope(Root)) {
                if(Count != 0) {
                    DoContent();
                }
                else {
                    DoEmptyContent();
                }
                Layout.EndCurrentScope();
            }

            // if scroll pos changed => recalculate visible elements & rebind drawers if needed
            if(!Mathf.Approximately(preScrollPos, Root.VerticalScroll) && Event.current.type != EventType.Layout) {
                RebindInvalidatedDrawers();
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
        private void HandleDefaultEvents() {
            _currentControlId = GUIUtility.GetControlID(ListViewControlIdHint, FocusType.Passive);
            var evt = Event.current;
            var type = evt.GetTypeForControl(_currentControlId);
            switch (type) {
                // Selection, reordering & context click
                case EventType.MouseDown:
                    HandleMouseDown(evt);
                    break;
                // Drag and drop
                case EventType.DragUpdated:
                    if(ValidateDragData != null) HandleDragUpdated(evt);
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
            _currentControlId = GUIUtility.GetControlID(ListViewControlIdHint, FocusType.Passive);
            var evt = Event.current;
            var type = evt.GetTypeForControl(_currentControlId);
            switch (type) {
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
            RebindAllDrawers();
            Root.MarkLayoutDirty();
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
        protected void RebindInvalidatedDrawers() {
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
                newFirstDrawer.Bind(this[newIndex], newIndex, _selectedIndices.Contains(newIndex));

                lastBindedDrawerIndex += 1;
            }

            int totalCountDiff = newCount - initialCount;
            for(int i = lastBindedDrawerIndex + 1; i < newCount; i++) {
                var dataIndex = newIndex + i;
                _drawers[i].Bind(this[dataIndex], dataIndex, _selectedIndices.Contains(dataIndex));
            }

            
            _firstVisibleIndex = newIndex;
            _visibleElementsCount = newCount;
        }
        protected void RebindAllDrawers() {
            CalculateVisibleData(out _firstVisibleIndex, out _visibleElementsCount);
            int dataFirstVisibleIndex = _firstVisibleIndex;
            for(int i = 0; i < _visibleElementsCount; i++) {
                var dataIndex = dataFirstVisibleIndex + i;
                _drawers[i].Bind(this[dataIndex], dataIndex, _selectedIndices.Contains(dataIndex));
            }
        }
        
        private int GetDrawerIndexFromDataIndex(int index) {
            var shiftedIndex = index - _firstVisibleIndex;
            var drawerIndexInVisibleRange = shiftedIndex >= 0 && shiftedIndex < _visibleElementsCount;
            return drawerIndexInVisibleRange ? shiftedIndex : -1;
        }
        private TDrawer GetDataDrawer(int index) {
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

            Root.MarkLayoutDirty();
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
            Root.MarkLayoutDirty();
        }
        public void GoTo(int index) {
            var indexScrollPos = Mathf.Clamp01(index * _elementHeightWithSpace / (_totalElementsHeight - _visibleHeight));
            Root.VerticalScroll = indexScrollPos;
            RebindInvalidatedDrawers();
        }
    }
}