using NUnit.Framework;
using UnityEngine;
using Geidai.Common.Create;

namespace Geidai.Tests.EditMode
{
    /// <summary>SoundRecipe JSON 往復（U8）。</summary>
    public class SoundRecipeJsonTests
    {
        [Test]
        public void RoundTrip_Preserves_Layers()
        {
            var original = new SoundRecipe
            {
                id = "rid",
                title = "mix",
                createdAtIso = "2026-07-30T12:00:00Z",
                layerA = new SoundRecipeLayer
                {
                    curatedSoundId = "c1",
                    volume = 0.7f,
                    pitchSemitones = 2,
                    reverb = 0.1f,
                    timbre = RecipeTimbreKind.Robot
                },
                layerB = new SoundRecipeLayer
                {
                    curatedSoundId = "c2",
                    volume = 0.3f,
                    pitchSemitones = -5,
                    reverb = 0.9f,
                    timbre = RecipeTimbreKind.Chorus
                }
            };

            var restored = JsonUtility.FromJson<SoundRecipe>(JsonUtility.ToJson(original));
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.LayerCount, restored.LayerCount);
            Assert.AreEqual(original.layerA.volume, restored.layerA.volume, 0.0001f);
            Assert.AreEqual(original.layerB.pitchSemitones, restored.layerB.pitchSemitones);
        }
    }
}
