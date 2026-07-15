using NUnit.Framework;
using Geidai.Rec;

namespace Geidai.Tests.EditMode
{
    /// <summary>
    /// RecordingClock の単体テスト（NFR-03 / U3）。
    /// 3秒固定・自動停止・残り秒・リセットの境界挙動を検証する。
    /// </summary>
    public class RecordingClockTests
    {
        [Test]
        public void Tick_Reaches_Done_At_Three_Seconds()
        {
            var clock = new RecordingClock();
            clock.Start();

            // float 累積誤差で 3.0 の到達が 30/31 回目に前後するため、
            // 完了まで tick して回数が概ね 3秒相当（29〜31 回）であることを検証する。
            bool done = false;
            int ticks = 0;
            while (!done && ticks < 100)
            {
                done = clock.Tick(0.1f);
                ticks++;
            }

            Assert.IsTrue(done, "3秒経過で完了となること");
            Assert.IsTrue(clock.IsDone);
            Assert.IsFalse(clock.IsRunning, "完了後は停止すること");
            Assert.AreEqual(RecordingClock.DurationSeconds, clock.Elapsed, 1e-4f, "Elapsed は 3.0 に丸められること");
            Assert.That(ticks, Is.InRange(29, 31), "0.1s 刻みで約3秒（29〜31 回）で完了すること");
        }

        [Test]
        public void Tick_Completes_Cleanly_When_Overshooting()
        {
            var clock = new RecordingClock();
            clock.Start();

            // 一気に 3秒を超える deltaTime を与えても Elapsed は 3.0 に丸められ完了する。
            bool done = clock.Tick(5f);

            Assert.IsTrue(done);
            Assert.IsTrue(clock.IsDone);
            Assert.IsFalse(clock.IsRunning);
            Assert.AreEqual(RecordingClock.DurationSeconds, clock.Elapsed, 1e-6f);
        }

        [Test]
        public void Not_Done_Before_Three_Seconds()
        {
            var clock = new RecordingClock();
            clock.Start();

            bool done = false;
            for (int i = 0; i < 20; i++) // 2.0s
                done = clock.Tick(0.1f);

            Assert.IsFalse(done);
            Assert.IsFalse(clock.IsDone);
            Assert.Greater(clock.RemainingSeconds, 0f);
        }

        [Test]
        public void Tick_Does_Nothing_When_Not_Running()
        {
            var clock = new RecordingClock();
            bool done = clock.Tick(1.0f);

            Assert.IsFalse(done);
            Assert.AreEqual(0f, clock.Elapsed, 1e-6f);
        }

        [Test]
        public void Reset_Clears_Elapsed_And_Running()
        {
            var clock = new RecordingClock();
            clock.Start();
            clock.Tick(1.0f);
            clock.Reset();

            Assert.AreEqual(0f, clock.Elapsed, 1e-6f);
            Assert.IsFalse(clock.IsRunning);
            Assert.AreEqual(RecordingClock.DurationSeconds, clock.RemainingSeconds, 1e-4f);
        }

        [Test]
        public void RemainingSeconds_Never_Negative()
        {
            var clock = new RecordingClock();
            clock.Start();
            clock.Tick(10f); // 大幅に超過

            Assert.AreEqual(0f, clock.RemainingSeconds, 1e-6f);
        }
    }
}
