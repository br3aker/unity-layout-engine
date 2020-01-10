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
        private const string LightSkinSubPath = "/Light GUISkin.guiskin";
        private const string DarkSkinSubPath = "/Dark GUISkin.guiskin";
        
        private const string ResourcesEditorLightVersion = "/Resources/Light/";
        private const string ResourcesEditorDarkVersion = "/Resources/Dark/";

        internal struct CommonMargins {
            public GUIStyle DefaultMargins;

            public CommonMargins(GUISkin skin) {
                DefaultMargins = skin.GetStyle("Layout/Default margins");
            }
        }
        internal struct LayoutGroupData {
            public GUIStyle VerticalGroup;
            public GUIStyle VerticalFadeGroup;
            public GUIStyle VerticalScrollGroup;
            public GUIStyle VerticalSeparatorGroup;
            public GUIStyle HorizontalGroup;
            public GUIStyle HorizontalFadeGroup;
            public GUIStyle HorizontalScrollGroup;

            public LayoutGroupData(GUISkin skin) {
                VerticalGroup = skin.GetStyle("Layout group/Vertical group");
                VerticalFadeGroup = skin.GetStyle("Layout group/Vertical fade group");
                VerticalScrollGroup = skin.GetStyle("Layout group/Vertical scroll group");
                VerticalSeparatorGroup = skin.GetStyle("Layout group/Vertical separator group");
                HorizontalGroup = skin.GetStyle("Layout group/Horizontal group");
                HorizontalFadeGroup = skin.GetStyle("Layout group/Horizontal fade group");
                HorizontalScrollGroup = skin.GetStyle("Layout group/Horizontal scroll group");
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
        internal struct InputFieldData {
            public GUIStyle UnderlineMainNormal;
            public GUIStyle UnderlineMainError;
            public GUIStyle UnderlineErrorMessage;

            public GUIStyle Postfix;
            

            public InputFieldData(GUISkin skin) {
                UnderlineMainNormal = skin.GetStyle("Controls/Input field/Underline/Main");
                UnderlineMainError = skin.GetStyle("Controls/Input field/Underline/Main error");
                UnderlineErrorMessage = skin.GetStyle("Controls/Input field/Underline/Error message");

                Postfix = skin.GetStyle("Controls/Input field/Postfix");
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

        internal CommonMargins Margins;
        internal LayoutGroupData LayoutGroup;
        internal ToggleData Toggle;
        internal InputFieldData InputField;
        internal FoldoutData Foldout;
        internal KeyboardListenerData KeyboardListener;
        internal ColorFieldData ColorField;
        internal ObjectFieldData ObjectField;

        internal ResourcesHolder() {
            // Paths resolution
            var isProSkin = EditorGUIUtility.isProSkin;
            
            var skinPath = 
                ExtendedEditorGUI.PluginPath + (isProSkin ? DarkSkinSubPath : LightSkinSubPath);
            var resourcesPath = 
                ExtendedEditorGUI.PluginPath + (isProSkin ? ResourcesEditorDarkVersion : ResourcesEditorLightVersion);
            
            // Resources loading
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);

            Margins = new CommonMargins(skin);
            LayoutGroup = new LayoutGroupData(skin);
            Toggle = new ToggleData(skin, resourcesPath + "Toggle/");
            InputField = new InputFieldData(skin);
            Foldout = new FoldoutData(skin);
            KeyboardListener = new KeyboardListenerData(skin);
            ColorField = new ColorFieldData(skin);
            ObjectField = new ObjectFieldData(skin, resourcesPath + "Object field/");
        }
    }
}