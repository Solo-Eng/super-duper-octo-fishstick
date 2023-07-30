using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 350, 350, 600, 900, 10000 };
    Random rng = new();

    public Move Think(Board board, Timer timer)
    {
        int score = getBoardEvaluation(board, true);


        Move[] moves = GetMoves(board);
        Move retMove = moves[0];
        Console.WriteLine("----------------+"+score+"+----------------");
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
            Console.WriteLine("TOP Start: " + move.StartSquare + "Target: " + move.TargetSquare);
            int newScore = getScore(board, timer, depth, false, -1000000);
            board.UndoMove(move);
            //if the min is equal to score, that means the max is greater :)
            if (Math.Min(newScore, score) >= score)
            {
                score = newScore;
                retMove = move;
            }
        }
        return retMove;
    }

    Move[] GetMoves(Board board)
    {
        return board.GetLegalMoves().OrderBy(x => rng.Next()).ToArray();
    }

    int getScore(Board board, Timer timer, int depth, bool playersTurn, int bestScore)
    {
        //if the board is in a draw, checkmate, or the depth has run out
        if (board.IsInCheckmate() || board.IsDraw() || depth == 0)
        {
            //evaluate the score
            //we want to know if it is the player or the opponent's turn
            return getBoardEvaluation(board, playersTurn);
        }

        Console.WriteLine("-------------------------" + depth + "------------------------");
        //otherwise, go another level deep
        //this means going through all of the children for this move/board state
        Move[] moves = GetMoves(board);
        //for each of the childeren, get the score
        foreach (Move move in moves)
        {
            if (timer.MillisecondsElapsedThisTurn > 5000) return bestScore;
            //update the board state for the child 
            board.MakeMove(move);
            //score is whatever is gotten
            Console.WriteLine("Start: " + move.StartSquare + "Target: " + move.TargetSquare);
            int score = getScore(board, timer, depth - 1, !playersTurn, -bestScore); //this is the child
            Console.WriteLine("Score: " + score);
            //undo the child board state
            board.UndoMove(move);
            //check to see how the score measures up with the highest score
            //if its the players turn, we want a high score so
            if (playersTurn)
            {
                //alpha beta pruning
                //if it is the players turn, that means the previous player was the opponent
                //they want the smallest score possible
                //so if the score is greater than or equal to the best score 
                //then they won't go down this path
                //so if bestScore is less than or equal to score
                Console.WriteLine(playersTurn + "" + bestScore + "" + score);
                if (bestScore < score)
                {
                    //purge the result and go to the next possible move
                    return -bestScore;
                }
                //but if it's less, then the opponent will want to go down this path so...
                //set the new best score to be this score
                Console.WriteLine(playersTurn + "" + Math.Min(bestScore, score));
                bestScore = Math.Min(score, bestScore);
            }
            //otherwise we want the lowest score possible
            else
            {
                //if it is not the players turn, that means the previous player was
                //so they won't go down this path unless the score is greater than whan they already have
                //so if the score is less than the best score they won't go down this path
                if (score < bestScore)
                {
                    return -bestScore;
                }
                //but if it is more, then they will
                //set the new best score to be this score
                Console.WriteLine(playersTurn + "" + Math.Min(bestScore, score));
                bestScore = Math.Max(score, bestScore);
            }
        }
        //now we want to return the bestScore for whoever's turn it is
        return -bestScore;
    }
    int getBoardEvaluation(Board board, bool playersTurn)
    {
        //check if the board is in checkmate
        if (board.IsInCheckmate())
        {
            if (playersTurn) return -10000;
            else return 10000;
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
        return playersTurn ? score : -score;
    }
}

