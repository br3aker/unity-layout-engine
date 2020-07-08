using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace SoftKata.UnityEditor.Controls {
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
}