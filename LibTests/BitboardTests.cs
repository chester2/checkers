using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Lib;

namespace LibTests
{
    [TestFixture]
    public class BitboardTests
    {
        [Test]
        public void Constructors()
        {
            var a = new Bitboard();
            var b = new Bitboard(a.ToFEN());
            var c = a.DeepCopy();
            Assert.AreEqual(a.ColorToMove, b.ColorToMove);
            Assert.AreEqual(b[Color.Black], c[Color.Black]);
            Assert.AreEqual(a[Color.White, PieceType.Single], c[Color.White, PieceType.Single]);
        }

        [TestCase(0, 1UL)]
        [TestCase(1, 7UL)]
        [TestCase(2, 49UL)]
        [TestCase(3, 302UL)]
        [TestCase(4, 1469UL)]
        [TestCase(5, 7361UL)]
        [TestCase(6, 36768UL)]
        [TestCase(7, 179740UL)]
        [TestCase(8, 845931UL)] // starts to fail here
        public void Perft(int depth, ulong nodes)
        {
            Assert.AreEqual(nodes, new Bitboard().Perft(depth));
        }
    }
}
