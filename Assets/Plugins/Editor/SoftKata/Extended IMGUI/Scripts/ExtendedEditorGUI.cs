using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace SoftKata.UnityEditor {
    // etc
    public static partial class ExtendedEditor {
        public static IRepaintable CurrentView {get; internal set;}
    }

    // Resources
    public static partial class ExtendedEditor {
        public const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";
        
        private static ResourcesHolder _resources;
        public static ResourcesHolder DEPRECATED_Resources => _resources ?? (_resources = new ResourcesHolder());

        public class ResourcesHolder {
            private const string ControlsSkinSubPathFormat = "/{0}/Controls.guiskin";
            private const string LayoutSkinSubPathFormat = "/{0}/Layout.guiskin";
            private const string TextureFolderPathFormat = "/{0}/Textures/";

            // Primitive elements styles
            public GUIStyle CenteredGreyHeader;
            public GUIStyle InputFieldPostfix;
            public GUIStyle Foldout;

            public GUIStyle TabHeader;
            
            // Complex elements resources
            public readonly WindowHeaderResources WindowHeader;

            // Utility
            public readonly Texture ElevationShadow;
            public readonly Texture ListEmptyIcon;

            // Styles with default margin/border/padding
            public GUIStyle DefaultVerticalStyle;
            public GUIStyle DefaultHorizontalStyle;

            // Default styles for groups which require non-zero values in GUIStyle
            public GUIStyle ScrollGroup;
            public GUIStyle ScrollGroupThumb;

            public GUIStyle Treeview;
            
            internal ResourcesHolder() {
                var styleTypeString = EditorGUIUtility.isProSkin ? "Dark" : "Light";

                var controlsSkinPath = PluginPath + string.Format(ControlsSkinSubPathFormat, styleTypeString);
                var layoutSkinPath = PluginPath + string.Format(LayoutSkinSubPathFormat, styleTypeString);

                var skinTextureFolderPath = PluginPath + string.Format(TextureFolderPathFormat, styleTypeString);
                var utilityTextureFolderPath = PluginPath + "/Textures/";

                var controlsSkin = LoadAssetAtPathAndAssert<GUISkin>(controlsSkinPath);
                var layoutSkin = LoadAssetAtPathAndAssert<GUISkin>(layoutSkinPath);
                
                // Primitive elements
                CenteredGreyHeader = controlsSkin.FindStyle("Centered grey header");
                Foldout = controlsSkin.FindStyle("Foldout");
                InputFieldPostfix = controlsSkin.GetStyle("Postfix");

                TabHeader = controlsSkin.GetStyle("Tab header");
                
                // Complex elements
                WindowHeader = new WindowHeaderResources(controlsSkin, layoutSkin);

                // Utility
                ElevationShadow = LoadAssetAtPathAndAssert<Texture>(utilityTextureFolderPath + "elevation_shadow.png");

                // Empty style with default values
                DefaultVerticalStyle = layoutSkin.GetStyle("Vertical");
                DefaultHorizontalStyle = layoutSkin.GetStyle("Horizontal");

                // Default styles for groups which require non-zero values in GUIStyle
                ScrollGroup = layoutSkin.GetStyle("Scroll group");
                ScrollGroupThumb = layoutSkin.GetStyle("Scroll group thumb");
                Treeview = layoutSkin.GetStyle("Treeview");
            }

            public struct WindowHeaderResources {
                public readonly GUIStyle GroupStyle;
                public readonly GUIStyle ButtonStyle;
                public readonly GUIStyle SearchBoxStyle;

                public WindowHeaderResources(GUISkin controls, GUISkin layout) {
                    GroupStyle = layout.GetStyle("Window header");
                    ButtonStyle = controls.GetStyle("Window header button");
                    SearchBoxStyle = controls.GetStyle("Window header search box");
                }
            }
        }

        public static T LoadAssetAtPathAndAssert<T>(string assetPath) where T : UnityEngine.Object {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Couldn't load asset [{typeof(T).Name}] at path \"{assetPath}\"");
            return asset;
        }
    }

    public static class Resources {
        public const string PluginPath = "Assets/Plugins/Editor/SoftKata/Extended IMGUI";

        private const string ControlsSkinSubPathFormat = "/{0}/Controls.guiskin";
        private const string LayoutSkinSubPathFormat = "/{0}/Layout.guiskin";
        private const string TextureFolderPathFormat = "/{0}/Textures/";

        // Primitive elements styles
        public static GUIStyle CenteredGreyHeader;
        public static GUIStyle InputFieldPostfix;
        public static GUIStyle Foldout;

        public static GUIStyle TabHeader;
        
        // Complex elements resources
        public static readonly WindowHeaderResources WindowHeader;

        // Utility
        public static readonly Texture ElevationShadow;
        public static readonly Texture ListEmptyIcon;

        // Styles with default margin/border/padding
        public static GUIStyle DefaultVerticalStyle;
        public static GUIStyle DefaultHorizontalStyle;

        // Default styles for groups which require non-zero values in GUIStyle
        public static GUIStyle ScrollGroup;
        public static GUIStyle ScrollGroupThumb;

        public static GUIStyle Treeview;

        static Resources() {
            var styleTypeString = EditorGUIUtility.isProSkin ? "Dark" : "Light";

            var controlsSkinPath = PluginPath + string.Format(ControlsSkinSubPathFormat, styleTypeString);
            var layoutSkinPath = PluginPath + string.Format(LayoutSkinSubPathFormat, styleTypeString);

            var skinTextureFolderPath = PluginPath + string.Format(TextureFolderPathFormat, styleTypeString);
            var utilityTextureFolderPath = PluginPath + "/Textures/";

            var controlsSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(controlsSkinPath);
            var layoutSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(layoutSkinPath);
            
            // Primitive elements
            CenteredGreyHeader = controlsSkin.FindStyle("Centered grey header");
            Foldout = controlsSkin.FindStyle("Foldout");
            InputFieldPostfix = controlsSkin.GetStyle("Postfix");

            TabHeader = controlsSkin.GetStyle("Tab header");
            
            // Complex elements
            WindowHeader = new WindowHeaderResources(controlsSkin, layoutSkin);

            // Utility
            ElevationShadow = AssetDatabase.LoadAssetAtPath<Texture>(utilityTextureFolderPath + "elevation_shadow.png");

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