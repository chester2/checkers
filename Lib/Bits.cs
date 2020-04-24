using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lib
{
    /// <summary>
    /// Functions for working in binary.
    /// </summary>
    public static class Bits
    {
        #region Lookup Tables
        private static readonly Dictionary<ulong, int> bitToIndex;
        private static readonly Dictionary<ulong, int> bitToSquareNum;
        private static readonly Dictionary<int, ulong> squareNumToBit;

        static Bits()
        {
            Bits.bitToIndex = Enumerable.Range(0, 64).ToDictionary(i => 1UL << i, i => i);

            Bits.bitToSquareNum = new Dictionary<ulong, int>();
            Bits.squareNumToBit = new Dictionary<int, ulong>();
            var j = 1;
            foreach (var square in Bits.EnumerateOccupied(0x55aa55aa55aa55aaUL))
            {
                Bits.bitToSquareNum.Add(square, j);
                Bits.squareNumToBit.Add(j, square);
                j++;
            }
        }

        public static int ToIndex(ulong square) => Bits.bitToIndex[square];
        public static ulong FromIndex(int squareIndex) => 1UL << squareIndex;

        public static int ToSquareNum(ulong square) => Bits.bitToSquareNum[square];
        public static ulong FromSquareNum(int squareNum) => Bits.squareNumToBit[squareNum];
        #endregion

        #region Motions
        private const ulong ExclLeft = 0xfefefefefefefefeUL;
        private const ulong ExclRight = 0x7f7f7f7f7f7f7f7fUL;

        private static ulong NE(ulong b) => (b & Bits.ExclRight) >> 7;
        private static ulong SE(ulong b) => (b & Bits.ExclRight) << 9;
        private static ulong SW(ulong b) => (b & Bits.ExclLeft) << 7;
        private static ulong NW(ulong b) => (b & Bits.ExclLeft) >> 9;

        public static IReadOnlyDictionary<Direction, Func<ulong, ulong>> Moves { get; }
            = new ReadOnlyDictionary<Direction, Func<ulong, ulong>>(
                new Dictionary<Direction, Func<ulong, ulong>>
                {
                    [Direction.NE] = Bits.NE,
                    [Direction.SE] = Bits.SE,
                    [Direction.SW] = Bits.SW,
                    [Direction.NW] = Bits.NW,
                });
        #endregion

        public static IEnumerable<ulong> EnumerateOccupied(ulong b)
        {
            while (b != 0)
            {
                var ls1b = b & ~(b - 1);
                yield return ls1b;
                b ^= ls1b;
            }
        }
    }
}
