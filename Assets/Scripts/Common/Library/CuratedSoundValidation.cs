using System.Collections.Generic;
using Geidai.Common.Results;

namespace Geidai.Common.Library
{
    /// <summary>
    /// 図鑑定義・音色語彙の純粋検証（U7a / Editor・テスト共用）。
    /// </summary>
    public static class CuratedSoundValidation
    {
        public static Result ValidateForUpsert(
            CuratedSoundDefinition candidate,
            IReadOnlyList<CuratedSoundDefinition> existing,
            TimbreTagCatalog timbreCatalog,
            string replaceId = null)
        {
            if (candidate == null)
                return Result.Fail(ResultCode.ValidationError, "ていぎが ないよ");

            if (!candidate.IsValid)
                return Result.Fail(ResultCode.ValidationError, "ひつような こうもくが たりないよ");

            if (timbreCatalog == null || !timbreCatalog.ContainsId(candidate.timbreTagId))
                return Result.Fail(ResultCode.ValidationError, "おんしょくタグが みつからないよ");

            if (candidate.HasBasePitch == false && candidate.basePitchMidi != CuratedSoundDefinition.UnsetPitchMidi)
                return Result.Fail(ResultCode.ValidationError, "ピッチは 0〜127 か みせってい(-1) にしてね");

            if (existing != null)
            {
                for (int i = 0; i < existing.Count; i++)
                {
                    var other = existing[i];
                    if (other == null || !other.IsValid) continue;
                    if (!string.IsNullOrEmpty(replaceId) && other.id == replaceId) continue;

                    if (other.id == candidate.id)
                        return Result.Fail(ResultCode.ValidationError, "おなじ ID が あるよ");
                    if (other.encyclopediaNumber == candidate.encyclopediaNumber)
                        return Result.Fail(ResultCode.ValidationError, "おなじ ずかんナンバーが あるよ");
                }
            }

            return Result.Ok();
        }

        public static bool CanRemoveTag(
            string tagId,
            TimbreTagCatalog timbreCatalog,
            IReadOnlyList<CuratedSoundDefinition> sounds)
        {
            if (string.IsNullOrEmpty(tagId) || timbreCatalog == null || !timbreCatalog.ContainsId(tagId))
                return false;

            if (sounds == null) return true;
            for (int i = 0; i < sounds.Count; i++)
            {
                var s = sounds[i];
                if (s != null && s.timbreTagId == tagId) return false;
            }
            return true;
        }

        public static Result ValidateTagUpsert(
            TimbreTagDefinition candidate,
            IReadOnlyList<TimbreTagDefinition> existing,
            string replaceId = null)
        {
            if (candidate == null || !candidate.IsValid)
                return Result.Fail(ResultCode.ValidationError, "タグが たりないよ");

            if (existing == null) return Result.Ok();
            for (int i = 0; i < existing.Count; i++)
            {
                var other = existing[i];
                if (other == null || !other.IsValid) continue;
                if (!string.IsNullOrEmpty(replaceId) && other.id == replaceId) continue;
                if (other.id == candidate.id)
                    return Result.Fail(ResultCode.ValidationError, "おなじ タグ ID が あるよ");
            }
            return Result.Ok();
        }

        public static bool IsSoundReferencedByUnlockRules(string soundId, UnlockRulesCatalog unlockRules)
        {
            if (string.IsNullOrEmpty(soundId) || unlockRules == null) return false;
            var rules = unlockRules.ValidRules();
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].soundId == soundId) return true;
            }
            return false;
        }

        public static Result ValidateForRemove(
            string soundId,
            CuratedSoundCatalog catalog,
            UnlockRulesCatalog unlockRules)
        {
            if (catalog == null)
                return Result.Fail(ResultCode.NotFound, "カタログが ないよ");
            if (string.IsNullOrEmpty(soundId) || !catalog.ContainsId(soundId))
                return Result.Fail(ResultCode.NotFound, "音が みつからないよ");
            if (IsSoundReferencedByUnlockRules(soundId, unlockRules))
                return Result.Fail(ResultCode.ValidationError, "アンロック条件で つかわれているから さくじょできないよ");
            return Result.Ok();
        }
    }
}
