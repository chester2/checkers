using Lib;

namespace GUI
{
    public enum SquareColor
    {
        Dark,
        Light,
    }

    public enum SquareState
    {
        Empty,
        Destination,
        Occupied,
    }

    /// <summary>
    /// Model representing a board square.
    /// </summary>
    public class Square: Observable
    {
        public Square(SquareColor color) => this.SquareColor = color;

        public SquareColor SquareColor { get; }

        private SquareState state;
        public SquareState State
        {
            get => this.state;
            set => this.SetProperty(ref this.state, value, nameof(this.State));
        }

        private Piece piece;
        public Piece Piece
        {
            get => this.piece;
            set
            {
                this.SetProperty(ref this.piece, value, nameof(this.Piece.Color));
                this.RaisePropertyChanged(nameof(this.Piece.Type));
            }
        }

        private bool focus;
        public bool Focus
        {
            get => this.focus;
            set => this.SetProperty(ref this.focus, value, nameof(this.Focus));
        }
    }
}
