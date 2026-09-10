using System.Collections.Generic;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 解除判定の純粋ロジック（U7 / BR-UNLOCK / PBT-01）。
    /// 副作用なし。同一イベントの再適用は状態を変えず冪等。
    /// Combined は達成キー蓄積のうえ requireAll を評価する。
    /// </summary>
    public static class UnlockEvaluator
    {
        public static UnlockState Apply(
            UnlockState state,
            IReadOnlyList<UnlockRule> rules,
            IReadOnlyList<CuratedSoundDefinition> catalog,
            ProgressionEvent evt)
        {
            var next = state ?? UnlockState.Empty();
            if (evt == null) return next.Clone();

            // 1) 達成キーを蓄積（冪等）
            if (evt.type == ProgressionEventType.GameCleared && !string.IsNullOrEmpty(evt.key))
                next = next.WithGameKey(evt.key);
            else if (evt.type == ProgressionEventType.RecordingSaved && !string.IsNullOrEmpty(evt.key))
                next = next.WithRecordingKey(evt.key);

            if (rules == null) return next;

            var catalogIds = BuildIdSet(catalog);

            // 2) ルール評価（蓄積済みキー＋今回イベント）
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule == null || !rule.IsValid) continue;
                if (catalogIds.Count > 0 && !catalogIds.Contains(rule.soundId)) continue;

                if (IsSatisfied(rule, next))
                    next = next.WithUnlocked(rule.soundId);
            }

            return next;
        }

        public static UnlockState ApplyInitialUnlocks(
            UnlockState state,
            IReadOnlyList<CuratedSoundDefinition> catalog)
        {
            var next = state ?? UnlockState.Empty();
            if (catalog == null) return next.Clone();

            for (int i = 0; i < catalog.Count; i++)
            {
                var def = catalog[i];
                if (def == null || !def.IsValid || !def.initiallyUnlocked) continue;
                next = next.WithUnlocked(def.id);
            }
            return next;
        }

        public static List<LibraryItemView> Project(
            IReadOnlyList<CuratedSoundDefinition> catalog,
            UnlockState state,
            TimbreTagCatalog timbreCatalog = null)
        {
            var result = new List<LibraryItemView>();
            if (catalog == null) return result;
            var s = state ?? UnlockState.Empty();

            for (int i = 0; i < catalog.Count; i++)
            {
                var def = catalog[i];
                if (def == null || !def.IsValid) continue;
                result.Add(LibraryItemView.From(def, s.Contains(def.id), timbreCatalog));
            }
            return result;
        }

        public static bool UnlockedIdsAreSubsetOfCatalog(
            UnlockState state,
            IReadOnlyList<CuratedSoundDefinition> catalog)
        {
            if (state?.unlockedIds == null) return true;
            var ids = BuildIdSet(catalog);
            if (ids.Count == 0) return true;
            for (int i = 0; i < state.unlockedIds.Length; i++)
            {
                string id = state.unlockedIds[i];
                if (string.IsNullOrEmpty(id)) continue;
                if (!ids.Contains(id)) return false;
            }
            return true;
        }

        public static bool IsSatisfied(UnlockRule rule, UnlockState state)
        {
            if (rule == null || state == null) return false;

            switch (rule.kind)
            {
                case UnlockConditionKind.GameClear:
                    return !string.IsNullOrEmpty(rule.gameKey) && state.HasGameKey(rule.gameKey);

                case UnlockConditionKind.RecordingChallenge:
                    return !string.IsNullOrEmpty(rule.recordingChallengeKey)
                           && state.HasRecordingKey(rule.recordingChallengeKey);

                case UnlockConditionKind.Combined:
                    return IsCombinedSatisfied(rule, state);

                default:
                    return false;
            }
        }

        private static bool IsCombinedSatisfied(UnlockRule rule, UnlockState state)
        {
            bool hasGame = !string.IsNullOrEmpty(rule.gameKey);
            bool hasRec = !string.IsNullOrEmpty(rule.recordingChallengeKey);
            if (!hasGame && !hasRec) return false;

            bool gameOk = !hasGame || state.HasGameKey(rule.gameKey);
            bool recOk = !hasRec || state.HasRecordingKey(rule.recordingChallengeKey);

            if (rule.requireAll)
                return gameOk && recOk;
            return gameOk || recOk;
        }

        private static HashSet<string> BuildIdSet(IReadOnlyList<CuratedSoundDefinition> catalog)
        {
            var set = new HashSet<string>();
            if (catalog == null) return set;
            for (int i = 0; i < catalog.Count; i++)
            {
                var d = catalog[i];
                if (d != null && d.IsValid) set.Add(d.id);
            }
            return set;
        }
    }
}
