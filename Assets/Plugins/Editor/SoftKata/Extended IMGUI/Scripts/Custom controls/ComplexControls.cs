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
    }
}