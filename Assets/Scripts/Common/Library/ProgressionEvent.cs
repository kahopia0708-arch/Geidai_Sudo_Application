using System;

namespace Geidai.Common.Library
{
    public enum ProgressionEventType
    {
        GameCleared = 0,
        RecordingSaved = 1
    }

    /// <summary>進行達成イベント（Rec/Game → ProgressionService）。</summary>
    [Serializable]
    public class ProgressionEvent
    {
        public ProgressionEventType type;
        public string key;
        public string occurredAtIso;

        public static ProgressionEvent GameCleared(string gameKey, string occurredAtIso = null)
        {
            return new ProgressionEvent
            {
                type = ProgressionEventType.GameCleared,
                key = gameKey ?? string.Empty,
                occurredAtIso = occurredAtIso ?? string.Empty
            };
        }

        public static ProgressionEvent RecordingSaved(string challengeKey, string occurredAtIso = null)
        {
            return new ProgressionEvent
            {
                type = ProgressionEventType.RecordingSaved,
                key = challengeKey ?? string.Empty,
                occurredAtIso = occurredAtIso ?? string.Empty
            };
        }
    }
}
