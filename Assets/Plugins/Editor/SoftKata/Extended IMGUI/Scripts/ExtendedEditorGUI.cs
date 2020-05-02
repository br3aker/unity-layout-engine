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
        
        private static Resources _resources;
        public static Resources ControlsResources => _resources ?? (_resources = new Resources());

        public class Resources {
            private const string GuiSkinFilePathFormat = "/{0}/Controls.guiskin";
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
            
            internal Resources() {
                var styleTypeString = EditorGUIUtility.isProSkin ? "Dark" : "Light";

                var skinPath = PluginPath + string.Format(GuiSkinFilePathFormat, styleTypeString);
                var skinTextureFolderPath = PluginPath + string.Format(TextureFolderPathFormat, styleTypeString);
                var utilityTextureFolderPath = PluginPath + "/Textures/";

                var skin = Utility.LoadAssetAtPathAndAssert<GUISkin>(skinPath);
                
                // Primitive elements
                CenteredGreyHeader = skin.FindStyle("Centered grey header");
                Foldout = skin.FindStyle("Foldout");
                InputFieldPostfix = skin.GetStyle("Postfix");
                ButtonLeft = skin.GetStyle("Button left");
                ButtonMid = skin.GetStyle("Button mid");
                ButtonRight = skin.GetStyle("Button right");

                TabHeader = skin.GetStyle("Tab header");

                WindowHeaderButton = skin.GetStyle("Window header button");
                WindowHeaderSearchBox = skin.GetStyle("Window header search box");
                
                // Complex elements
                ShortcutRecorder = new ShortcutRecorderRecources(skin, skinTextureFolderPath);
                ListView = new ListViewResources(skin, skinTextureFolderPath);

                // Utility
                Shadow = Utility.LoadAssetAtPathAndAssert<Texture>(utilityTextureFolderPath + "elevation_shadow.png");
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