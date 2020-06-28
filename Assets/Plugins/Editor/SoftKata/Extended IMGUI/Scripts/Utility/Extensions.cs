using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    public static class Extentions {
        public static Rect Intersection(this Rect a, Rect b) {
            var x = Mathf.Max(a.x, b.x);
            var num2 = Mathf.Min(a.x + a.width, b.x + b.width);
            var y = Mathf.Max(a.y, b.y);
            var num4 = Mathf.Min(a.y + a.height, b.y + b.height);
            return new Rect(x, y, num2 - x, num4 - y);
        }
    }
}