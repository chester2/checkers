using System;

namespace Lib
{
    public struct Piece : IEquatable<Piece>
    {
        public Color Color { get; }
        public PieceType Type { get; }

        public Piece(Color color, PieceType pt)
        {
            this.Color = color;
            this.Type = pt;
        }

        public bool Equals(Piece other)
            => this.Color == other.Color && this.Type == other.Type;

        public override bool Equals(object obj)
            => this.Equals((Piece)obj);

        public override int GetHashCode()
            => this.Color.GetHashCode() - (this.Type.GetHashCode() * 10);

        public static bool operator ==(Piece self, Piece other) => self.Equals(other);
        public static bool operator !=(Piece self, Piece other) => !self.Equals(other);
    }
}
