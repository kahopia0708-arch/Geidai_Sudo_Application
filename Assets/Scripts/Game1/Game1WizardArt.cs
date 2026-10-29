using Geidai.Common.Game;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Geidai.Game1
{
    /// <summary>
    /// 師匠・弟子スプライトの解決（Resources → Config → Editor パス）。
    /// 多スプライト PNG の fileID 21300000 参照は実機で null になりやすいため Resources を優先する。
    /// </summary>
    public static class Game1WizardArt
    {
        private const string ResourcesMasterPath = "Geidai/Game1/WizardMaster";
        private const string ResourcesApprenticePath = "Geidai/Game1/WizardApprentice";

        private static Sprite _cachedResourcesMaster;
        private static Sprite _cachedResourcesApprentice;
        private static Game1StoryboardArt _resourcesArt;

        public static Sprite ResolveMaster(SoundMatchConfig config)
        {
            var fromResources = LoadResourcesMasterSprite();
            if (fromResources != null)
                return fromResources;

            var fromConfig = config != null ? config.WizardMasterSprite : null;
            if (fromConfig != null)
                return fromConfig;

            fromConfig = ResourcesArt()?.WizardMaster;
            if (fromConfig != null)
                return fromConfig;

            return LoadFromEditorPath(Game1ArtPaths.WizardMaster);
        }

        public static Sprite ResolveApprentice(SoundMatchConfig config)
        {
            var fromResources = LoadResourcesApprenticeSprite();
            if (fromResources != null)
                return fromResources;

            var fromConfig = config != null ? config.WizardApprenticeSprite : null;
            if (fromConfig != null)
                return fromConfig;

            fromConfig = ResourcesArt()?.WizardApprentice;
            if (fromConfig != null)
                return fromConfig;

            return LoadFromEditorPath(Game1ArtPaths.WizardApprentice);
        }

        private static Sprite LoadResourcesMasterSprite()
        {
            if (_cachedResourcesMaster != null)
                return _cachedResourcesMaster;
            _cachedResourcesMaster = LoadFirstSpriteFromResources(ResourcesMasterPath);
            return _cachedResourcesMaster;
        }

        private static Sprite LoadResourcesApprenticeSprite()
        {
            if (_cachedResourcesApprentice != null)
                return _cachedResourcesApprentice;
            _cachedResourcesApprentice = LoadFirstSpriteFromResources(ResourcesApprenticePath);
            return _cachedResourcesApprentice;
        }

        private static Sprite LoadFirstSpriteFromResources(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
                return null;

            var sprites = Resources.LoadAll<Sprite>(resourcesPath);
            if (sprites != null && sprites.Length > 0)
                return sprites[0];

            var single = Resources.Load<Sprite>(resourcesPath);
            return single;
        }

        private static Game1StoryboardArt ResourcesArt()
        {
            if (_resourcesArt != null)
                return _resourcesArt;

            _resourcesArt = Resources.Load<Game1StoryboardArt>(Game1StoryboardArt.ResourcesPath);
            return _resourcesArt;
        }

        private static Sprite LoadFromEditorPath(string assetPath)
        {
#if UNITY_EDITOR
            return LoadFirstSpriteAtAssetPath(assetPath);
#else
            return null;
#endif
        }

#if UNITY_EDITOR
        /// <summary>Editor / セットアップ用。多スプライト PNG から最初の Sprite を取得。</summary>
        public static Sprite LoadFirstSpriteAtAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return null;

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets != null)
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is Sprite sprite)
                        return sprite;
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
#endif
    }
}
