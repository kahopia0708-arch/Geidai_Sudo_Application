using System.Collections.Generic;
using NUnit.Framework;
using Geidai.Common.Library;
using UnityEngine;

namespace Geidai.Tests.EditMode
{
    public class LibraryFilterOptionsTests
    {
        private static AudioClip Clip(string name) => AudioClip.Create(name, 8, 1, 44100, false);

        private static CuratedSoundDefinition Sound(string id, int n, string category, string tag)
        {
            return new CuratedSoundDefinition
            {
                id = id,
                encyclopediaNumber = n,
                displayName = id,
                category = category,
                timbreTagId = tag,
                clipRef = Clip(id),
                allowPitchShift = true,
                basePitchMidi = CuratedSoundDefinition.UnsetPitchMidi
            };
        }

        [Test]
        public void CategoryLabels_Prefixed_With_All()
        {
            var items = new List<CuratedSoundDefinition>
            {
                Sound("a", 1, "金属", "bell"),
                Sound("b", 2, "木", "drum"),
                Sound("c", 3, "金属", "bell")
            };
            var labels = LibraryFilterOptions.CategoryLabels(items);
            Assert.AreEqual(LibraryFilterOptions.AllLabel, labels[0]);
            Assert.AreEqual(3, labels.Count);
            Assert.AreEqual(null, LibraryFilterOptions.CategoryValueAt(labels, 0));
            Assert.Contains("金属", labels);
            Assert.Contains("木", labels);
        }

        [Test]
        public void ResolveSelectionAfterFilter_Keeps_Or_Clears()
        {
            var items = new List<LibraryItemView>
            {
                new LibraryItemView { id = "a" },
                new LibraryItemView { id = "b" }
            };
            Assert.AreEqual("a", LibraryFilterOptions.ResolveSelectionAfterFilter("a", items));
            Assert.IsNull(LibraryFilterOptions.ResolveSelectionAfterFilter("z", items));
        }

        [Test]
        public void TimbreLabels_Include_All_And_Tags()
        {
            var catalog = ScriptableObject.CreateInstance<TimbreTagCatalog>();
            catalog.SetTags(new[]
            {
                new TimbreTagDefinition { id = "bell", displayName = "ベル", sortOrder = 0 }
            });
            var labels = LibraryFilterOptions.TimbreLabels(catalog);
            Assert.AreEqual(LibraryFilterOptions.AllLabel, labels[0]);
            Assert.AreEqual("ベル", labels[1]);
            var ids = LibraryFilterOptions.TimbreIds(catalog);
            Assert.IsNull(ids[0]);
            Assert.AreEqual("bell", ids[1]);
        }
    }
}
