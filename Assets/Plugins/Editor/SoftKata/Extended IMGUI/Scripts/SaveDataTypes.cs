using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveDataTypes {
    public class SaveInt {
        private string _name;
        
        public int Value {
            get => EditorPrefs.GetInt(_name);
            set => EditorPrefs.SetInt(_name, value);
        }

        public SaveInt(string name) {
            _name = name;
        }
    }

    public class SaveBool {
        private string _name;
        
        public bool Value {
            get => EditorPrefs.GetBool(_name);
            set => EditorPrefs.SetBool(_name, value);
        }

        public SaveBool(string name) {
            _name = name;
        }
    }
    
    public class SaveColor {
        private string _name_rg;
        private string _name_ba;
        
        public Color Value {
            get {
                int rawValue_rg = EditorPrefs.GetInt(_name_rg);
                int rawValue_ba = EditorPrefs.GetInt(_name_ba);
                float r = (float) (rawValue_rg & 255) / 255;
                float g = (float) ((rawValue_rg << 8) & 255) / 255;
                float b = (float) (rawValue_ba & 255) / 255;
                float a = (float) ((rawValue_ba << 8) & 255) / 255;
                return new Color(r, g, b, a);
            }
            set {
                int rawValue_rg = (int)(value.r * 255);
                rawValue_rg |= ((int) (value.g * 255)) >> 8;
                
                int rawValue_ba = (int)(value.b * 255);
                rawValue_ba |= ((int) (value.a * 255)) >> 8;
                
                EditorPrefs.SetInt(_name_rg, rawValue_rg);
                EditorPrefs.SetInt(_name_ba, rawValue_ba);
            }
        }

        public SaveColor(string name) {
            _name_rg = name + "_rg";
            _name_ba = name + "_ba";
        }
    }
}
