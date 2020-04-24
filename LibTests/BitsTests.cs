using NUnit.Framework;
using Lib;

namespace LibTests
{
    [TestFixture]
    public class BitsTests
    {
        [TestCase(1UL, 0)]
        [TestCase(1UL << 30, 30)]
        [TestCase(1UL << 63, 63)]
        public void ToFromIndex(ulong square, int index)
        {
            Assert.AreEqual(index, Bits.ToIndex(square));
            Assert.AreEqual(square, Bits.FromIndex(index));
        }

        [TestCase(1UL << 1, 1)]
        [TestCase(1UL << 3, 2)]
        [TestCase(1UL << 10, 6)]
        [TestCase(1UL << 62, 32)]
        public void ToFromSquareNum(ulong square, int squareNum)
        {
            Assert.AreEqual(squareNum, Bits.ToSquareNum(square));
            Assert.AreEqual(square, Bits.FromSquareNum(squareNum));
        }

        [TestCase(1UL, 0UL)]
        [TestCase(1UL << 7, 0UL)]
        [TestCase(1UL << 56, 1UL << 49)]
        [TestCase(1UL << 63, 0UL)]
        [TestCase(1UL << 30, 1UL << 23)]
        public void NE(ulong board, ulong expected)
            => Assert.AreEqual(expected, Bits.Moves[Direction.NE](board));

        [TestCase(1UL, 1UL << 9)]
        [TestCase(1UL << 7, 0UL)]
        [TestCase(1UL << 56, 0UL)]
        [TestCase(1UL << 63, 0UL)]
        [TestCase(1UL << 30, 1UL << 39)]
        public void SE(ulong board, ulong expected)
            => Assert.AreEqual(expected, Bits.Moves[Direction.SE](board));

        [TestCase(1UL, 0UL)]
        [TestCase(1UL << 7, 1UL << 14)]
        [TestCase(1UL << 56, 0UL)]
        [TestCase(1UL << 63, 0UL)]
        [TestCase(1UL << 30, 1UL << 37)]
        public void SW(ulong board, ulong expected)
            => Assert.AreEqual(expected, Bits.Moves[Direction.SW](board));

        [TestCase(1UL, 0UL)]
        [TestCase(1UL << 7, 0UL)]
        [TestCase(1UL << 56, 0UL)]
        [TestCase(1UL << 63, 1UL << 54)]
        [TestCase(1UL << 30, 1UL << 21)]
        public void NW(ulong board, ulong expected)
            => Assert.AreEqual(expected, Bits.Moves[Direction.NW](board));
    }
}
