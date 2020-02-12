using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
        internal struct InputFieldStyle {
            public GUIStyle Main;
            public GUIStyle Error;

            public GUIStyle ErrorMessage;

            public GUIStyle Postfix;
            

            public InputFieldStyle(GUISkin skin) {
                Main = skin.GetStyle("Input field");
                Error = skin.GetStyle("Input field error");
                
                Postfix = skin.GetStyle("Input field/Postfix");
                
                ErrorMessage = skin.GetStyle("Input field/Error message");
            }
        }
        internal struct FoldoutStyle {
            public GUIStyle Underline;
            
            public FoldoutStyle(GUISkin skin) {
                Underline = skin.GetStyle("Foldout");
            }
        }
        internal struct ShortcutRecorderStyle {
            public GUIStyle Main;
            public Texture RecordingIcon;

            public ShortcutRecorderStyle(GUISkin skin) {
                Main = skin.GetStyle("Keyboard shortcut main");
                RecordingIcon = Utility.LoadAssetAtPathAndAssert<Texture>(ExtendedEditorGUI.PluginPath + "/Resources/Dark/Textures/recording_icon_set.png");
            }
        }

        // Layout engine
        internal LayoutGroupsStyles LayoutGroups;
        
        // Controls
        public GUIStyle Label;
        
        public InputFieldStyle InputField;
        public FoldoutStyle Foldout;
        public ShortcutRecorderStyle ShortcutRecorder;

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
            var controlsSkin = Utility.LoadAssetAtPathAndAssert<GUISkin>(controlsSkinPath);

            Label = controlsSkin.FindStyle("Label");
            
            InputField = new InputFieldStyle(controlsSkin);
            Foldout = new FoldoutStyle(controlsSkin);
            ShortcutRecorder = new ShortcutRecorderStyle(controlsSkin);
        }
    }
}