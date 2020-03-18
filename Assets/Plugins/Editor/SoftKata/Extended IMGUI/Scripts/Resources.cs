using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        internal const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";

        private static LayoutResources _layoutResources;
        public static LayoutResources LayoutResources => _layoutResources ?? (_layoutResources = new LayoutResources());

        private static ControlsResources _controlsResources;
        public static ControlsResources ControlsResources => _controlsResources ?? (_controlsResources = new ControlsResources());

        private static GUIElementsResources _guiElementsResources;
        public static GUIElementsResources GUIElementsResources => _guiElementsResources ?? (_guiElementsResources = new GUIElementsResources());

        private static GUIStyle EmptyStyle = new GUIStyle();
    }

    public class LayoutResources {
        private const string LightSkinSubPath = "/Light Layout Engine skin.guiskin";
        private const string DarkSkinSubPath = "/Dark Layout Engine skin.guiskin";

        public GUIStyle VerticalGroup;
        public GUIStyle VerticalFadeGroup;
        public GUIStyle Treeview;

        public GUIStyle HorizontalGroup;
        public GUIStyle HorizontalRestrictedGroup;

        public GUIStyle ScrollGroup;
        
        internal LayoutResources() {
            var skinPath =
                ExtendedEditorGUI.PluginPath + (EditorGUIUtility.isProSkin ? DarkSkinSubPath : LightSkinSubPath);
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
            
            VerticalGroup = skin.GetStyle("Vertical group");
            VerticalFadeGroup = skin.GetStyle("Vertical fade group");
            ScrollGroup = skin.GetStyle("Scroll group");
            Treeview = skin.GetStyle("Treeview");
            HorizontalGroup = skin.GetStyle("Horizontal group");
            HorizontalRestrictedGroup = skin.GetStyle("Horizontal restricted group");
        }
    }

    public class ControlsResources {
        private const string LightSkinSubPath = "/Light Controls skin.guiskin";
        private const string DarkSkinSubPath = "/Dark Controls skin.guiskin";
        
        // GUIStyles
        public GUIStyle CenteredGreyHeader;
        public GUIStyle InputFieldPostfix;
        public GUIStyle ButtonLeft;
        public GUIStyle ButtonMid;
        public GUIStyle ButtonRight;
        public GUIStyle Foldout;
        
        // Complex
        public KeyboardShortcutRecorder ShortcutRecorder;
        
        internal ControlsResources() {
            var skinPath =
                ExtendedEditorGUI.PluginPath + (EditorGUIUtility.isProSkin ? DarkSkinSubPath : LightSkinSubPath);
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
            
            // Styles
            CenteredGreyHeader = skin.FindStyle("Centered grey header");
            Foldout = skin.FindStyle("Foldout");
            InputFieldPostfix = skin.GetStyle("Postfix");
            ButtonLeft = skin.GetStyle("Button left");
            ButtonMid = skin.GetStyle("Button mid");
            ButtonRight = skin.GetStyle("Button right");
            
            // Complex
            ShortcutRecorder = new KeyboardShortcutRecorder(skin);
        }
        
        public struct KeyboardShortcutRecorder {
            public GUIStyle Style;
            public Texture RecordStateIconSet;

            public KeyboardShortcutRecorder(GUISkin skin) {
                Style = skin.GetStyle("Keyboard shortcut main");
                RecordStateIconSet =
                    Utility.LoadAssetAtPathAndAssert<Texture>(
                        ExtendedEditorGUI.PluginPath + "/Resources/Dark/Textures/recording_icon_set.png");
            }
        }
    }

    public class GUIElementsResources {
        private const string LightSkinSubPath = "/Light Elements skin.guiskin";
        private const string DarkSkinSubPath = "/Dark Elements skin.guiskin";

        // Styles
        public GUIStyle CardRoot;
        public GUIStyle CardContent;
        public GUIStyle CardHeader;
        public GUIStyle CardFoldoutHeader;

        public GUIStyle TabHeader;

        // Textures
        public Texture EmptyListIcon;

        public GUIElementsResources() {
            var skinPath =
                ExtendedEditorGUI.PluginPath + (EditorGUIUtility.isProSkin ? DarkSkinSubPath : LightSkinSubPath);
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);

            var controlsResources= ExtendedEditorGUI.ControlsResources;

            // Card element
            CardRoot = skin.GetStyle("Card root");
            CardContent = skin.GetStyle("Card content");
            CardHeader = skin.GetStyle("Card header");
            CardFoldoutHeader = skin.GetStyle("Card foldout header");

            // Tab element
            TabHeader = skin.GetStyle("Tab header");

            // Textures
            EmptyListIcon = Utility.LoadAssetAtPathAndAssert<Texture>(ExtendedEditorGUI.PluginPath + "/Resources/Dark/Textures/empty_icon.png");
        }
    }
}