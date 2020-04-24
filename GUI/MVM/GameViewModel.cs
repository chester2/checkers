using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Lib;

namespace GUI
{
    /// <summary>
    /// Main view model for the game window.
    /// </summary>
    public class GameViewModel : Observable
    {
        #region Fields and Properties
        private Bitboard bitboard;

        private ulong activePiece;
        private ulong ActivePiece
        {
            get => this.activePiece;
            set
            {
                if (this.activePiece != 0)
                {
                    this.UISquares[Bits.ToIndex(this.activePiece)].Focus = false;
                }
                this.activePiece = value;
                if (this.activePiece != 0)
                {
                    this.UISquares[Bits.ToIndex(this.activePiece)].Focus = true;
                }
            }
        }

        private List<Move> validMoves;
        public bool IsMoveStartingPoint(int squareIndex)
            => this.validMoves.Any(m => m.From == Bits.FromIndex(squareIndex));

        public string WhoseTurn { get; private set; }
        private void SetWhoseTurn()
        {
            this.WhoseTurn = $"{this.bitboard.ColorToMove}'s Turn";
            this.RaisePropertyChanged(nameof(this.WhoseTurn));
        }

        private ObservableCollection<Square> uiSquares;
        public ObservableCollection<Square> UISquares
        {
            get => this.uiSquares;
            set => this.SetProperty(ref this.uiSquares, value, nameof(this.UISquares));
        }
        private void DrawSquares()
        {
            this.UISquares = new ObservableCollection<Square>();
            var pieceMap = bitboard.GetPieceMap();

            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var squareColor = ((x + y) % 2 == 0) ? SquareColor.Light : SquareColor.Dark;
                    this.UISquares.Add(
                        pieceMap.TryGetValue(Bits.FromIndex(x + y * 8), out var piece)
                        ? new Square(squareColor) { State = SquareState.Occupied, Piece = piece }
                        : new Square(squareColor) { State = SquareState.Empty });
                }
            }
        }

        public string WinnerText { get; private set; }
        private void SetWinnerText(Lib.Color winner)
        {
            this.WinnerText = $"{winner} won!";
            this.RaisePropertyChanged(nameof(this.WinnerText));
        }

        private Visibility winnerPopupVisibility;
        public Visibility WinnerPopupVisibility
        {
            get => this.winnerPopupVisibility;
            set => this.SetProperty(ref this.winnerPopupVisibility, value, nameof(this.WinnerPopupVisibility));
        }


        private Visibility restartPopupVisibility;
        public Visibility RestartPopupVisibility
        {
            get => this.restartPopupVisibility;
            set => this.SetProperty(ref this.restartPopupVisibility, value, nameof(this.RestartPopupVisibility));
        }
        #endregion

        public GameViewModel()
        {
            this.NewGame();
        }

        public void NewGame()
        {
            this.WinnerPopupVisibility = Visibility.Hidden;
            this.RestartPopupVisibility = Visibility.Hidden;

            this.bitboard = new Bitboard();
            this.DrawSquares();
            this.SetWhoseTurn();
            this.ActivePiece = 0;
            this.validMoves = this.bitboard.GetMoves();
        }

        #region Mouse Events
        public void HandleSquareMouseUp(int squareIndex)
        {
            // Do nothing if popups are visible.
            if (this.WinnerPopupVisibility == Visibility.Visible
                || this.RestartPopupVisibility == Visibility.Visible)
            {
                return;
            }

            var square = Bits.FromIndex(squareIndex);
            List<Move> possibleMoves;

            // No active piece: make clicked piece active if it has valid moves.
            if (this.ActivePiece == 0)
            {
                possibleMoves = this.validMoves.Where(m => m.From == square).ToList();
                if (possibleMoves.Count > 0)
                {
                    this.ActivePiece = square;
                    foreach (var possibleMove in possibleMoves)
                    {
                        this.UISquares[Bits.ToIndex(possibleMove.To)].State = SquareState.Destination;
                    }
                }
                return;
            }

            // Filter valid moves for those whose "from" field is the clicked square.
            possibleMoves = this.validMoves
                .Where(m => (m.From == this.ActivePiece && m.To == square))
                .ToList();

            // Invalid destination for active piece: make active piece inactive and clear destinations from board.
            if (possibleMoves.Count == 0)
            {
                this.ActivePiece = 0;
                foreach (var uiSquare in this.UISquares)
                {
                    if (uiSquare.State != SquareState.Occupied)
                    {
                        uiSquare.State = SquareState.Empty;
                    }
                }

                // If the clicked square is a piece belonging to the current side, make that piece the new active piece.
                this.HandleSquareMouseUp(squareIndex);
                return;
            }

            // Apply move and update UI.
            var move = possibleMoves[0];
            var followupMoves = bitboard.ApplyMove(move);
            this.ActivePiece = 0;
            this.DrawSquares();

            // Multijump followup.
            if (followupMoves.Count > 0)
            {
                this.validMoves = followupMoves;
                this.HandleSquareMouseUp(Bits.ToIndex(move.To));
                return;
            }

            // Not jump or no multijump followups.
            this.SetWhoseTurn();
            this.validMoves = this.bitboard.GetMoves();
            if (this.validMoves.Count == 0)
            {
                this.SetWinnerText(this.bitboard.ColorToMove.Flip());
                this.WinnerPopupVisibility = Visibility.Visible;
            }
        }

        public void HandleSquareMouseEnter(int squareIndex)
        {
            if (this.WinnerPopupVisibility == Visibility.Visible
                || this.RestartPopupVisibility == Visibility.Visible)
            {
                return;
            }

            if (this.validMoves.Any(m => m.From == Bits.FromIndex(squareIndex)))
            {
                this.UISquares[squareIndex].Focus = true;
            }
        }

        public void HandleSquareMouseLeave(int squareIndex)
        {
            if (this.WinnerPopupVisibility == Visibility.Visible
                || this.RestartPopupVisibility == Visibility.Visible)
            {
                return;
            }

            if (Bits.FromIndex(squareIndex) != this.ActivePiece)
            {
                this.UISquares[squareIndex].Focus = false;
            }
        }
        #endregion

        #region Button Events
        public ICommand RestartCommand
        {
            get => new RelayCommand((parameter) => this.RestartPopupVisibility = Visibility.Visible);
        }

        public ICommand RestartPopupNoCommand
        {
            get => new RelayCommand((parameter) => this.RestartPopupVisibility = Visibility.Hidden);
        }

        public ICommand RestartPopupYesCommand
        {
            get
            {
                return new RelayCommand(
                    (parameter) =>
                    {
                        this.WinnerPopupVisibility = Visibility.Hidden;
                        this.RestartPopupVisibility = Visibility.Hidden;
                        this.NewGame();
                    }
                );
            }
        }
        #endregion
    }
}