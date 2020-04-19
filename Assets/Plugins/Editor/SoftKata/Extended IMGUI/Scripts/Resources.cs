using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        internal const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";

        private static Resources _controlsResources;
        public static Resources ControlsResources => _controlsResources ?? (_controlsResources = new Resources());

        [System.Obsolete]
        private static GUIElementsResources _guiElementsResources;
        [System.Obsolete]
        public static GUIElementsResources GUIElementsResources => _guiElementsResources ?? (_guiElementsResources = new GUIElementsResources());
    }
    public class Resources {
        [System.Obsolete]
        private const string LightSkinSubPath = "/Light Controls skin.guiskin";
        [System.Obsolete]
        private const string DarkSkinSubPath = "/Dark Controls skin.guiskin";
        private const string GuiSkinFilePath = "/{0}/Controls.guiskin";
        
        // GUIStyles for primitive elements
        public GUIStyle CenteredGreyHeader;
        public GUIStyle InputFieldPostfix;
        public GUIStyle ButtonLeft;
        public GUIStyle ButtonMid;
        public GUIStyle ButtonRight;
        public GUIStyle Foldout;

        public GUIStyle TabHeader;
        
        // Resources for complex elements
        public KeyboardShortcutRecorder ShortcutRecorder;
        
        internal Resources() {
            var skinPath =
                ExtendedEditorGUI.PluginPath + string.Format(GuiSkinFilePath, EditorGUIUtility.isProSkin ? "Dark" : "Light");
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
            
            // Primitive elements
            CenteredGreyHeader = skin.FindStyle("Centered grey header");
            Foldout = skin.FindStyle("Foldout");
            InputFieldPostfix = skin.GetStyle("Postfix");
            ButtonLeft = skin.GetStyle("Button left");
            ButtonMid = skin.GetStyle("Button mid");
            ButtonRight = skin.GetStyle("Button right");

            TabHeader = skin.GetStyle("Tab header");
            
            // Complex elements
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
    
        // public struct ListView {
        //     public ListView(GUISkin skin, path) {

        //     }
        // }
    }

    [System.Obsolete]
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