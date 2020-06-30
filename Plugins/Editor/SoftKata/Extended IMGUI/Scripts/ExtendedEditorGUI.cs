using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    public static partial class ExtendedEditor {
        public static IRepaintable CurrentView {get; internal set;}
    }
}