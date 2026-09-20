using Geidai.Common.Game;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geidai.Game3
{
    [Serializable]
    public class Game3DifficultySetting
    {
        [Tooltip("難易度名")]
        public string label;

        [Tooltip("使用する音・雲の数（2〜4）")]
        [Range(2, 4)]
        public int choiceCount;

        [Tooltip("各問題の音同士のピッチ間隔[cent]")]
        public List<int> centsSteps = new List<int>();
    }
    /// <summary>
    /// ③音ならべの出題設定。
    /// 出題数・選択肢数・難易度・カタログ音をInspectorから調整する。
    /// </summary>
    [CreateAssetMenu(
        fileName = "Game3Config",
        menuName = "Geidai/Game3 Config",
        order = 3)]
    public class Game3Config : ScriptableObject
    {
        [Tooltip("難易度ごとの設定")]
        [SerializeField]
        private List<Game3DifficultySetting> difficulties =
            new List<Game3DifficultySetting>
            {
                new Game3DifficultySetting
                {
                    label = "かんたん",
                    choiceCount = 2,
                    centsSteps = new List<int> { 100, 50, 25, 10 }
                },

                new Game3DifficultySetting
                {
                    label = "ふつう",
                    choiceCount = 3,
                    centsSteps = new List<int> { 100, 50, 25, 10 }
                },

                new Game3DifficultySetting
                {
                    label = "むずかしい",
                    choiceCount = 4,
                    centsSteps = new List<int> { 100, 50, 25, 10 }
                }
            };

        [Tooltip("音ならべで優先して使用するカタログ音")]
        [SerializeField]
        private List<AudioClip> catalogClips = new List<AudioClip>();

        public IReadOnlyList<Game3DifficultySetting> Difficulties
            => difficulties;

        public IReadOnlyList<AudioClip> CatalogClips
            => catalogClips;

        public Game3DifficultySetting GetDifficulty(int index)
        {
            if (difficulties == null || difficulties.Count == 0)
            {
                return new Game3DifficultySetting
                {
                    label = "ふつう",
                    choiceCount = 3,
                    centsSteps = new List<int> { 100, 50, 25, 10 }
                };
            }

            int clamped =
                Mathf.Clamp(index, 0, difficulties.Count - 1);

            return difficulties[clamped];
        }

        public int GetChoiceCount(int difficultyIndex)
        {
            Game3DifficultySetting difficulty =
                GetDifficulty(difficultyIndex);

            return Mathf.Clamp(
                difficulty.choiceCount,
                2,
                4
            );
        }

        public int GetQuestionCount(int difficultyIndex)
        {
            Game3DifficultySetting difficulty =
                GetDifficulty(difficultyIndex);

            if (difficulty.centsSteps == null)
                return 0;

            return difficulty.centsSteps.Count;
        }

        public int GetCentsStep(
            int difficultyIndex,
            int questionIndex)
        {
            Game3DifficultySetting difficulty =
                GetDifficulty(difficultyIndex);

            if (difficulty.centsSteps == null ||
                difficulty.centsSteps.Count == 0)
            {
                return 100;
            }

            int clamped = Mathf.Clamp(
                questionIndex,
                0,
                difficulty.centsSteps.Count - 1
            );

            return Mathf.Max(
                1,
                difficulty.centsSteps[clamped]
            );
        }
    }
}
