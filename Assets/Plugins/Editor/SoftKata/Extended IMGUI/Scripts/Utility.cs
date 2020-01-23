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
        
        public static Rect RectIntersection(Rect a, Rect b)
        {
            float x = Mathf.Max(a.x, b.x);
            var num2 = Mathf.Min(a.x + a.width, b.x + b.width);
            float y = Mathf.Max(a.y, b.y);
            var num4 = Mathf.Min(a.y + a.height, b.y + b.height);
            if ((num2 >= x) && (num4 >= y))
            {
                return new Rect(x, y, num2 - x, num4 - y);
            }
 
            return new Rect();
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