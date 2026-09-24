using System.Collections.Generic;

namespace Geidai.Game3
{
    public class PitchOrderQuestion
    {
        public PitchOrderDirection Direction { get; }
        public IReadOnlyList<int> InitialOrder { get; }

        public PitchOrderQuestion(
            PitchOrderDirection direction,
            IReadOnlyList<int> initialOrder)
        {
            Direction = direction;
            InitialOrder = initialOrder;
        }
    }
}