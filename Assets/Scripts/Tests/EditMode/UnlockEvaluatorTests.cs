using System.Collections.Generic;
using NUnit.Framework;
using FsCheck;
using UnityEngine;
using Geidai.Common.Library;

namespace Geidai.Tests.EditMode
{
    /// <summary>UnlockEvaluator PBT（冪等・Combined・初期解除・投影）。</summary>
    public class UnlockEvaluatorTests
    {
        private static CuratedSoundDefinition Def(string id, bool initial = false)
        {
            var clip = AudioClip.Create(id, 8, 1, 44100, false);
            return new CuratedSoundDefinition
            {
                id = id,
                encyclopediaNumber = Mathf.Abs(id.GetHashCode() % 100000) + 1,
                displayName = id,
                timbreTagId = "bell",
                category = "test",
                clipRef = clip,
                initiallyUnlocked = initial,
                allowPitchShift = true,
                basePitchMidi = CuratedSoundDefinition.UnsetPitchMidi
            };
        }

        [Test]
        public void Apply_Is_Idempotent()
        {
            Prop.ForAll<int>(_ =>
            {
                var catalog = new List<CuratedSoundDefinition> { Def("s1") };
                var rules = new List<UnlockRule>
                {
                    new UnlockRule
                    {
                        soundId = "s1",
                        kind = UnlockConditionKind.GameClear,
                        gameKey = "g1"
                    }
                };
                var evt = ProgressionEvent.GameCleared("g1");
                var s1 = UnlockEvaluator.Apply(UnlockState.Empty(), rules, catalog, evt);
                var s2 = UnlockEvaluator.Apply(s1, rules, catalog, evt);
                return s1.Contains("s1")
                       && s2.Contains("s1")
                       && s1.unlockedIds.Length == s2.unlockedIds.Length
                       && s1.achievedGameKeys.Length == s2.achievedGameKeys.Length;
            }).QuickCheckThrowOnFailure();
        }

        [Test]
        public void Combined_RequireAll_Needs_Both_Keys()
        {
            var catalog = new List<CuratedSoundDefinition> { Def("combo") };
            var rules = new List<UnlockRule>
            {
                new UnlockRule
                {
                    soundId = "combo",
                    kind = UnlockConditionKind.Combined,
                    gameKey = "gA",
                    recordingChallengeKey = "rA",
                    requireAll = true
                }
            };

            var s1 = UnlockEvaluator.Apply(UnlockState.Empty(), rules, catalog, ProgressionEvent.GameCleared("gA"));
            Assert.IsFalse(s1.Contains("combo"));
            Assert.IsTrue(s1.HasGameKey("gA"));

            var s2 = UnlockEvaluator.Apply(s1, rules, catalog, ProgressionEvent.RecordingSaved("rA"));
            Assert.IsTrue(s2.Contains("combo"));
        }

        [Test]
        public void InitiallyUnlocked_Applied()
        {
            var catalog = new List<CuratedSoundDefinition>
            {
                Def("a", true),
                Def("b", false)
            };
            var state = UnlockEvaluator.ApplyInitialUnlocks(UnlockState.Empty(), catalog);
            Assert.IsTrue(state.Contains("a"));
            Assert.IsFalse(state.Contains("b"));
        }

        [Test]
        public void Project_Marks_Locked_And_Unlocked()
        {
            var catalog = new List<CuratedSoundDefinition> { Def("x"), Def("y") };
            var state = UnlockState.Empty().WithUnlocked("x");
            var items = UnlockEvaluator.Project(catalog, state);
            Assert.AreEqual(2, items.Count);
            Assert.IsTrue(items[0].isUnlocked);
            Assert.IsFalse(items[1].isUnlocked);
        }

        [Test]
        public void UnlockState_Json_RoundTrip()
        {
            var original = UnlockState.Empty()
                .WithGameKey("g1")
                .WithRecordingKey("r1")
                .WithUnlocked("s1");
            string json = JsonUtility.ToJson(original);
            var restored = JsonUtility.FromJson<UnlockState>(json);
            Assert.IsTrue(restored.Contains("s1"));
            Assert.IsTrue(restored.HasGameKey("g1"));
            Assert.IsTrue(restored.HasRecordingKey("r1"));
        }
    }
}
