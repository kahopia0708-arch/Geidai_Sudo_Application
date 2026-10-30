using Geidai.Common.Game;
using Geidai.Game2;
using UnityEditor;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>②神経衰弱: 雲・表絵柄を Resources に同期し iPad でも表示できるようにする。</summary>
    public static class Game2ArtSetup
    {
        private const string ResourcesDir = "Assets/Resources/Geidai/Game2";
        private const string CloudResourcesPath = "Assets/Resources/Geidai/Game2/Cloud.png";
        private const string CardFaceResourcesPath = "Assets/Resources/Geidai/Game2/CardFacePattern.png";
        private const string CardBackResourcesPath = "Assets/Resources/Geidai/Game2/CardBackPattern.png";

        [MenuItem("Geidai/Game2/Sync Art To Resources")]
        public static void SyncArtToResources()
        {
            EnsureFolder(ResourcesDir);
            CopySpriteAsset(Game2ArtPaths.Cloud, CloudResourcesPath);
            CopySpriteAsset(Game2ArtPaths.CardFacePattern, CardFaceResourcesPath);
            CopySpriteAsset(Game2ArtPaths.CardBackPattern, CardBackResourcesPath);
            WireConfigCardFace();
            AssetDatabase.SaveAssets();
            Debug.Log("[Geidai] Game2: cloud / card face (and card back if present) synced to Resources.");
        }

        [MenuItem("Geidai/Game2/Wire Card Face On Config")]
        public static void WireConfigCardFace()
        {
            var config = AssetDatabase.LoadAssetAtPath<MemoryMatchConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError($"[Geidai] Missing {ConfigPath}");
                return;
            }

            var face = Game2CardArt.LoadFirstSpriteAtAssetPath(Game2ArtPaths.CardFacePattern);
            var so = new SerializedObject(config);
            so.FindProperty("cardFaceSprite").objectReferenceValue = face;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private const string ConfigPath = "Assets/Settings/MemoryMatchConfig.asset";

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = "Assets/Resources/Geidai";
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    AssetDatabase.CreateFolder("Assets/Resources", "Geidai");
                }

                AssetDatabase.CreateFolder(parent, "Game2");
            }
        }

        private static void CopySpriteAsset(string sourceAssetPath, string destAssetPath)
        {
            if (!System.IO.File.Exists(sourceAssetPath))
            {
                Debug.LogWarning($"[Geidai] Game2 art missing: {sourceAssetPath}");
                return;
            }

            if (AssetDatabase.CopyAsset(sourceAssetPath, destAssetPath))
                return;

            if (System.IO.File.Exists(destAssetPath))
                AssetDatabase.ImportAsset(destAssetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
