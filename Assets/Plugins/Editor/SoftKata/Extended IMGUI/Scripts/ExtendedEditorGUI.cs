using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        internal const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";
        internal static ResourcesHolder Resources;
        
        private static int _userCount;
        
        public static void RegisterUsage() {
            if (0 == _userCount++) {
                Resources = new ResourcesHolder();
            }
        }
        public static void UnregisterUsage() {
            if (0 == --_userCount) {
                Resources = null;
            }
        }
    }
    
    internal class ResourcesHolder {
        private const string LayoutEngineLightSkinSubPath = "/Light Layout Engine skin.guiskin";
        private const string LayoutEngineDarkSkinSubPath = "/Dark Layout Engine skin.guiskin";

        private const string ControlsLightSkinSubPath = "/Light Controls skin.guiskin";
        private const string ControlsDarkSkinSubPath = "/Dark Controls skin.guiskin";

        // Layout Engine
        internal struct LayoutGroupsStyles {
            public GUIStyle VerticalGroup;
            public GUIStyle VerticalFadeGroup;
            public GUIStyle VerticalScrollGroup;
            public GUIStyle VerticalHierarchyGroup;
            public GUIStyle HorizontalGroup;
            public GUIStyle HorizontalFadeGroup;
            public GUIStyle HorizontalScrollGroup;
            public GUIStyle HorizontalRestrictedGroup;

            public LayoutGroupsStyles(GUISkin skin) {
                VerticalGroup = skin.GetStyle("Layout group/Vertical group");
                VerticalFadeGroup = skin.GetStyle("Layout group/Vertical fade group");
                VerticalScrollGroup = skin.GetStyle("Layout group/Vertical scroll group");
                VerticalHierarchyGroup = skin.GetStyle("Layout group/Vertical hierarchy group");
                HorizontalGroup = skin.GetStyle("Layout group/Horizontal group");
                HorizontalFadeGroup = skin.GetStyle("Layout group/Horizontal fade group");
                HorizontalScrollGroup = skin.GetStyle("Layout group/Horizontal scroll group");
                HorizontalRestrictedGroup = skin.GetStyle("Layout group/Horizontal restricted group");
            }
        }
        
        // Controls
        internal struct InputFieldStyles {
            public GUIStyle Normal;
            public GUIStyle Error;

            public GUIStyle ErrorMessage;

            public GUIStyle Postfix;
            

            public InputFieldStyles(GUISkin skin) {
                Normal = skin.GetStyle("Input field");
                Error = skin.GetStyle("Input field error");
                
                Postfix = skin.GetStyle("Input field/Postfix");
                
                ErrorMessage = skin.GetStyle("Input field/Error message");
            }
        }
        
        internal struct ToggleData {
            public GUIStyle Style;
            public Texture DisabledIcon;

            public ToggleData(GUISkin skin, string path) {
                Style = skin.GetStyle("Controls/Toggle");
                DisabledIcon = Utility.LoadAssetAtPathAndAssert<Texture>(path + "toggle_on_disabled.png");
            }
        }
        internal struct FoldoutData {
            public GUIStyle UnderlineLabelStyle;
            public GUIStyle ArrowIconStyle;
            
            public FoldoutData(GUISkin skin) {
                UnderlineLabelStyle = skin.GetStyle("Controls/Foldout/Foldout underline/Label");
                ArrowIconStyle = skin.GetStyle("Controls/Foldout/Foldout underline/Arrow icon");
            }
        }
        internal struct KeyboardListenerData {
            public GUIStyle MainLabel;
            public GUIStyle SubLabel;

            public KeyboardListenerData(GUISkin skin) {
                MainLabel = skin.GetStyle("Controls/Keyboard shortcut listener/Main label");
                SubLabel = skin.GetStyle("Controls/Keyboard shortcut listener/Sub label");
            }
        }
        internal struct ColorFieldData {
            public GUIStyle Style;

            public ColorFieldData(GUISkin skin) {
                Style = skin.GetStyle("Controls/Color field/Main");
            }
        }
        internal struct ObjectFieldData {
            public GUIStyle Style;
            public Texture OpenObjectPickerIcon;

            public ObjectFieldData(GUISkin skin, string path) {
                Style = skin.GetStyle("Controls/Object field/Main");
                OpenObjectPickerIcon = Utility.LoadAssetAtPathAndAssert<Texture>(path + "open_object_picker.png");
            }
        }

        // Layout engine
        internal LayoutGroupsStyles LayoutGroups;
        
        // Controls
        internal InputFieldStyles InputField;

//        internal ToggleData Toggle;
//        internal FoldoutData Foldout;
//        internal KeyboardListenerData KeyboardListener;
//        internal ColorFieldData ColorField;
//        internal ObjectFieldData ObjectField;

        internal ResourcesHolder() {
            // Paths resolution
            var isProSkin = EditorGUIUtility.isProSkin;
            
            var skinPath = 
                ExtendedEditorGUI.PluginPath + (isProSkin ? LayoutEngineDarkSkinSubPath : LayoutEngineLightSkinSubPath);
            var controlsSkinPath = 
                ExtendedEditorGUI.PluginPath + (isProSkin ? ControlsDarkSkinSubPath : ControlsLightSkinSubPath);
            
            // Layout engine
            LayoutGroups = new LayoutGroupsStyles(Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath));
            
            // Controls
            var controlSkin = Utility.LoadAssetAtPathAndAssert<GUISkin>(controlsSkinPath);
            
            InputField = new InputFieldStyles(controlSkin);
        }
    }
}