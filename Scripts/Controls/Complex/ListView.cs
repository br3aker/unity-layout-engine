using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


namespace SoftKata.UnityEditor.Controls {
    public class ListView<TData, TDrawer> : ListViewBase<TData, TDrawer> 
        where TDrawer : class, IAbsoluteDrawableElement, IListBindable<TData>, new() 
    {
        // Data source
        private readonly IList<TData> _sourceList;

        // Data source indexers
        public override int Count => _sourceList.Count;
        public override TData this[int index] => _sourceList[index];

        // Public delegates
        public Action<IList<TData>> AddDragDataToArray;

        // ctor
        public ListView(IList<TData> source, Vector2 container, float elementHeight, GUIStyle containerStyle, GUIStyle thumbStyle) 
            : base(container, elementHeight, containerStyle, thumbStyle) 
        {
            _sourceList = source;
            RebindAllDrawers();
        }
        public ListView(IList<TData> source, float height, float elementHeight, GUIStyle containerStyle, GUIStyle thumbStyle)
            : this(source, new Vector2(Layout.FlexibleWidth, height), elementHeight, containerStyle, thumbStyle) { }
        public ListView(IList<TData> source, Vector2 container, float elementHeight)
            : base(container, elementHeight) {
            _sourceList = source;
            RebindAllDrawers();
        }
        public ListView(IList<TData> source, float height, float elementHeight)
            : this(source, new Vector2(Layout.FlexibleWidth, height), elementHeight) { }

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