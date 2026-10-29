using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Geidai.Common.Game;
using Geidai.Common.Library;

namespace Geidai.Tests.EditMode
{
    public class SoundMatchBaseSoundSelectorTests
    {
        [Test]
        public void TrySelectFromCatalog_PicksPitchCapableItem()
        {
            var clip = AudioClip.Create("test", 4410, 1, 44100, false);
            var badClip = AudioClip.Create("bad", 100, 1, 44100, false);
            var catalog = ScriptableObject.CreateInstance<CuratedSoundCatalog>();
            catalog.SetItems(new List<CuratedSoundDefinition>
            {
                new CuratedSoundDefinition
                {
                    id = "a",
                    encyclopediaNumber = 1,
                    displayName = "A",
                    timbreTagId = "t",
                    category = "c",
                    clipRef = clip,
                    basePitchMidi = 60,
                    allowPitchShift = true,
                },
                new CuratedSoundDefinition
                {
                    id = "bad",
                    encyclopediaNumber = 2,
                    displayName = "Bad",
                    timbreTagId = "t",
                    category = "c",
                    clipRef = badClip,
                    basePitchMidi = 60,
                    allowPitchShift = false,
                },
            });

            bool ok = SoundMatchBaseSoundSelector.TrySelectFromCatalog(
                catalog,
                null,
                42,
                out var selection);

            Assert.IsTrue(ok);
            Assert.IsTrue(selection.IsValid);
            Assert.AreEqual("a", selection.SoundId);
            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(badClip);
            Object.DestroyImmediate(catalog);
        }
    }
}
