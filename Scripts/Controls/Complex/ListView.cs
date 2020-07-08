using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace SoftKata.UnityEditor.Controls {
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