using NUnit.Framework;
using UnityEngine;
using Geidai.Common.Library;

namespace Geidai.Tests.EditMode
{
    /// <summary>UnlockState JSON 往復（U7）。</summary>
    public class UnlockStateJsonTests
    {
        [Test]
        public void RoundTrip_Preserves_Ids_And_Keys()
        {
            var original = UnlockState.Empty()
                .WithUnlocked("s1")
                .WithUnlocked("s2")
                .WithGameKey("game.easy")
                .WithRecordingKey("rec.theme1");
            original.version = 1;

            string json = JsonUtility.ToJson(original);
            var restored = JsonUtility.FromJson<UnlockState>(json);

            Assert.IsNotNull(restored);
            Assert.IsTrue(restored.Contains("s1"));
            Assert.IsTrue(restored.Contains("s2"));
            Assert.IsTrue(restored.HasGameKey("game.easy"));
            Assert.IsTrue(restored.HasRecordingKey("rec.theme1"));
            Assert.AreEqual(1, restored.version);
        }

        [Test]
        public void WithUnlocked_Is_Idempotent()
        {
            var s0 = UnlockState.Empty();
            var s1 = s0.WithUnlocked("x");
            var s2 = s1.WithUnlocked("x");
            Assert.AreEqual(1, s1.unlockedIds.Length);
            Assert.AreEqual(1, s2.unlockedIds.Length);
        }
    }
}
