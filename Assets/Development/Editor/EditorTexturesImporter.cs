using UnityEditor;
using UnityEngine;

namespace Development.Editor {
    
    // Sets all imported textures to this settings
    public class EditorTexturesImporter : AssetPostprocessor
    {
        void OnPostprocessTexture(Texture2D texture) {
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.alphaIsTransparency = true;
            texture.Apply();
        }
    }
}