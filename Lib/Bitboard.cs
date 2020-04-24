using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Lib
{
    public class Bitboard
    {
        #region Fields and Properties
        public static IReadOnlyDictionary<Color, ulong> KingsRows { get; }
            = new ReadOnlyDictionary<Color, ulong>(
                new Dictionary<Color, ulong>
                {
                    [Color.Black] = 0xffUL,
                    [Color.White] = 0xffUL << 8 * 7,
                });

        /// <summary>
        /// Return a dictionary mapping occupied squares to piece information.
        /// </summary>
        public IReadOnlyDictionary<ulong, Piece> GetPieceMap()
        {
            var map = new Dictionary<ulong, Piece>();
            foreach (var kvpair in this.boards)
            {
                var color = kvpair.Key.Color;
                var pt = kvpair.Key.Type;
                var board = kvpair.Value;
                foreach (var square in Bits.EnumerateOccupied(board))
                {
                    map.Add(square, new Piece(color, pt));
                }
            }
            return map;
        }

        private readonly Dictionary<Piece, ulong> boards = new Dictionary<Piece, ulong>
        {
            [new Piece(Color.Black, PieceType.Single)] = 0UL,
            [new Piece(Color.Black, PieceType.Double)] = 0UL,
            [new Piece(Color.White, PieceType.Single)] = 0UL,
            [new Piece(Color.White, PieceType.Double)] = 0UL,
        };

        public Color ColorToMove { get; private set; }
        public ulong Occupied { get => this[Color.Black] | this[Color.White]; }
        #endregion

        #region Constructors
        public Bitboard()
        {
            this.ColorToMove = Color.Black;
            this[Color.Black, PieceType.Single] = 0xaa55aaUL;
            this[Color.White, PieceType.Single] = 0x55aa550000000000UL;
        }

        private Bitboard(Bitboard bitboard)
        {
            this.ColorToMove = bitboard.ColorToMove;
            foreach (var color in Util.EnumerateEnum<Color>())
            {
                foreach (var pt in Util.EnumerateEnum<PieceType>())
                {
                    this[color, pt] = bitboard[color, pt];
                }
            }
        }

        public Bitboard DeepCopy()
        {
            return new Bitboard(this);
        }

        public Bitboard(string fen)
        {
            this.ColorToMove = (fen[0] == Color.Black.ToChar())
               ? Color.Black
               : Color.White;

            var fenParts = fen.Split(':');
            for (var i = 1; i < fenParts.Length; i++)
            {
                var color = (fenParts[i][0] == Color.Black.ToChar()) ? Color.Black : Color.White;
                var colorSectionParts = fenParts[i].Substring(1).Split(',');
                foreach (var squareNumStr in colorSectionParts)
                {
                    var (pt, squareNum) = (squareNumStr[0] == 'K')
                        ? (PieceType.Double, Convert.ToInt32(squareNumStr.Substring(1)))
                        : (PieceType.Single, Convert.ToInt32(squareNumStr));
                    this[color, pt] |= Bits.FromSquareNum(squareNum);
                }
            }
        }

        public string ToFEN()
        {
            var sb = new StringBuilder(this.ColorToMove.ToChar().ToString());

            foreach (var color in Util.EnumerateEnum<Color>())
            {
                sb.Append(":");
                sb.Append(color.ToChar());
                foreach (var square in Bits.EnumerateOccupied(this[color, PieceType.Single]))
                {
                    sb.Append(Bits.ToSquareNum(square));
                    sb.Append(',');
                }
                foreach (var square in Bits.EnumerateOccupied(this[color, PieceType.Double]))
                {
                    sb.Append(Bits.ToSquareNum(square));
                    sb.Append(',');
                }
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
        #endregion

        public ulong Perft(int depth) => this.Perft(depth, null);
        private ulong Perft(int depth, List<Move> followups)
        {
            if (depth == 0) return 1;

            var moves = (followups != null && followups.Count > 0) ? followups : this.GetMoves();
            if (depth == 1) return (ulong)moves.Count;

            var _nodes = 0UL;
            foreach (var move in moves)
            {
                var nextbb = this.DeepCopy();
                followups = nextbb.ApplyMove(move);
                if (followups.Count > 0)
                {
                    _nodes += nextbb.Perft(depth, followups);
                }
                else
                {
                    _nodes += nextbb.Perft(depth - 1, null);
                }
            }
            return _nodes;
        }

        #region Indexers
        public ulong this[Piece piece]
        {
            get => this.boards[piece];
            private set => this.boards[piece] = value;
        }

        public ulong this[Color color, PieceType pt]
        {
            get => this[new Piece(color, pt)];
            private set => this[new Piece(color, pt)] = value;
        }

        public ulong this[Color color]
        {
            get => this[color, PieceType.Single] | this[color, PieceType.Double];
        }
        #endregion

        #region Movegen
        private static readonly Dictionary<Color, Direction[]> directions
            = new Dictionary<Color, Direction[]>
            {
                [Color.Black] = new[] { Direction.SE, Direction.SW },
                [Color.White] = new[] { Direction.NE, Direction.NW },
            };

        /// <summary>
        /// Lookup table mapping each (Direction, Square) pair to the squares that are 1 and 2 steps away.
        /// </summary>
        private static readonly Dictionary<(Direction, ulong), (ulong OneAway, ulong TwoAway)> moveTargets;
        static Bitboard()
        {
            Bitboard.moveTargets = new Dictionary<(Direction, ulong), (ulong, ulong)>();
            foreach (var direction in Util.EnumerateEnum<Direction>())
            {
                var moveFunc = Bits.Moves[direction];
                for (var i = 0; i < 64; i++)
                {
                    var square = Bits.FromIndex(i);
                    Bitboard.moveTargets.Add(
                        (direction, square),
                        (moveFunc(square), moveFunc(moveFunc(square))));
                }
            }
        }

        /// <summary>
        /// For the given side, return a list of all possible non-capturing moves. Return an empty list if no moves are possible.
        /// </summary>
        private List<Move> GetQuiets(Color color)
        {
            var moves = new List<Move>();
            foreach (var square in Bits.EnumerateOccupied(this[color, PieceType.Single]))
            {
                foreach (var direction in Bitboard.directions[color])
                {
                    var oneAway = Bitboard.moveTargets[(direction, square)].OneAway;
                    if (oneAway != 0 && (oneAway & this.Occupied) == 0)
                    {
                        moves.Add(new Move(square, oneAway));
                    }
                }
            }
            foreach (var square in Bits.EnumerateOccupied(this[color, PieceType.Double]))
            {
                foreach (var direction in Util.EnumerateEnum<Direction>())
                {
                    var oneAway = Bitboard.moveTargets[(direction, square)].OneAway;
                    if (oneAway != 0 && (oneAway & this.Occupied) == 0)
                    {
                        moves.Add(new Move(square, oneAway));
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// For the given side, return a list of all possible capturing moves. Return an empty list if no moves are possible.
        /// </summary>
        private List<Move> GetJumps(Color color)
        {
            var moves = new List<Move>();
            foreach (var square in Bits.EnumerateOccupied(this[color, PieceType.Single]))
            {
                foreach (var direction in Bitboard.directions[color])
                {
                    var (oneAway, twoAway) = Bitboard.moveTargets[(direction, square)];
                    if ((oneAway & this[color.Flip()]) != 0
                        && twoAway != 0
                        && (twoAway & this.Occupied) == 0)
                    {
                        moves.Add(new Move(square, twoAway, oneAway));
                    }
                }
            }
            foreach (var square in Bits.EnumerateOccupied(this[color, PieceType.Double]))
            {
                foreach (var direction in Util.EnumerateEnum<Direction>())
                {
                    var (oneAway, twoAway) = Bitboard.moveTargets[(direction, square)];
                    if ((oneAway & this[color.Flip()]) != 0
                        && twoAway != 0
                        && (twoAway & this.Occupied) == 0)
                    {
                        moves.Add(new Move(square, twoAway, oneAway));
                    }
                }
            }
            return moves;
        }
        #endregion

        #region Handle Moves
        /// <summary>
        /// For the given color, return all possible moves.
        /// </summary>
        private List<Move> GetMoves(Color color)
        {
            var moves = this.GetJumps(color);
            return (moves.Count > 0) ? moves : this.GetQuiets(color);
        }

        /// <summary>
        /// Return all possible moves for the current color.
        /// </summary>
        public List<Move> GetMoves() => this.GetMoves(this.ColorToMove);

        /// <summary>
        /// Apply a move. The given move must be an element of the list returned by <c>GetMoves()</c>. If the move is a jump and further jumps are possible, return a list of possible followup moves. Return an empty list otherwise or if the given move is not a jump.
        /// </summary>
        public List<Move> ApplyMove(Move move)
        {
            var pieceMap = this.GetPieceMap();
            var from = pieceMap[move.From];
            this[from.Color, from.Type] ^= move.From;

            var opponentColor = from.Color.Flip();
            var captures = move.Capture != 0;
            if (captures)
            {
                this[opponentColor, pieceMap[move.Capture].Type] ^= move.Capture;
            }

            var promoted =
                from.Type == PieceType.Single
                && (move.To & Bitboard.KingsRows[opponentColor]) != 0;
            this[from.Color, promoted ? PieceType.Double : from.Type] ^= move.To;

            if (promoted || !captures)
            {
                this.ColorToMove = opponentColor;
                return new List<Move>(0);
            }

            var jumpFollowup = this
                .GetMoves(from.Color)
                .Where(m => (m.From == move.To && m.Capture != 0))
                .ToList();
            if (jumpFollowup.Count == 0)
            {
                this.ColorToMove = opponentColor;
            }
            return jumpFollowup;
        }
        #endregion
    }
}
