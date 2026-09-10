using NUnit.Framework;
using Geidai.Game3;

namespace Geidai.Tests
{
    public class PitchOrderJudgeTests
    {
        [Test]
        public void IsAscending_LowToHigh_ReturnsTrue()
        {
            int[] cents = { -300, -100, 200, 500 };

            bool result = PitchOrderJudge.IsAscending(cents);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsAscending_WrongOrder_ReturnsFalse()
        {
            int[] cents = { -300, 200, -100, 500 };

            bool result = PitchOrderJudge.IsAscending(cents);

            Assert.IsFalse(result);
        }

        [Test]
        public void IsDescending_HighToLow_ReturnsTrue()
        {
            int[] cents = { 500, 200, -100, -300 };

            bool result = PitchOrderJudge.IsDescending(cents);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsDescending_WrongOrder_ReturnsFalse()
        {
            int[] cents = { 500, -100, 200, -300 };

            bool result = PitchOrderJudge.IsDescending(cents);

            Assert.IsFalse(result);
        }
    }
}
