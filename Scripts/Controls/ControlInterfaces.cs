using UnityEngine;


namespace SoftKata.UnityEditor.Controls {
    public interface IDrawableElement {
        void OnGUI();
    }
    
    public interface IAbsoluteDrawableElement {
        void OnGUI(Rect position);
    }

    public interface IListBindable<TData> {
        void Bind(TData data, int index, bool selected);
    }
}
