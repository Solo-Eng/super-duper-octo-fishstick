using ChessChallenge.API;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 350, 350, 600, 900, 10000 };
    Random rng = new();

    public Move Think(Board board, Timer timer)
    {
        int score = getBoardEvaluation(board, true);
        int depth = 4;

        Move[] moves = GetMoves(board);
        Move retMove = moves[0];
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int timeLeft = timer.MillisecondsRemaining;
            depth = 4;
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
            int newScore = getScore(board, timer, depth, true, int.MinValue, int.MaxValue);
            board.UndoMove(move);
            //if the min is equal to score, that means the max is greater :)
            if (Math.Min(newScore, score) >= score)
            {
                score = newScore;
                retMove = move;
            }
        }
        depth = 4;
        //Console.WriteLine("=================================================================================");
        board.MakeMove(retMove);
        Console.WriteLine("Start: " + retMove.StartSquare + "Target: " + retMove.TargetSquare);
        getScore(board, timer, depth, true, int.MinValue, int.MaxValue);
        board.UndoMove(retMove);

        return retMove;
    }

    Move[] GetMoves(Board board)
    {
        return board.GetLegalMoves().OrderBy(x => rng.Next()).ToArray();
    }

    //beta is trying to be the lowest it can be, alpha is trying to be the biggest it can be
    //on minimizing, you try to get the lowest value, which means that you update beta
    int getScore(Board board, Timer timer, int depth, bool IsMaximizing, int alpha, int beta)
    {
        //if the board is in a draw, checkmate, or the depth has run out
        if (board.IsInCheckmate() || board.IsDraw() || depth == 0)
        {
            //evaluate the score
            //we want to know if it is the player or the opponent's turn
            return getBoardEvaluation(board, IsMaximizing);
        }
        //Console.WriteLine("---------" + depth + "---------");
        //otherwise, go another level deep
        //this means going through all of the children for this move/board state
        Move[] moves = GetMoves(board);
        //for each of the childeren, get the score
        foreach (Move move in moves)
        //for (int i = 0; i < 2; i++)
        {
            //if (timer.MillisecondsElapsedThisTurn > 5000) return IsMaximizing ? alpha : beta ;
            //update the board state for the child 
            board.MakeMove(move);
            //board.MakeMove(moves[i]);

            //Console.WriteLine("Start: " + move.StartSquare  +   "Target: " + move.TargetSquare);
            //score is whatever is gotten
            int score = getScore(board, timer, depth - 1, !IsMaximizing, alpha, beta); //this is the child
            //undo the child board state
            board.UndoMove(move);
            //board.UndoMove(moves[i]);
            //check to see how the score measures up with the highest score
            //if its the players turn, we want a high score so
            /*for (int j = 0; j < depth; j++)
            {
                Console.Write("=");
            }*/
            if (IsMaximizing)
            {
                //alpha beta pruning
                //if the this is maximizing, that means it wants to get the highest value possible, which means updating alpha
                //if alpha is greater than beta, beta being the lowest value possible, which is the branch that the minimizing would choose
                //then we can just return beta because any higher values found will not matter
                if (score >= beta)
                {
                    //purge the result and go to the next possible move
                    //Console.WriteLine(beta);
                    return beta;
                }
                //otherwise, the alpha is not greater than the beta, which means that there is a possible new best branch
                //so 
                if (score > alpha) alpha = score;
            }
            //otherwise we want the lowest score possible
            else
            {
                //alpha beta pruning
                //if the this is minimizing, that means it wants to get the lowest value possible, which means updating beta
                //if beta is less than alpha, we want to update beta to be a new low
                if (score <= alpha)
                {
                    //purge the result and go to the next possible move
                    //Console.WriteLine(alpha);
                    return alpha;
                }

                if (score < beta) beta = score;
            }
        }
        //if maximizing, return highest score, if minimizing, return lowest score
        //Console.WriteLine((IsMaximizing ? alpha : beta));
        return IsMaximizing ? alpha : beta;
    }
    int getBoardEvaluation(Board board, bool IsMaximizing)
    {
        //check if the board is in checkmate
        if (board.IsInCheckmate())
        {
            if (IsMaximizing) return int.MaxValue;
            else return int.MinValue;
        }
        if (board.IsDraw())
        {
            return 0;
        }
        int score = 0;

        //get the score by evaluating the pieces on the board
        PieceList[] pieceLists = board.GetAllPieceLists();
        for (int i = 0; i < 5; i++)
        {
            int val = pieceValues[i + 1];
            //the pieceList always starts with the white pieces and then does the black pieces
            score += (pieceLists[i].Count - pieceLists[i + 6].Count) * val;
            //so if the board is not whites turn, then the score will be inverted
            //since we want the score in the eyes of whoever's turn it is
            score = board.IsWhiteToMove ? score : -score;
        }

        //if it is the players turn we want the score to be positive
        //otherwise it is negative
        //Console.WriteLine((IsMaximizing ? score : -score));
        return IsMaximizing ? -score : score;
    }
}

