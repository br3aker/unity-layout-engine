using System;
using UnityEditor;
using UnityEngine;

[Obsolete]
public class SaveDataTypes {
    public class SaveInt {
        private readonly string _name;

        public SaveInt(string name) {
            _name = name;
        }

        public int Value {
            get => EditorPrefs.GetInt(_name);
            set => EditorPrefs.SetInt(_name, value);
        }
    }

    public class SaveBool {
        private readonly string _name;

        public SaveBool(string name) {
            _name = name;
        }

        public bool Value {
            get => EditorPrefs.GetBool(_name);
            set => EditorPrefs.SetBool(_name, value);
        }
    }

    public class SaveColor {
        private readonly string _name_ba;
        private readonly string _name_rg;

        public SaveColor(string name) {
            _name_rg = name + "_rg";
            _name_ba = name + "_ba";
        }

        public Color Value {
            get {
                var rawValue_rg = EditorPrefs.GetInt(_name_rg);
                var rawValue_ba = EditorPrefs.GetInt(_name_ba);
                var r = (float) (rawValue_rg & 255) / 255;
                var g = (float) ((rawValue_rg << 8) & 255) / 255;
                var b = (float) (rawValue_ba & 255) / 255;
                var a = (float) ((rawValue_ba << 8) & 255) / 255;
                return new Color(r, g, b, a);
            }
            set {
                var rawValue_rg = (int) (value.r * 255);
                rawValue_rg |= (int) (value.g * 255) >> 8;

                var rawValue_ba = (int) (value.b * 255);
                rawValue_ba |= (int) (value.a * 255) >> 8;

                EditorPrefs.SetInt(_name_rg, rawValue_rg);
                EditorPrefs.SetInt(_name_ba, rawValue_ba);
            }
        }
    }
}