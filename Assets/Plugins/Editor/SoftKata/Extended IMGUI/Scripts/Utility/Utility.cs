using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    public static class Utility {
        public static T LoadAssetAtPathAndAssert<T>(string assetPath) where T : UnityEngine.Object {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Couldn't load asset [{typeof(T).Name}] at path \"{assetPath}\"");
            return asset;
        }

        public static int GetContentHeight(this GUIStyle style, GUIContent content) {
            return Mathf.CeilToInt(style.CalcSize(content).y);
        }

        public static void SwapElementsInplace<T>(this IList<T> list, int firstIndex, int secondIndex) {
            T tmp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = tmp;
        }

        public static void MoveElement<T>(this IList<T> list, int srcIndex, int dstIndex) {
            T item = list[srcIndex];
            list.RemoveAt(srcIndex);
            list.Insert(dstIndex, item);
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
    }
}