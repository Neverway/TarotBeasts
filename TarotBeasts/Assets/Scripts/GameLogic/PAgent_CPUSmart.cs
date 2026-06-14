//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PAgent_CPUSmart : IPlayerAgent
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public PlayerProfile Profile { get; }
    public event Action<int, int> OnMoveChosen;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private MonoBehaviour coroutineHost;
    private int round;
    private GameBoard gameBoard;
    private Coroutine thinkRoutine;



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public PAgent_CPUSmart(PlayerProfile _profile, MonoBehaviour _coroutineHost, int _round)
    {
        Profile = _profile;
        coroutineHost = _coroutineHost;
        round = _round;
        gameBoard = _coroutineHost as GameBoard;
    }

    private IEnumerator Think(BoardState state, IRuleset ruleset, int mySlot)
    {
        yield return new WaitForSeconds(ThinkTime());

        var validTiles  = ruleset.GetValidPlacements(state, mySlot);
        var validPieces = ruleset.GetValidPieceTypes(state, mySlot);

        if (validTiles.Count == 0 || validPieces.Count == 0) yield break;

        int tile, piece;

        // Easy cpu (picks a random tile) reminder, this is  only in effect until r3
        if (round <= 2)
        {
            // Easy — fully random
            tile  = validTiles [UnityEngine.Random.Range(0, validTiles.Count)];
            piece = validPieces[UnityEngine.Random.Range(0, validPieces.Count)];
        }
        // Medium cpu (best tile for this turn) reminder, this is only in effect until r5
        else if (round <= 4)
        {
            // Medium — pick the (tile, piece) combo that scores highest for the CPU right now
            (tile, piece) = BestGreedyMove(state, ruleset, mySlot, validTiles, validPieces);
        }
        // Hard cpu (think 1 move ahead) reminder, this is only in effect until rINFINITY
        else
        {
            (tile, piece) = BestLookaheadMove(state, ruleset, mySlot, validTiles, validPieces);
        }
        
        
        OnMoveChosen?.Invoke(tile, piece);
        gameBoard?.NotifyCPUPlacedAt(tile);
    }

    private (int tile, int piece) BestGreedyMove(BoardState state, IRuleset ruleset, int mySlot, IReadOnlyList<int> tiles, IReadOnlyList<int> pieces)
    {
        int bestTile = tiles[0], bestPiece = pieces[0], bestScore = int.MinValue;

        foreach (int t in tiles)
        {
            foreach (int p in pieces)
            {
                var sim = SimulatePlace(state, t, mySlot + 1, p);
                int score = ScoreForPlayer(sim, ruleset, mySlot);
                if (score > bestScore) { bestScore = score; bestTile = t; bestPiece = p; }
            }
        }
        return (bestTile, bestPiece);
    }

    private (int tile, int piece) BestLookaheadMove(BoardState state, IRuleset ruleset, int mySlot, IReadOnlyList<int> tiles, IReadOnlyList<int> pieces)
    {
        int bestTile = tiles[0], bestPiece = pieces[0];
        int bestDelta = int.MinValue;
        int playerCount = mySlot + 2;

        foreach (int t in tiles)
        {
            foreach (int p in pieces)
            {
                var sim = SimulatePlace(state, t, mySlot + 1, p);
                int myScore = ScoreForPlayer(sim, ruleset, mySlot);
                int oppScore = 0;
                for (int s = 0; s < playerCount; s++)
                {
                    if (s != mySlot) oppScore += ScoreForPlayer(sim, ruleset, s);
                }

                int delta = myScore - oppScore;
                if (delta > bestDelta) { bestDelta = delta; bestTile = t; bestPiece = p; }
            }
        }
        return (bestTile, bestPiece);
    }

    private static BoardState SimulatePlace(BoardState original, int tileIndex, int playerSlot, int pieceType)
    {
        var sim = new BoardState(original.width, original.height);
        for (int i = 0; i < original.tileCount; i++)
        {
            sim.Tiles[i] = original.Tiles[i];
        }

        sim.PlacePiece(tileIndex, playerSlot, pieceType);
        return sim;
    }

    private static int ScoreForPlayer(BoardState state, IRuleset ruleset, int slot)
    {
        int total = 0;
        for (int i = 0; i < state.tileCount; i++)
        {
            if (state.Tiles[i].player == slot + 1)
            {
                total += ruleset.ScoreTile(state, i).Score;
            }
        }
        return total;
    }
    
    private float ThinkTime()
    {
        var value = Mathf.Max(0.4f, 1.0f - (round - 1) * 0.1f);
        return value;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void BeginTurn(BoardState state, IRuleset ruleset, int mySlot)
    {
        thinkRoutine = coroutineHost.StartCoroutine(Think(state, ruleset, mySlot));
    }

    public void CancelTurn()
    {
        if (thinkRoutine != null) coroutineHost.StopCoroutine(thinkRoutine);
    }

    #endregion
}
