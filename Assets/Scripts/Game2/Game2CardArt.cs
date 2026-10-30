using Geidai.Common.Game;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Geidai.Game2
{
    /// <summary>神経衰弱の背景・カード表／裏スプライト解決。</summary>
    public static class Game2CardArt
    {
        private const string ResourcesCloudPath = "Geidai/Game2/Cloud";
        private const string ResourcesCardFacePath = "Geidai/Game2/CardFacePattern";
        private const string ResourcesCardBackPath = "Geidai/Game2/CardBackPattern";

        private static Sprite _cachedCloud;
        private static Sprite _cachedCardFace;
        private static Sprite _cachedCardBack;
        private static Sprite _cachedPlaceholderBack;

        public static Sprite ResolveCloud()
        {
            if (_cachedCloud != null) return _cachedCloud;
            _cachedCloud = Resources.Load<Sprite>(ResourcesCloudPath);
            if (_cachedCloud != null) return _cachedCloud;
            return LoadFirstSpriteAtAssetPath(Game2ArtPaths.Cloud);
        }

        /// <summary>枚数ボタン用の仮装飾（表絵柄と雲を交互）。</summary>
        public static Sprite ResolvePresetDecoration(int presetIndex)
        {
            return presetIndex % 2 == 0 ? ResolveCardFace(null) : ResolveCloud();
        }

        /// <summary>めくったときの表絵柄（ペアの区別は音のみ）。</summary>
        public static Sprite ResolveCardFace(MemoryMatchConfig config)
        {
            if (config != null && config.CardFaceSprite != null)
                return config.CardFaceSprite;

            if (_cachedCardFace != null) return _cachedCardFace;

            _cachedCardFace = LoadSpriteFromResources(ResourcesCardFacePath);
            if (_cachedCardFace != null) return _cachedCardFace;

            return LoadFirstSpriteAtAssetPath(Game2ArtPaths.CardFacePattern);
        }

        /// <summary>伏せているときの裏（未着なら null）。</summary>
        public static Sprite ResolveCardBack(MemoryMatchConfig config)
        {
            if (config != null && config.CardBackSprite != null)
                return config.CardBackSprite;

            if (_cachedCardBack != null) return _cachedCardBack;
            _cachedCardBack = Resources.Load<Sprite>(ResourcesCardBackPath);
            if (_cachedCardBack != null) return _cachedCardBack;
            return LoadFirstSpriteAtAssetPath(Game2ArtPaths.CardBackPattern);
        }

        /// <summary>裏面素材未到着時の仮模様（#23 着稿まで）。</summary>
        public static Sprite ResolvePlaceholderCardBack()
        {
            if (_cachedPlaceholderBack != null) return _cachedPlaceholderBack;

            const int w = 128;
            const int h = 168;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var baseCol = new Color(0.22f, 0.38f, 0.62f, 1f);
            var stripe = new Color(0.28f, 0.46f, 0.72f, 1f);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool diag = ((x + y) / 12) % 2 == 0;
                    tex.SetPixel(x, y, diag ? stripe : baseCol);
                }
            }

            tex.Apply();
            _cachedPlaceholderBack = Sprite.Create(
                tex,
                new Rect(0f, 0f, w, h),
                new Vector2(0.5f, 0.5f),
                100f);
            return _cachedPlaceholderBack;
        }

        public static Sprite LoadFirstSpriteAtAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;

#if UNITY_EDITOR
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets == null) return null;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite) return sprite;
            }
#endif
            return null;
        }

        private static Sprite LoadSpriteFromResources(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath)) return null;

            var direct = Resources.Load<Sprite>(resourcesPath);
            if (direct != null) return direct;

            var all = Resources.LoadAll<Sprite>(resourcesPath);
            if (all != null && all.Length > 0) return all[0];

            var atPath = Resources.LoadAll<Sprite>(resourcesPath + "/");
            if (atPath != null && atPath.Length > 0) return atPath[0];

            return null;
        }
    }
}
