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
        public static ResourcesHolder Resources => _resources ?? (_resources = new ResourcesHolder());

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
            public readonly ListViewResources ListView;
            public readonly WindowHeaderResources WindowHeader;

            // Utility
            public readonly Texture Shadow;

            // UNDER DEVELOPMENT
            public GUIStyle VerticalGroup;
            public GUIStyle VerticalFadeGroup;
            public GUIStyle Treeview;

            public GUIStyle HorizontalGroup;
            public GUIStyle HorizontalRestrictedGroup;

            public GUIStyle ScrollGroup;
            public GUIStyle ScrollGroupThumb;
            
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
                ListView = new ListViewResources(controlsSkin, skinTextureFolderPath);
                WindowHeader = new WindowHeaderResources(controlsSkin, layoutSkin);

                // Utility
                Shadow = LoadAssetAtPathAndAssert<Texture>(utilityTextureFolderPath + "elevation_shadow.png");


                // UNDER DEVELOPMENT
                VerticalGroup = layoutSkin.GetStyle("[testing] Vertical group");
                VerticalFadeGroup = layoutSkin.GetStyle("[testing] Vertical fade group");
                ScrollGroup = layoutSkin.GetStyle("[testing] Scroll group");
                ScrollGroupThumb = layoutSkin.GetStyle("[testing] Scroll group thumb");
                Treeview = layoutSkin.GetStyle("[testing] Treeview");
                HorizontalGroup = layoutSkin.GetStyle("[testing] Horizontal group");
                HorizontalRestrictedGroup = layoutSkin.GetStyle("[testing] Horizontal flexible group");
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

            public struct ListViewResources {
                public Texture EmptyIcon;

                public ListViewResources(GUISkin skin, string textureFolderPath) {
                    EmptyIcon = 
                        LoadAssetAtPathAndAssert<Texture>(textureFolderPath + "empty_list_icon.png");
                }
            }
        }

        public static T LoadAssetAtPathAndAssert<T>(string assetPath) where T : UnityEngine.Object {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Couldn't load asset [{typeof(T).Name}] at path \"{assetPath}\"");
            return asset;
        }
    }
}