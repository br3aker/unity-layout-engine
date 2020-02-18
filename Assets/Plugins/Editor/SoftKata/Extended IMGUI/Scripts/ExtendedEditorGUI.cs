using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        internal const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";

        private static ResourcesHolder _resources;

        public static ResourcesHolder Resources => _resources ?? (_resources = new ResourcesHolder());
    }
    
    public class ResourcesHolder {
        private const string LayoutEngineLightSkinSubPath = "/Light Layout Engine skin.guiskin";
        private const string LayoutEngineDarkSkinSubPath = "/Dark Layout Engine skin.guiskin";

        private const string ControlsLightSkinSubPath = "/Light Controls skin.guiskin";
        private const string ControlsDarkSkinSubPath = "/Dark Controls skin.guiskin";

        // Layout Engine
        internal struct LayoutGroupsStyles {
            public GUIStyle VerticalGroup;
            public GUIStyle VerticalFadeGroup;
            public GUIStyle VerticalHierarchyGroup;
            
            public GUIStyle HorizontalGroup;
            public GUIStyle HorizontalRestrictedGroup;
            
            public GUIStyle ScrollGroup;

            public LayoutGroupsStyles(GUISkin skin) {
                VerticalGroup = skin.GetStyle("Vertical group");
                VerticalFadeGroup = skin.GetStyle("Vertical fade group");
                ScrollGroup = skin.GetStyle("Scroll group");
                VerticalHierarchyGroup = skin.GetStyle("Vertical hierarchy group");
                HorizontalGroup = skin.GetStyle("Horizontal group");
                HorizontalRestrictedGroup = skin.GetStyle("Horizontal restricted group");
            }
        }
        
        // Controls
        public struct PostfixInputFieldStyle {
            public GUIStyle Error;
            public GUIStyle ErrorMessage;
            
            public PostfixInputFieldStyle(GUISkin skin) {
                Error = skin.GetStyle("Input field error");
                ErrorMessage = skin.GetStyle("Input field/Error message");
            }
        }
        public struct FoldoutStyle {
            public GUIStyle Underline;
            
            public FoldoutStyle(GUISkin skin) {
                Underline = skin.GetStyle("Foldout");
            }
        }
        public struct KeyboardShortcutRecorder {
            public GUIStyle Main;
            public Texture RecordStateIconSet;

            public KeyboardShortcutRecorder(GUISkin skin) {
                Main = skin.GetStyle("Keyboard shortcut main");
                RecordStateIconSet = Utility.LoadAssetAtPathAndAssert<Texture>(ExtendedEditorGUI.PluginPath + "/Resources/Dark/Textures/recording_icon_set.png");
            }
        }
        public struct ButtonStyle {
            public GUIStyle Left;
            public GUIStyle Mid;
            public GUIStyle Right;

            public ButtonStyle(GUISkin skin) {
                var prefix = "Button ";
                Left = skin.GetStyle(prefix + "left");
                Mid = skin.GetStyle(prefix + "mid");
                Right = skin.GetStyle(prefix + "right");
            }
        }
        public struct ListElementStyle {
            public GUIStyle MainLabel;
            public GUIStyle SubLabel;

            public ListElementStyle(GUISkin skin) {
                var prefix = "List element ";
                MainLabel = skin.GetStyle(prefix + "main");
                SubLabel = skin.GetStyle(prefix + "sub");
            }
        }

        // Layout engine
        internal LayoutGroupsStyles LayoutGroups;
        
        // Controls
        public GUIStyle Label;
        public GUIStyle GenericInputField;
        public GUIStyle GenericPostfix;
        
        public PostfixInputFieldStyle PostfixInputField;
        public FoldoutStyle Foldout;
        public KeyboardShortcutRecorder ShortcutRecorder;
        public ButtonStyle Buttons;
        public ListElementStyle ListElement;

        internal ResourcesHolder() {
            // Paths resolution
            var isProSkin = EditorGUIUtility.isProSkin;
            
            var layoutEngineSkinPath = 
                ExtendedEditorGUI.PluginPath + (isProSkin ? LayoutEngineDarkSkinSubPath : LayoutEngineLightSkinSubPath);
            var controlsSkinPath = 
                ExtendedEditorGUI.PluginPath + (isProSkin ? ControlsDarkSkinSubPath : ControlsLightSkinSubPath);
            
            // Layout engine
            LayoutGroups = new LayoutGroupsStyles(Utility.LoadAssetAtPathAndAssert<GUISkin>(layoutEngineSkinPath));
            
            // Controls
            var controlsSkin = Utility.LoadAssetAtPathAndAssert<GUISkin>(controlsSkinPath);

            Label = controlsSkin.FindStyle("Label");
            GenericInputField = EditorStyles.numberField;
            GenericPostfix = controlsSkin.GetStyle("Generic Postfix");
            
            PostfixInputField = new PostfixInputFieldStyle(controlsSkin);
            Foldout = new FoldoutStyle(controlsSkin);
            ShortcutRecorder = new KeyboardShortcutRecorder(controlsSkin);
            Buttons = new ButtonStyle(controlsSkin);
            ListElement = new ListElementStyle(controlsSkin);
        }
    }
}