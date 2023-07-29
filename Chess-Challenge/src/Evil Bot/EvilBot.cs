using ChessChallenge.API;
using System;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 350, 350, 600, 1100, 10000 };
        Random rng = new();

        public Move Think(Board board, Timer timer)
        {
            int score = Evaluate(board, 1);

            Move[] moves = board.GetLegalMoves();
            Move retMove = moves[0];

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int timeLeft = timer.MillisecondsRemaining;
                int depth = 4;
                if (timeLeft < 2000)
                {
                    depth--;
                    if (timeLeft < 1000)
                    {
                        depth--;
                        if (timeLeft < 700)
                        {
                            depth--;
                            if (timeLeft < 500)
                            {
                                depth--;
                            }
                        }
                    }
                }
                int newScore = AlphaBetaMax(board, timer, int.MinValue, int.MaxValue, depth, -1);
                board.UndoMove(move);
                if (newScore < score)
                {
                    score = newScore;
                    retMove = move;
                }
            }
            return retMove;
        }

        int Evaluate(Board board, int maximizingPlayer)
        {
            if (board.IsInCheckmate())
            {
                if (maximizingPlayer == -1) return -10000;
                else return 10000;
            }
            if (board.IsDraw()) return 0;
            int score = 0;
            PieceList[] pieceLists = board.GetAllPieceLists();
            for (int i = 0; i < 5; i++)
            {
                int val = pieceValues[i + 1];
                score += (pieceLists[i].Count - pieceLists[i + 6].Count) * val;
            }
            return score * (board.IsWhiteToMove ? 1 : -1);
        }

        int AlphaBetaMax(Board board, Timer timer, int alpha, int beta, int depth_left, int maximizingPlayer)
        {
            if (depth_left == 0 || board.IsDraw() || board.IsInCheckmate() || timer.MillisecondsRemaining == 0) return Evaluate(board, maximizingPlayer);
            Move[] moves = board.GetLegalMoves();
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int score = AlphaBetaMax(board, timer, alpha, beta, depth_left - 1, -1 * maximizingPlayer);
                board.UndoMove(move);
                if (timer.MillisecondsElapsedThisTurn > 5000) return (maximizingPlayer == 1) ? alpha : beta;
                if (maximizingPlayer == -1)
                {
                    if (score >= beta)
                    {
                        return beta;
                    }
                    if (score > alpha) alpha = score;
                }
                else
                {
                    if (score <= alpha)
                    {
                        return alpha;
                    }
                    if (score < beta) beta = score;
                }
            }
            //reutrns alpha if maximizing, returns beta if not
            return ((maximizingPlayer == 1) ? beta : alpha);
        }
    }
}
/*// Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            int thatMove = rng.Next(allMoves.Length);
            Move moveToPlay = allMoves[thatMove];

            int k = 0;
            do
            {
                thatMove = rng.Next(allMoves.Length);
                if (!board.SquareIsAttackedByOpponent(allMoves[thatMove].TargetSquare)) //something like this but with starting square for the todo
                {
                    moveToPlay = allMoves[thatMove];
                    break;
                }
                else
                {
                    k++;
                }
            }
            while (k < 1000);

            int highestValueCapture = 0;
            int highestThreatenedPieceValue = 0;

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if ((capturedPieceValue > highestValueCapture) && (capturedPieceValue > highestThreatenedPieceValue))
                {
                    if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
                    {
                        moveToPlay = move;
                        highestValueCapture = capturedPieceValue;
                    }
                }
                //ThreatenedPiece
                Piece movePiece = board.GetPiece(move.StartSquare);
                int movePieceValue = pieceValues[(int)movePiece.PieceType];

                if ((movePieceValue > highestThreatenedPieceValue) && (movePieceValue > highestValueCapture))
                {
                    if (board.SquareIsAttackedByOpponent(move.StartSquare))
                    {
                        if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
                        {
                            moveToPlay = move;
                            highestThreatenedPieceValue = movePieceValue;
                        }
                    }
                }
            }
            return moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }*/