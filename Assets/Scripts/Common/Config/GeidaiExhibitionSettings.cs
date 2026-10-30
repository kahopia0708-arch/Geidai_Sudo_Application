using Geidai.Common.Models;
using UnityEngine;

namespace Geidai.Common.Config
{
    /// <summary>
    /// 11月展示など、機能を絞ったビルド用設定（Resources/Geidai/ExhibitionSettings）。
    /// 開発時は isExhibitionMode=false のまま。
    /// </summary>
    [CreateAssetMenu(fileName = "ExhibitionSettings", menuName = "Geidai/Exhibition Settings", order = 20)]
    public class GeidaiExhibitionSettings : ScriptableObject
    {
        [Tooltip("ON のとき展示用導線・ナビ制限を有効にする。")]
        [SerializeField] private bool isExhibitionMode;

        [Tooltip("メイン画面（Boot）の「はじめる」を出さず、自動でルーティングする。")]
        [SerializeField] private bool skipBootScreen;

        [Tooltip("未登録でも Home へ（展示 iPad 用。Register をスキップ）。")]
        [SerializeField] private bool allowHomeWithoutProfile;

        [Tooltip("Resources 内の HomeMenuConfig（例: Geidai/HomeMenuConfig_Exhibition）。空ならシーン既定。")]
        [SerializeField] private string homeMenuConfigResourcesPath = "Geidai/HomeMenuConfig_Exhibition";

        [Tooltip("展示中はゲーム選択の「もどる」を非表示。")]
        [SerializeField] private bool hideGameSelectBackButton = true;

        [Tooltip("GoTo 拒否するシーン（録音・音作り等）。")]
        [SerializeField] private SceneId[] blockedScenes =
        {
            SceneId.Rec,
            SceneId.Create,
            SceneId.Collection,
            SceneId.Theme
        };

        public bool IsExhibitionMode => isExhibitionMode;
        public bool SkipBootScreen => skipBootScreen;
        public bool AllowHomeWithoutProfile => allowHomeWithoutProfile;
        public string HomeMenuConfigResourcesPath => homeMenuConfigResourcesPath ?? string.Empty;
        public bool HideGameSelectBackButton => hideGameSelectBackButton;
        public SceneId[] BlockedScenes => blockedScenes;

        public bool IsSceneBlocked(SceneId sceneId)
        {
            if (!isExhibitionMode || blockedScenes == null || blockedScenes.Length == 0)
                return false;
            for (int i = 0; i < blockedScenes.Length; i++)
            {
                if (blockedScenes[i] == sceneId) return true;
            }

            return false;
        }
    }
}
