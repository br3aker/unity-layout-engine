using UnityEditor;
using UnityEngine;

namespace SoftKata.UnityEditor {
    public static class Resources {
        public const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";

        private const string DarkSubFolder = "Dark";
        private const string LightSubFolder = "Light";

        private const string ControlsSkinSubPathFormat = "/{0}/Controls.guiskin";
        private const string LayoutSkinSubPathFormat = "/{0}/Layout.guiskin";
        private const string TextureFolderPathFormat = "/Textures/";

        // Primitive elements styles
        public static readonly GUIStyle CenteredGreyHeader;
        public static readonly GUIStyle InputFieldPostfix;
        public static readonly GUIStyle Foldout;

        public static readonly GUIStyle TabHeader;
        
        // Complex elements resources
        public static readonly WindowHeaderResources WindowHeader;

        // Utility
        public static readonly Texture ElevationShadow;
        public static readonly Texture ListEmptyIcon;

        // Styles with default margin/border/padding
        public static readonly GUIStyle DefaultVerticalStyle;
        public static readonly GUIStyle DefaultHorizontalStyle;

        // Default styles for groups which require non-zero values in GUIStyle
        public static readonly GUIStyle ScrollGroup;
        public static readonly GUIStyle ScrollGroupThumb;

        public static readonly GUIStyle Treeview;

        static Resources() {
            var styleTypeString = EditorGUIUtility.isProSkin ? DarkSubFolder : LightSubFolder;

            var controlsSkinPath = PluginPath + string.Format(ControlsSkinSubPathFormat, styleTypeString);
            var layoutSkinPath = PluginPath + string.Format(LayoutSkinSubPathFormat, styleTypeString);

            var texturesFolderPath = PluginPath + TextureFolderPathFormat;
            

            var controlsSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(controlsSkinPath);
            var layoutSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(layoutSkinPath);
            
            // Primitive elements
            CenteredGreyHeader = controlsSkin.FindStyle("Centered grey header");
            Foldout = controlsSkin.FindStyle("Foldout");
            InputFieldPostfix = controlsSkin.GetStyle("Postfix");

            TabHeader = controlsSkin.GetStyle("Tab header");
            
            // Complex elements
            WindowHeader = new WindowHeaderResources(controlsSkin, layoutSkin);
            ListEmptyIcon = AssetDatabase.LoadAssetAtPath<Texture>(texturesFolderPath + "empty_list.png");

            // Utility
            ElevationShadow = AssetDatabase.LoadAssetAtPath<Texture>(texturesFolderPath + "shadow.png");

            // Empty style with default values
            DefaultVerticalStyle = layoutSkin.GetStyle("Vertical");
            DefaultHorizontalStyle = layoutSkin.GetStyle("Horizontal");

            // Default styles for groups which require non-zero values in GUIStyle
            ScrollGroup = layoutSkin.GetStyle("Scroll group");
            ScrollGroupThumb = layoutSkin.GetStyle("Scroll group thumb");
            Treeview = layoutSkin.GetStyle("Treeview");
        }

        public struct WindowHeaderResources {
            public readonly GUIStyle Group;
            public readonly GUIStyle ButtonStyle;
            public readonly GUIStyle SearchBoxStyle;

            public WindowHeaderResources(GUISkin controls, GUISkin layout) {
                Group = layout.GetStyle("Window header");
                ButtonStyle = controls.GetStyle("Window header button");
                SearchBoxStyle = controls.GetStyle("Window header search box");
            }
        }
    }
}