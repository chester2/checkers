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
            Assert.AreEqual(a.ColorToMove, b.ColorToMove);
            Assert.AreEqual(a[Color.Black], b[Color.Black]);
            Assert.AreEqual(a[Color.White, PieceType.Single], b[Color.White, PieceType.Single]);
        }
    }
}
