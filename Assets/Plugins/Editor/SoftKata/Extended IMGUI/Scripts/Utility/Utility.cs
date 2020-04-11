using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public static class Utility {
        public static T LoadAssetAtPathAndAssert<T>(string assetPath) where T : UnityEngine.Object {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Couldn't load asset [{typeof(T).Name}] at path \"{assetPath}\"");
            return asset;
        }

        public static int GetContentHeight(this GUIStyle style, GUIContent content) {
            return (int)Mathf.Ceil(style.CalcSize(content).y);
        }

        public static Rect RectIntersection(Rect a, Rect b) {
            var x = Mathf.Max(a.x, b.x);
            var num2 = Mathf.Min(a.x + a.width, b.x + b.width);
            var y = Mathf.Max(a.y, b.y);
            var num4 = Mathf.Min(a.y + a.height, b.y + b.height);
            return new Rect(x, y, num2 - x, num4 - y);
        }

        public static void ResetToDefaults<T>(this IList<T> list) {
            for (var i = 0; i < list.Count; i++) list[i] = default;
        }

        public static void SetRange<T>(this IList<T> list, int startIndex, int endIndex, T value) {
            if (endIndex < startIndex) (startIndex, endIndex) = (endIndex, startIndex);

            for (var i = startIndex; i <= endIndex; i++) list[i] = value;
        }

        public static void SwapElementsInplace<T>(this IList<T> list, int firstIndex, int secondIndex) {
            T tmp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = tmp;
        }

        public static void Swap<T>(ref T a, ref T b) {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static void Accumulate(this RectOffset rectOffset, RectOffset source) {
            rectOffset.left += source.left;
            rectOffset.right += source.right;
            rectOffset.top += source.top;
            rectOffset.bottom += source.bottom;
        }

        [MenuItem("Utility/Recompile scripts")]
        public static void RequestRecompilation() {
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}