using Geidai.Game3;
using NUnit.Framework;
using UnityEngine;

namespace Geidai.Tests.EditMode
{
    public class Game3PitchPlayerTests
    {
        [Test]
        public void SetBase_PreservesNative48000HzClip()
        {
            var clip = AudioClip.Create(
                "game3_48000hz_test",
                4800,
                1,
                48000,
                false);
            var player = new Game3PitchPlayer();

            try
            {
                Assert.IsTrue(player.SetBase(clip));
                Assert.AreEqual(48000, player.BaseFrequency);
                Assert.AreEqual(1f, player.CurrentPitch);
                Assert.AreEqual(
                    clip.length,
                    player.GetPlaybackDurationSeconds(0),
                    0.0001f);
                Assert.AreEqual(
                    clip.length * 0.5f,
                    player.GetPlaybackDurationSeconds(1200),
                    0.0001f);
            }
            finally
            {
                player.Dispose();
                Object.DestroyImmediate(clip);
            }
        }
    }
}
