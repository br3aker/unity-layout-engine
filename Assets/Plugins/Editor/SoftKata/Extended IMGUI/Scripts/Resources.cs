using UnityEditor;
using UnityEngine;

namespace SoftKata.ExtendedEditorGUI {
    public static partial class ExtendedEditorGUI {
        internal const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";

        private static LayoutResources _layoutResources;
        private static ControlsResources _controlsResources;
        private static ComplexControlsResources _complexControlsResources;

        public static LayoutResources LayoutResources => _layoutResources ?? (_layoutResources = new LayoutResources());
        public static ControlsResources ControlsResources => _controlsResources ?? (_controlsResources = new ControlsResources());
        public static ComplexControlsResources ComplexControlsResources => _complexControlsResources ?? (_complexControlsResources = new ComplexControlsResources());
    }

    public class LayoutResources {
        private const string LayoutEngineLightSkinSubPath = "/Light Layout Engine skin.guiskin";
        private const string LayoutEngineDarkSkinSubPath = "/Dark Layout Engine skin.guiskin";

        public GUIStyle VerticalGroup;
        public GUIStyle VerticalFadeGroup;
        public GUIStyle Treeview;

        public GUIStyle HorizontalGroup;
        public GUIStyle HorizontalRestrictedGroup;

        public GUIStyle ScrollGroup;

        public GUIStyle PureScrollGroup;
        
        internal LayoutResources() {
            var skinPath =
                ExtendedEditorGUI.PluginPath + (EditorGUIUtility.isProSkin ? LayoutEngineDarkSkinSubPath : LayoutEngineLightSkinSubPath);
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
            
            VerticalGroup = skin.GetStyle("Vertical group");
            VerticalFadeGroup = skin.GetStyle("Vertical fade group");
            ScrollGroup = skin.GetStyle("Scroll group");
            Treeview = skin.GetStyle("Treeview");
            HorizontalGroup = skin.GetStyle("Horizontal group");
            HorizontalRestrictedGroup = skin.GetStyle("Horizontal restricted group");

            PureScrollGroup = skin.GetStyle("Pure scroll group");
        }
    }

    public class ControlsResources {
        private const string ControlsLightSkinSubPath = "/Light Controls skin.guiskin";
        private const string ControlsDarkSkinSubPath = "/Dark Controls skin.guiskin";
        
        // GUIStyles
        public GUIStyle InputFieldPostfix;
        public GUIStyle ButtonLeft;
        public GUIStyle ButtonMid;
        public GUIStyle ButtonRight;
        public GUIStyle Label;
        public GUIStyle Foldout;
        public GUIStyle TabHeader;

        // Textures
        public Texture GreenGradient;
        
        // Complex
        public KeyboardShortcutRecorder ShortcutRecorder;
        
        internal ControlsResources() {
            var skinPath =
                ExtendedEditorGUI.PluginPath + (EditorGUIUtility.isProSkin ? ControlsDarkSkinSubPath : ControlsLightSkinSubPath);
            var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
            
            // Classic styles
            Label = skin.FindStyle("Label");
            Foldout = skin.FindStyle("Foldout");
            InputFieldPostfix = skin.GetStyle("Postfix");
            ButtonLeft = skin.GetStyle("Button left");
            ButtonMid = skin.GetStyle("Button mid");
            ButtonRight = skin.GetStyle("Button right");
            TabHeader = skin.GetStyle("Tab header");

            // Textures
            GreenGradient = Utility.LoadAssetAtPathAndAssert<Texture>(
                ExtendedEditorGUI.PluginPath + "/Resources/Dark/Textures/green_gradient.png");
            
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

    public class ComplexControlsResources {
        public CardElementData StaticCard;
        public CardElementData ExpandableCard;
        public ComplexControlsResources() {
            var layoutResources = ExtendedEditorGUI.LayoutResources;
            var controlsResources= ExtendedEditorGUI.ControlsResources;

            StaticCard = new CardElementData {
                BackgroundColor = new Color(0.20f, 0.20f, 0.20f),
                HeaderStyle = controlsResources.Label,
                ContentStyle = layoutResources.VerticalGroup,
                RootStyle = layoutResources.VerticalGroup
            };
            
            ExpandableCard = new CardElementData {
                BackgroundColor = new Color(0.20f, 0.20f, 0.20f),
                HeaderStyle = controlsResources.Foldout,
                ContentStyle = layoutResources.VerticalGroup,
                RootStyle = layoutResources.VerticalGroup
            };
        }
        
        public struct CardElementData {
            public GUIStyle HeaderStyle;
            public GUIStyle RootStyle;
            public GUIStyle ContentStyle;

            public Color BackgroundColor;
        }
    }
}