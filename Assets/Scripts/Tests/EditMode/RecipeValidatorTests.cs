using System.Collections.Generic;
using NUnit.Framework;
using FsCheck;
using UnityEngine;
using Geidai.Common.Create;

namespace Geidai.Tests.EditMode
{
    /// <summary>RecipeValidator / SoundRecipe JSON PBT（U8）。</summary>
    public class RecipeValidatorTests
    {
        [Test]
        public void Clamp_Keeps_Parameters_In_Range()
        {
            Prop.ForAll<float, int, float>((vol, pitch, rev) =>
            {
                var recipe = new SoundRecipe
                {
                    id = "r1",
                    layerA = new SoundRecipeLayer
                    {
                        curatedSoundId = "a",
                        volume = vol,
                        pitchSemitones = pitch,
                        reverb = rev,
                        timbre = RecipeTimbreKind.Robot
                    }
                };
                var clamped = RecipeValidator.Clamp(recipe);
                return RecipeValidator.IsWithinClamp(clamped)
                       && clamped.layerA.volume >= RecipeClamp.VolumeMin
                       && clamped.layerA.volume <= RecipeClamp.VolumeMax
                       && clamped.layerA.pitchSemitones >= RecipeClamp.PitchSemitonesMin
                       && clamped.layerA.pitchSemitones <= RecipeClamp.PitchSemitonesMax;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void CanSave_Requires_Unlocked_Ids()
        {
            var recipe = new SoundRecipe
            {
                id = "r1",
                layerA = new SoundRecipeLayer { curatedSoundId = "locked" }
            };
            Assert.IsFalse(RecipeValidator.CanSave(recipe, new HashSet<string> { "other" }, out _));
            Assert.IsTrue(RecipeValidator.CanSave(recipe, new HashSet<string> { "locked" }, out _));
        }

        [Test]
        public void LayerCount_Preserved_After_Clamp()
        {
            var recipe = new SoundRecipe
            {
                id = "r1",
                layerA = new SoundRecipeLayer { curatedSoundId = "a", volume = 99f },
                layerB = new SoundRecipeLayer { curatedSoundId = "b", pitchSemitones = 999 }
            };
            int before = recipe.LayerCount;
            var clamped = RecipeValidator.Clamp(recipe);
            Assert.AreEqual(before, clamped.LayerCount);
            Assert.AreEqual(2, clamped.LayerCount);
        }

        [Test]
        public void SoundRecipe_Json_RoundTrip()
        {
            var original = new SoundRecipe
            {
                id = "abc",
                title = "テスト",
                createdAtIso = "2026-07-30T00:00:00Z",
                layerA = new SoundRecipeLayer
                {
                    curatedSoundId = "s1",
                    volume = 0.5f,
                    pitchSemitones = -3,
                    reverb = 0.2f,
                    timbre = RecipeTimbreKind.Chorus
                },
                layerB = new SoundRecipeLayer
                {
                    curatedSoundId = "s2",
                    volume = 0.8f,
                    pitchSemitones = 4,
                    reverb = 0f,
                    timbre = RecipeTimbreKind.None
                }
            };

            string json = JsonUtility.ToJson(original);
            var restored = JsonUtility.FromJson<SoundRecipe>(json);
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.title, restored.title);
            Assert.AreEqual(2, restored.LayerCount);
            Assert.AreEqual(original.layerA.curatedSoundId, restored.layerA.curatedSoundId);
            Assert.AreEqual(original.layerA.pitchSemitones, restored.layerA.pitchSemitones);
            Assert.AreEqual(original.layerB.timbre, restored.layerB.timbre);
        }
    }
}
