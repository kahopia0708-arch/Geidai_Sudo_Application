using System;
using System.Collections.Generic;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 解除済み素材ID＋達成キー集合（永続 / FR-23）。
    /// 同一ID・同一キーの再追加は冪等（BR-UNLOCK-03）。JsonUtility 対応。
    /// </summary>
    [Serializable]
    public class UnlockState
    {
        public string[] unlockedIds = Array.Empty<string>();
        public string[] achievedGameKeys = Array.Empty<string>();
        public string[] achievedRecordingKeys = Array.Empty<string>();
        public int version = 1;

        public static UnlockState Empty() => new UnlockState
        {
            unlockedIds = Array.Empty<string>(),
            achievedGameKeys = Array.Empty<string>(),
            achievedRecordingKeys = Array.Empty<string>(),
            version = 1
        };

        public bool Contains(string id) => ContainsIn(unlockedIds, id);
        public bool HasGameKey(string key) => ContainsIn(achievedGameKeys, key);
        public bool HasRecordingKey(string key) => ContainsIn(achievedRecordingKeys, key);

        public UnlockState WithUnlocked(string id)
        {
            if (string.IsNullOrEmpty(id) || Contains(id)) return Clone();
            return new UnlockState
            {
                unlockedIds = Append(unlockedIds, id),
                achievedGameKeys = CloneArr(achievedGameKeys),
                achievedRecordingKeys = CloneArr(achievedRecordingKeys),
                version = version
            };
        }

        public UnlockState WithGameKey(string key)
        {
            if (string.IsNullOrEmpty(key) || HasGameKey(key)) return Clone();
            return new UnlockState
            {
                unlockedIds = CloneArr(unlockedIds),
                achievedGameKeys = Append(achievedGameKeys, key),
                achievedRecordingKeys = CloneArr(achievedRecordingKeys),
                version = version
            };
        }

        public UnlockState WithRecordingKey(string key)
        {
            if (string.IsNullOrEmpty(key) || HasRecordingKey(key)) return Clone();
            return new UnlockState
            {
                unlockedIds = CloneArr(unlockedIds),
                achievedGameKeys = CloneArr(achievedGameKeys),
                achievedRecordingKeys = Append(achievedRecordingKeys, key),
                version = version
            };
        }

        public UnlockState Clone()
        {
            return new UnlockState
            {
                unlockedIds = CloneArr(unlockedIds),
                achievedGameKeys = CloneArr(achievedGameKeys),
                achievedRecordingKeys = CloneArr(achievedRecordingKeys),
                version = version
            };
        }

        private static bool ContainsIn(string[] arr, string value)
        {
            if (string.IsNullOrEmpty(value) || arr == null) return false;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] == value) return true;
            return false;
        }

        private static string[] Append(string[] arr, string value)
        {
            var list = new List<string>(arr ?? Array.Empty<string>()) { value };
            return list.ToArray();
        }

        private static string[] CloneArr(string[] arr)
            => arr != null ? (string[])arr.Clone() : Array.Empty<string>();
    }
}
