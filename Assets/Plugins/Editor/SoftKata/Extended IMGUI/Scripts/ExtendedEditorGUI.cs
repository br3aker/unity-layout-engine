using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SoftKata.ExtendedEditorGUI {
    // etc
    public static partial class ExtendedEditorGUI {
        public const float ShadowPixelHeight = 5;

        private static UnityAction _currentViewRepaint;
        public static UnityAction CurrentViewRepaint {
            get {
                if(_currentViewRepaint != null){
                    return _currentViewRepaint;
                }
                throw new Exception("Accessing CurrentRepaint without initialization via InitEditor(...) or InitEditorWindow(...)");
            }
        }

        public static void InitEditor(Editor editor) {
            _currentViewRepaint = editor.Repaint;
        }
        public static void InitEditorWindow(EditorWindow editorWindow) {
            _currentViewRepaint = editorWindow.Repaint;
        }
    }

    // Resources
    public static partial class ExtendedEditorGUI {
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
            public GUIStyle ButtonLeft;
            public GUIStyle ButtonMid;
            public GUIStyle ButtonRight;
            public GUIStyle Foldout;

            public GUIStyle TabHeader;

            public GUIStyle WindowHeaderButton;
            public GUIStyle WindowHeaderSearchBox;
            
            // Complex elements resources
            public ShortcutRecorderRecources ShortcutRecorder;
            public ListViewResources ListView;

            // Utility
            public readonly Texture Shadow;

            // UNDER DEVELOPMENT
            [Obsolete] public GUIStyle VerticalGroup;
            [Obsolete] public GUIStyle VerticalFadeGroup;
            [Obsolete] public GUIStyle Treeview;

            [Obsolete] public GUIStyle HorizontalGroup;
            [Obsolete] public GUIStyle HorizontalRestrictedGroup;

            [Obsolete] public GUIStyle ScrollGroup;

            // Special cases
            [Obsolete] public GUIStyle WindowHeaderGroup;
            
            internal ResourcesHolder() {
                var styleTypeString = EditorGUIUtility.isProSkin ? "Dark" : "Light";

                var controlsSkinPath = PluginPath + string.Format(ControlsSkinSubPathFormat, styleTypeString);
                var layoutSkinPath = PluginPath + string.Format(LayoutSkinSubPathFormat, styleTypeString);

                var skinTextureFolderPath = PluginPath + string.Format(TextureFolderPathFormat, styleTypeString);
                var utilityTextureFolderPath = PluginPath + "/Textures/";

                var controlsSkin = Utility.LoadAssetAtPathAndAssert<GUISkin>(controlsSkinPath);
                var layoutSkin = Utility.LoadAssetAtPathAndAssert<GUISkin>(layoutSkinPath);
                
                // Primitive elements
                CenteredGreyHeader = controlsSkin.FindStyle("Centered grey header");
                Foldout = controlsSkin.FindStyle("Foldout");
                InputFieldPostfix = controlsSkin.GetStyle("Postfix");
                ButtonLeft = controlsSkin.GetStyle("Button left");
                ButtonMid = controlsSkin.GetStyle("Button mid");
                ButtonRight = controlsSkin.GetStyle("Button right");

                TabHeader = controlsSkin.GetStyle("Tab header");

                WindowHeaderButton = controlsSkin.GetStyle("Window header button");
                WindowHeaderSearchBox = controlsSkin.GetStyle("Window header search box");
                
                // Complex elements
                ShortcutRecorder = new ShortcutRecorderRecources(controlsSkin, skinTextureFolderPath);
                ListView = new ListViewResources(controlsSkin, skinTextureFolderPath);

                // Utility
                Shadow = Utility.LoadAssetAtPathAndAssert<Texture>(utilityTextureFolderPath + "elevation_shadow.png");


                // UNDER DEVELOPMENT
                VerticalGroup = layoutSkin.GetStyle("[testing] Vertical group");
                VerticalFadeGroup = layoutSkin.GetStyle("[testing] Vertical fade group");
                ScrollGroup = layoutSkin.GetStyle("[testing] Scroll group");
                Treeview = layoutSkin.GetStyle("[testing] Treeview");
                HorizontalGroup = layoutSkin.GetStyle("[testing] Horizontal group");
                HorizontalRestrictedGroup = layoutSkin.GetStyle("[testing] Horizontal flexible group");

                // Special cases
                WindowHeaderGroup = layoutSkin.GetStyle("Window header");
            }


            public struct ShortcutRecorderRecources {
                public GUIStyle Style;
                public Texture RecordStateIconSet;

                public ShortcutRecorderRecources(GUISkin skin, string textureFolderPath) {
                    Style = skin.GetStyle("Keyboard shortcut main");
                    RecordStateIconSet =
                        Utility.LoadAssetAtPathAndAssert<Texture>(textureFolderPath + "recording_icon_set.png");
                }
            }
            public struct ListViewResources {
                public Texture EmptyIcon;

                public ListViewResources(GUISkin skin, string textureFolderPath) {
                    EmptyIcon = 
                        Utility.LoadAssetAtPathAndAssert<Texture>(textureFolderPath + "empty_list_icon.png");
                }
            }
        }
    }
}