using System;
using System.Collections.Generic;
using Geidai.Common.Library;
using Geidai.Common.Results;
using Geidai.Common.Utils;
using Geidai.Services.Content;
using Geidai.Services.Storage;

namespace Geidai.Services.Progression
{
    /// <summary>
    /// ProgressionService 本実装（U7）。
    /// UnlockEvaluator（純粋）＋ Storage 原子的保存。同一イベント再適用は冪等。
    /// </summary>
    public class ProgressionService : IProgressionService
    {
        private readonly IStorageService _storage;
        private readonly IContentService _content;
        private UnlockState _cache;

        public ProgressionService(IStorageService storage, IContentService content)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _cache = UnlockState.Empty();
            Reload();
        }

        public UnlockState CurrentUnlockState => _cache?.Clone() ?? UnlockState.Empty();

        public Result Reload()
        {
            var loaded = _storage.LoadUnlockState();
            _cache = loaded.IsSuccess && loaded.Value != null ? loaded.Value : UnlockState.Empty();
            return Result.Ok();
        }

        public Result ApplyInitialUnlocks()
        {
            try
            {
                var catalogResult = _content.GetCuratedCatalog();
                IReadOnlyList<CuratedSoundDefinition> defs = catalogResult.IsSuccess && catalogResult.Value != null
                    ? catalogResult.Value.ValidItems()
                    : Array.Empty<CuratedSoundDefinition>();

                var next = UnlockEvaluator.ApplyInitialUnlocks(_cache, defs);
                return PersistIfChanged(next);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Progression] ApplyInitialUnlocks failed: " + e.Message);
                return Result.Fail(ResultCode.Unknown, "しょきかいじょに しっぱいしたよ");
            }
        }

        public Result NotifyGameCleared(string gameKey)
        {
            return ApplyEvent(ProgressionEvent.GameCleared(gameKey, DateTime.UtcNow.ToString("o")));
        }

        public Result NotifyRecordingChallenge(string challengeKey)
        {
            return ApplyEvent(ProgressionEvent.RecordingSaved(challengeKey, DateTime.UtcNow.ToString("o")));
        }

        private Result ApplyEvent(ProgressionEvent evt)
        {
            try
            {
                var rulesResult = _content.GetUnlockRules();
                IReadOnlyList<UnlockRule> rules = rulesResult.IsSuccess && rulesResult.Value != null
                    ? rulesResult.Value.ValidRules()
                    : Array.Empty<UnlockRule>();

                var catalogResult = _content.GetCuratedCatalog();
                IReadOnlyList<CuratedSoundDefinition> defs = catalogResult.IsSuccess && catalogResult.Value != null
                    ? catalogResult.Value.ValidItems()
                    : Array.Empty<CuratedSoundDefinition>();

                var next = UnlockEvaluator.Apply(_cache, rules, defs, evt);
                return PersistIfChanged(next);
            }
            catch (Exception e)
            {
                SafeLogger.Error("[Progression] ApplyEvent failed: " + e.Message);
                return Result.Fail(ResultCode.Unknown, "しんこうの こうしんに しっぱいしたよ");
            }
        }

        private Result PersistIfChanged(UnlockState next)
        {
            if (StatesEqual(_cache, next))
            {
                _cache = next;
                return Result.Ok();
            }

            var save = _storage.SaveUnlockState(next);
            if (!save.IsSuccess) return save;
            _cache = next;
            return Result.Ok();
        }

        private static bool StatesEqual(UnlockState a, UnlockState b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return ArrayEquals(a.unlockedIds, b.unlockedIds)
                   && ArrayEquals(a.achievedGameKeys, b.achievedGameKeys)
                   && ArrayEquals(a.achievedRecordingKeys, b.achievedRecordingKeys)
                   && a.version == b.version;
        }

        private static bool ArrayEquals(string[] a, string[] b)
        {
            a ??= Array.Empty<string>();
            b ??= Array.Empty<string>();
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
