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

        public static void Accumulate(this RectOffset rectOffset, RectOffset source) {
            rectOffset.left += source.left;
            rectOffset.right += source.right;
            rectOffset.top += source.top;
            rectOffset.bottom += source.bottom;
        }
    }
}