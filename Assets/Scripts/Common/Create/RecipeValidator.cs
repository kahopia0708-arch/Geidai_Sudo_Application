using System;
using System.Collections.Generic;

namespace Geidai.Common.Create
{
    /// <summary>
    /// レシピ検証・クランプの純粋ロジック（U8 / BR-CREATE / PBT）。
    /// </summary>
    public static class RecipeValidator
    {
        public const int MaxLayers = 2;

        /// <summary>パラメータを範囲内にクランプしたコピーを返す。</summary>
        public static SoundRecipe Clamp(SoundRecipe recipe)
        {
            if (recipe == null) return null;
            var copy = recipe.Clone();
            if (copy.layerA != null) ClampLayer(copy.layerA);
            if (copy.layerB != null) ClampLayer(copy.layerB);
            return copy;
        }

        public static void ClampLayer(SoundRecipeLayer layer)
        {
            if (layer == null) return;
            layer.volume = RecipeClamp.ClampVolume(layer.volume);
            layer.pitchSemitones = RecipeClamp.ClampPitch(layer.pitchSemitones);
            layer.reverb = RecipeClamp.ClampReverb(layer.reverb);
            if (!Enum.IsDefined(typeof(RecipeTimbreKind), layer.timbre))
                layer.timbre = RecipeTimbreKind.None;
        }

        /// <summary>
        /// 保存可能か検証する。unlockIds が非 null のとき、選択素材は解除済み必須（BR-CREATE-01）。
        /// </summary>
        public static bool CanSave(SoundRecipe recipe, ICollection<string> unlockIds, out string reason)
        {
            reason = null;
            if (recipe == null)
            {
                reason = "レシピが ないよ";
                return false;
            }
            if (string.IsNullOrWhiteSpace(recipe.id))
            {
                reason = "IDが ないよ";
                return false;
            }

            int count = recipe.LayerCount;
            if (count == 0)
            {
                reason = "おとを えらんでね";
                return false;
            }
            if (count > MaxLayers)
            {
                reason = "2つまで だよ";
                return false;
            }

            if (!LayerAllowed(recipe.layerA, unlockIds, out reason)) return false;
            if (!LayerAllowed(recipe.layerB, unlockIds, out reason)) return false;
            return true;
        }

        /// <summary>全レイヤーパラメータがクランプ範囲内か。</summary>
        public static bool IsWithinClamp(SoundRecipe recipe)
        {
            if (recipe == null) return false;
            return LayerWithin(recipe.layerA) && LayerWithin(recipe.layerB);
        }

        private static bool LayerAllowed(SoundRecipeLayer layer, ICollection<string> unlockIds, out string reason)
        {
            reason = null;
            if (layer == null || string.IsNullOrEmpty(layer.curatedSoundId)) return true;
            if (unlockIds == null) return true;
            if (!unlockIds.Contains(layer.curatedSoundId))
            {
                reason = "まだ つかえない おとだよ";
                return false;
            }
            return true;
        }

        private static bool LayerWithin(SoundRecipeLayer layer)
        {
            if (layer == null || string.IsNullOrEmpty(layer.curatedSoundId)) return true;
            return layer.volume >= RecipeClamp.VolumeMin && layer.volume <= RecipeClamp.VolumeMax
                   && layer.pitchSemitones >= RecipeClamp.PitchSemitonesMin
                   && layer.pitchSemitones <= RecipeClamp.PitchSemitonesMax
                   && layer.reverb >= RecipeClamp.ReverbMin && layer.reverb <= RecipeClamp.ReverbMax;
        }
    }
}
