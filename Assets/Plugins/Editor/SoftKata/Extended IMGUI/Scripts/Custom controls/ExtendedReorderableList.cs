using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public class ExtendedReorderableList : ReorderableList {
        public delegate void OnAddElementCallbackDelegate(int addedElementIndex, Object addedElement);

        public delegate void OnDeleteAllElementsCallbackDelegate();

        public delegate void OnDeleteElementCallbackDelegate(int deletedElementIndex);

        private readonly GUIContent _deleteAllContextContent = new GUIContent("Delete all");

        // GUI content
        private readonly GUIContent _deleteSelectedContextContent = new GUIContent("Delete selected");
        private readonly GUIContent _deselectAllContextContent = new GUIContent("Deselect all");
        private readonly GUIContent _lockUnlockContextContent = new GUIContent();

        // Selection
        private readonly List<bool> _selectedElements;

        protected readonly int ExtendedReorderableListControlIdHint = nameof(ExtendedReorderableList).GetHashCode();
        private readonly float _layoutRemovalOffsetY = 7f;
        private int _shiftSelectionAnchor = -1;

        protected int ControlId;
        protected Event CurrentEvent;
        protected bool DeleteAllFlag;
        protected bool DeleteSelectedFlag;

        // Deletion
        protected int DeleteSingleElementIndex = -1;
        protected System.Predicate<Object> DragAndDropValidator;

        // Drag and Drop
        protected bool EnableDragAndDrop;
        protected EventType EventType;
        public OnAddElementCallbackDelegate OnAddElementCallback;
        public OnDeleteAllElementsCallbackDelegate OnDeleteAllElementsCallback;
        public OnDeleteElementCallbackDelegate OnDeleteElementCallback;

        // Callbacks
        protected DrawElementCallbackDelegate OnDrawElementCallback;
        protected OnProcessEventDelegate OnProcessEventDelegateCallback;

        public ExtendedReorderableList(SerializedObject serializedObject, SerializedProperty elements, bool draggable,
            bool displayHeader, bool displayAddButton, bool displayRemoveButton)
            : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton) {
            drawElementCallback = DrawElement;
            onReorderCallbackWithDetails = (reorderableList, oldIndex, newIndex) => {
                _selectedElements[oldIndex] = false;
                _selectedElements[newIndex] = true;
            };

            _selectedElements = new List<bool>(new bool[elements.arraySize]);
        }

        public void DrawExtendedList(Rect rect) {
            ProcessEvents(rect);
            DoList(rect);
            DoDragAndDrop(rect);

            HandleDeletion();
        }

        private void ProcessEvents(Rect rect) {
            ControlId = GUIUtility.GetControlID(ExtendedReorderableListControlIdHint, FocusType.Passive, rect);

            CurrentEvent = Event.current;
            EventType = CurrentEvent.GetTypeForControl(ControlId);
            switch (EventType) {
                case EventType.MouseDown:
                    if (rect.Contains(CurrentEvent.mousePosition)) {
                        if (CurrentEvent.button == 0) {
                            var clickedIndex = GetRowIndexByPosition(rect, CurrentEvent.mousePosition);

                            if (clickedIndex > -1)
                                switch (CurrentEvent.modifiers) {
                                    case EventModifiers.Control:
                                        _selectedElements[clickedIndex] = true;
                                        break;
                                    case EventModifiers.Shift:
                                        if (_shiftSelectionAnchor > -1) {
                                            _selectedElements.ResetToDefaults();
                                            _selectedElements.SetRange(_shiftSelectionAnchor, clickedIndex, true);
                                        }

                                        break;
                                    default:
                                        _shiftSelectionAnchor = clickedIndex;
                                        _selectedElements.ResetToDefaults();
                                        _selectedElements[clickedIndex] = true;
                                        break;
                                }
                        }
                        else if (CurrentEvent.button == 1) {
                            CreateContextMenu().ShowAsContext();
                        }
                    }

                    break;
                case EventType.KeyDown:
                    var currentKeycode = CurrentEvent.keyCode;
                    if (currentKeycode == KeyCode.Backspace || currentKeycode == KeyCode.Delete)
                        DeleteSelectedFlag = true;

                    break;
            }

            OnProcessEventDelegateCallback?.Invoke();
        }

        private void DoDragAndDrop(Rect rect) {
            if (!EnableDragAndDrop) return;

            switch (EventType) {
                case EventType.DragUpdated:
                    if (rect.Contains(CurrentEvent.mousePosition)) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        DragAndDrop.activeControlID = ControlId;
                        //CurrentEvent.Use();
                    }

                    break;
                case EventType.DragPerform:
                    if (rect.Contains(CurrentEvent.mousePosition)) {
                        foreach (var obj in DragAndDrop.objectReferences) {
                            if (!DragAndDropValidator(obj)) continue;
                            AddNewElement(obj as ScriptableObject);
                        }

                        CurrentEvent.Use();
                    }

                    break;
            }
        }

        private int GetRowIndexByPosition(Rect listRect, Vector2 pos) {
            var headerMark =
                listRect.y + headerHeight + _layoutRemovalOffsetY;

            if (pos.y < headerMark) return -1;

            var elemHeight = elementHeight;
            var curCheckedHeight = headerMark + elemHeight;
            for (var i = 0; i < _selectedElements.Count; i++) {
                if (pos.y <= curCheckedHeight) return i;
                curCheckedHeight += elemHeight;
            }

            return -1;
        }

        public void AddNewElement(ScriptableObject elem) {
            serializedProperty.arraySize++;
            index = serializedProperty.arraySize - 1;

            _selectedElements.Add(default);

            OnAddElementCallback?.Invoke(index, elem);
        }

        private void DrawElement(Rect rect, int elemIndex, bool isActive, bool isFocused) {
            OnDrawElementCallback?.Invoke(rect, elemIndex, _selectedElements[elemIndex]);
        }

        private GenericMenu CreateContextMenu() {
            var menu = new GenericMenu();

            var anySelected = _selectedElements.Any(flag => flag);

            // Delete selected
            if (anySelected)
                menu.AddItem(_deleteSelectedContextContent, false, () => DeleteSelectedFlag = true);
            else
                menu.AddDisabledItem(_deleteSelectedContextContent, false);

            // Delete all
            menu.AddItem(_deleteAllContextContent, false, () => DeleteAllFlag = true);

            // Deselect all
            menu.AddSeparator("");
            if (anySelected)
                menu.AddItem(_deselectAllContextContent, false, DeselectAll);
            else
                menu.AddDisabledItem(_deselectAllContextContent, false);

            // Lock/Unlock (draggable)
            menu.AddSeparator("");
            _lockUnlockContextContent.text = draggable ? "Lock" : "Unlock";
            menu.AddItem(_lockUnlockContextContent, false, () => draggable = !draggable);

            return menu;
        }

        protected void DeselectAll() {
            _selectedElements.ResetToDefaults();
        }

        protected delegate void DrawElementCallbackDelegate(Rect rect, int elemIndex, bool isActive);

        protected delegate void OnProcessEventDelegate();

        #region Deletion

        private void HandleDeletion() {
            if (DeleteAllFlag) {
                DeleteAllElements();
                DeleteAllFlag = false;
                return;
            }

            if (DeleteSelectedFlag) {
                DeleteSelectedElements();
                DeleteSelectedFlag = false;
                return;
            }

            if (DeleteSingleElementIndex > -1) {
                DeleteElement(DeleteSingleElementIndex);
                DeleteSingleElementIndex = -1;
            }
        }

        private void DeleteElement(int deleteIndex) {
            serializedProperty.DeleteArrayElementAtIndex(deleteIndex);
            _selectedElements.RemoveAt(deleteIndex);
            index = -1;

            OnDeleteElementCallback?.Invoke(deleteIndex);
        }

        private void DeleteAllElements() {
            serializedProperty.ClearArray();
            _selectedElements.Clear();
            index = -1;

            OnDeleteAllElementsCallback?.Invoke();
        }

        private void DeleteSelectedElements() {
            for (var i = _selectedElements.Count - 1; i >= 0; i--)
                if (_selectedElements[i])
                    DeleteElement(i);

            DeselectAll();
        }

        #endregion
    }
}