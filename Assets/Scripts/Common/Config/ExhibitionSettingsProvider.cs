using UnityEngine;

namespace Geidai.Common.Config
{
    /// <summary>Resources から展示設定を1回だけ読む。</summary>
    public static class ExhibitionSettingsProvider
    {
        private const string DefaultResourcesPath = "Geidai/ExhibitionSettings";
        private static GeidaiExhibitionSettings _cached;
        private static bool _loaded;

        public static GeidaiExhibitionSettings Current
        {
            get
            {
                if (!_loaded)
                {
                    _cached = Resources.Load<GeidaiExhibitionSettings>(DefaultResourcesPath);
                    _loaded = true;
                }

                return _cached;
            }
        }

        public static bool IsActive =>
            Current != null && Current.IsExhibitionMode;

#if UNITY_INCLUDE_TESTS
        public static void ResetForTests()
        {
            _cached = null;
            _loaded = false;
        }
#endif
    }
}
