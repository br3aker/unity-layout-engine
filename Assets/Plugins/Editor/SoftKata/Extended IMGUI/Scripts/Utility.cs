using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.ExtendedEditorGUI {
    public static class Utility {
        public static T LoadAssetAtPathAndAssert<T>(string assetPath) where T : UnityEngine.Object {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Couldn't load asset [{typeof(T).Name}] at path \"{assetPath}\"");
            return asset;
        }

        public static int GetContentHeight(this GUIStyle style) {
            return (int)Mathf.Ceil(style.CalcSize(GUIContent.none).y);
        }

        public static bool IsValid(this Rect rect) {
            return rect.height > 0f && rect.width > 0f;
        }

        public static void ResetToDefaults<T>(this IList<T> list) {
            for (int i = 0; i < list.Count; i++) {
                list[i] = default;
            }
        }
        public static void SetRange<T>(this IList<T> list, int startIndex, int endIndex, T value) {
            if (endIndex < startIndex) {
                (startIndex, endIndex) = (endIndex, startIndex);
            }

            for (int i = startIndex; i <= endIndex; i++) {
                list[i] = value;
            }
        }
    }
}