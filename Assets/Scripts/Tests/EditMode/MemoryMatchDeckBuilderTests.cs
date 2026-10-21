using System.Collections.Generic;
using System.Linq;
using Geidai.Common.Game;
using Geidai.Common.Library;
using NUnit.Framework;
using UnityEngine;

namespace Geidai.Tests.EditMode
{
    public class MemoryMatchDeckBuilderTests
    {
        [Test]
        public void Build_ReturnsFail_WhenNoPairKeys()
        {
            var items = new List<CuratedSoundDefinition>
            {
                MakeSound("a", "A", null),
                MakeSound("b", "B", ""),
            };

            var result = MemoryMatchDeckBuilder.Build(items, 2, 42);
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public void Build_DuplicatesSameSound_PerPair()
        {
            var clip = AudioClip.Create("t", 100, 1, 44100, false);
            var items = new List<CuratedSoundDefinition>
            {
                MakeSound("a", "A", "Bell", clip),
                MakeSound("b", "B", "Chime", clip),
            };

            var result = MemoryMatchDeckBuilder.Build(items, 2, 7);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(4, result.Cards.Count);

            var byId = result.Cards.GroupBy(c => c.SoundId).ToDictionary(g => g.Key, g => g.Count());
            Assert.AreEqual(2, byId["a"]);
            Assert.AreEqual(2, byId["b"]);
            Assert.IsTrue(result.Cards.All(c => c.SoundId == c.MatchGroupId));
        }

        [Test]
        public void Build_DoesNotPairDifferentSounds_WithSameToken()
        {
            var clip = AudioClip.Create("t", 100, 1, 44100, false);
            var items = new List<CuratedSoundDefinition>
            {
                MakeSound("a", "A", "Bell", clip),
                MakeSound("b", "B", "Bell", clip),
            };

            var result = MemoryMatchDeckBuilder.Build(items, 1, 99);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Cards.Count);
            Assert.AreEqual(result.Cards[0].SoundId, result.Cards[1].SoundId);
        }

        [Test]
        public void Tokenize_SplitsCommaSeparatedKeys()
        {
            var tokens = PairKeyTokenizer.Tokenize("FuwaFuwa, Bell");
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual("FuwaFuwa", tokens[0]);
            Assert.AreEqual("Bell", tokens[1]);
        }

        private static CuratedSoundDefinition MakeSound(string id, string name, string pairKey, AudioClip clip = null)
        {
            return new CuratedSoundDefinition
            {
                id = id,
                encyclopediaNumber = 1,
                displayName = name,
                timbreTagId = "t",
                category = "cat",
                pairKey = pairKey,
                clipRef = clip ?? AudioClip.Create(id, 50, 1, 44100, false),
            };
        }
    }
}
