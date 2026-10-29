using System.IO;
using UnityEditor;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>
    /// 睡蓮プレースホルダ PNG に焼き込まれたチェッカー／水色の疑似影を除去する。
    /// </summary>
    public static class Game1LilyPadArtCleanup
    {
        private const string LilyPath = "Assets/Art/Game/Game1_Placeholder_LilyPad.png";

        [MenuItem("Geidai/Game1/Clean Lily Pad Placeholder (Remove Checker)")]
        public static void CleanFromMenu()
        {
            if (!CleanAsset(LilyPath))
            {
                Debug.LogError($"[Geidai] Failed to clean {LilyPath}");
                return;
            }

            AssetDatabase.Refresh();
            Debug.Log("[Geidai] Lily pad placeholder cleaned (checker / cyan shadow removed).");
        }

        public static bool CleanAsset(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return false;

            bool prevReadable = importer.isReadable;
            if (!prevReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex == null) return false;

            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                pixels[i] = ShouldKeepPixel(c.r, c.g, c.b, c.a) ? c : new Color32(0, 0, 0, 0);
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            var png = tex.EncodeToPNG();
            File.WriteAllBytes(assetPath, png);

            if (!prevReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
            else
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return true;
        }

        internal static bool ShouldKeepPixel(int r, int g, int b, int a)
        {
            if (a < 128) return false;

            if (b >= r && b >= g - 1 && r > 155) return false;

            int min = Mathf.Min(r, Mathf.Min(g, b));
            int max = Mathf.Max(r, Mathf.Max(g, b));
            if (min > 195 && max - min < 18) return false;

            if (g >= 215 && r <= 202 && b >= 200) return false;
            if (g >= 210 && r <= 198 && b >= 208) return false;

            if (g >= r + 8 && g >= b + 5) return true;
            if (g >= r && g >= b - 3 && r < 180) return true;
            return false;
        }
    }
}
