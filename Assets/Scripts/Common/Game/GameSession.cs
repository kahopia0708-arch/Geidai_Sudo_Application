using System.Collections.Generic;

namespace Geidai.Common.Game
{
    /// <summary>
    /// ①音合わせの進行状態（U6 / 実行時のみ・非永続 / FR-19 の非保存方針と一貫）。
    /// 判定・演出は Controller が担い、ここは進行の保持に徹する。
    /// </summary>
    public class GameSession
    {
        public List<Question> questions;
        public int currentIndex;
        public int correctCount;

        public GameSession()
        {
            questions = new List<Question>();
            currentIndex = 0;
            correctCount = 0;
        }

        public GameSession(List<Question> questions)
        {
            this.questions = questions ?? new List<Question>();
            currentIndex = 0;
            correctCount = 0;
        }

        public bool IsFinished => questions == null || currentIndex >= questions.Count;

        public Question Current =>
            (questions != null && currentIndex >= 0 && currentIndex < questions.Count)
                ? questions[currentIndex]
                : null;

        public int Total => questions != null ? questions.Count : 0;

        /// <summary>正解を記録する。</summary>
        public void MarkCorrect() => correctCount++;

        /// <summary>次の問題へ進める。</summary>
        public void Advance() => currentIndex++;
    }
}
