using System.Collections.Generic;
using NUnit.Framework;
using FsCheck;
using UnityEngine;
using Geidai.Common.Library;

namespace Geidai.Tests.EditMode
{
    public class CuratedSoundValidationTests
    {
        private static AudioClip Clip(string name) => AudioClip.Create(name, 8, 1, 44100, false);

        private static CuratedSoundDefinition Sound(string id, int number, string tag = "bell")
        {
            return new CuratedSoundDefinition
            {
                id = id,
                encyclopediaNumber = number,
                displayName = id,
                timbreTagId = tag,
                category = "test",
                clipRef = Clip(id),
                allowPitchShift = true,
                basePitchMidi = CuratedSoundDefinition.UnsetPitchMidi
            };
        }

        private static TimbreTagCatalog MakeTimbre()
        {
            var catalog = ScriptableObject.CreateInstance<TimbreTagCatalog>();
            catalog.SetTags(new[]
            {
                new TimbreTagDefinition { id = "bell", displayName = "ベル", sortOrder = 0 },
                new TimbreTagDefinition { id = "drum", displayName = "ドラム", sortOrder = 1 }
            });
            return catalog;
        }

        [Test]
        public void ValidateForUpsert_Rejects_Missing_Required()
        {
            var timbre = MakeTimbre();
            var bad = new CuratedSoundDefinition { id = "x", displayName = "x" };
            var result = CuratedSoundValidation.ValidateForUpsert(bad, null, timbre);
            Assert.IsFalse(result.IsSuccess);
        }

        [Test]
        public void ValidateForUpsert_Rejects_Duplicate_Id_And_Number()
        {
            var timbre = MakeTimbre();
            var existing = new List<CuratedSoundDefinition> { Sound("a", 1) };
            Assert.IsFalse(CuratedSoundValidation.ValidateForUpsert(Sound("a", 2), existing, timbre).IsSuccess);
            Assert.IsFalse(CuratedSoundValidation.ValidateForUpsert(Sound("b", 1), existing, timbre).IsSuccess);
            Assert.IsTrue(CuratedSoundValidation.ValidateForUpsert(Sound("b", 2), existing, timbre).IsSuccess);
        }

        [Test]
        public void CanRemoveTag_False_When_Referenced()
        {
            var timbre = MakeTimbre();
            var sounds = new List<CuratedSoundDefinition> { Sound("a", 1, "bell") };
            Assert.IsFalse(CuratedSoundValidation.CanRemoveTag("bell", timbre, sounds));
            Assert.IsTrue(CuratedSoundValidation.CanRemoveTag("drum", timbre, sounds));
        }

        [Test]
        public void LibraryQuery_Sorts_By_EncyclopediaNumber()
        {
            var items = new List<CuratedSoundDefinition> { Sound("b", 3), Sound("a", 1), Sound("c", 2) };
            var sorted = LibraryQuery.SortByEncyclopediaNumber(items);
            Assert.AreEqual(1, sorted[0].encyclopediaNumber);
            Assert.AreEqual(2, sorted[1].encyclopediaNumber);
            Assert.AreEqual(3, sorted[2].encyclopediaNumber);
        }

        [Test]
        public void LibraryQuery_Filter_By_Category_And_Timbre()
        {
            var items = new List<CuratedSoundDefinition>
            {
                Sound("a", 1, "bell"),
                Sound("b", 2, "drum")
            };
            items[0].category = "metal";
            items[1].category = "wood";

            var filtered = LibraryQuery.Filter(items, "metal", null);
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("a", filtered[0].id);

            filtered = LibraryQuery.Filter(items, null, "drum");
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("b", filtered[0].id);
        }

        [Test]
        public void LibraryQuery_SortAndFilter_Is_Deterministic()
        {
            Prop.ForAll<int>(seed =>
            {
                var items = new List<CuratedSoundDefinition>();
                for (int i = 0; i < 5; i++)
                {
                    int n = (Mathf.Abs(seed + i * 17) % 50) + 1;
                    items.Add(Sound("id" + i + "_" + n, n, i % 2 == 0 ? "bell" : "drum"));
                }

                var a = LibraryQuery.SortAndFilter(items, null, null);
                var b = LibraryQuery.SortAndFilter(items, null, null);
                if (a.Count != b.Count) return false;
                for (int i = 0; i < a.Count; i++)
                {
                    if (a[i].id != b[i].id) return false;
                    if (i > 0 && a[i].encyclopediaNumber < a[i - 1].encyclopediaNumber) return false;
                }
                return true;
            }).QuickCheckThrowOnFailure();
        }
    }
}
