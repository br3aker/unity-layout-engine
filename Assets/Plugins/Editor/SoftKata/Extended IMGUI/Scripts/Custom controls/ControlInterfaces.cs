using UnityEngine;


namespace SoftKata.UnityEditor.Controls {
    public interface IDrawableElement {
        void OnGUI();
    }
    
    public interface IAbsoluteDrawableElement {
        void OnGUI(Rect position);
    }
}
