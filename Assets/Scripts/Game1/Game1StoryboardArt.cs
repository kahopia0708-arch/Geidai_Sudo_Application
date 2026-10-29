using UnityEngine;

namespace Geidai.Game1
{
    /// <summary>
    /// ①音合わせの師匠・弟子（Resources フォールバック。実機ビルドで Config 参照が欠けても表示する）。
    /// </summary>
    [CreateAssetMenu(fileName = "Game1StoryboardArt", menuName = "Geidai/Game1 Storyboard Art", order = 2)]
    public class Game1StoryboardArt : ScriptableObject
    {
        public const string ResourcesPath = "Geidai/Game1StoryboardArt";

        [SerializeField] private Sprite wizardMaster;
        [SerializeField] private Sprite wizardApprentice;

        public Sprite WizardMaster => wizardMaster;
        public Sprite WizardApprentice => wizardApprentice;
    }
}
