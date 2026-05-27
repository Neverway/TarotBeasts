//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles scoring tiles, game-over states, and valid placements
/// </summary>
public interface IRuleset
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    string DisplayName { get; }


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    TileScoringResult ScoreTile(BoardState boardState, int tileIndex);

    bool IsPieceUpgraded(BoardState boardState, int tileIndex);

    GameOverResult CheckGameOver(BoardState boardState, int playerCount, int[] scores);

    IReadOnlyList<int> GetValidPlacements(BoardState boardState, int currentPlayerSlot);

    IReadOnlyList<int> GetValidPieceTypes(BoardState boardState, int currentPlayerSlot);


    #endregion
}

public struct TileScoringResult
{
    // Total score points this tile contributes to its owning player
    public int Score;
    // Which scoring arrows for the tile should be lit
    // Order is 0-Up 1-Down 2-Left 3-Right 4-UpLeft 5-UpRight 6-DownRight 7-DownLeft
    public bool[] ActiveArrows;
    // If this tile is currently upgraded
    public bool IsUpgraded;
}

public class GameOverResult
{
    // Turn slot index of the winner player, or -1 if the match was a tie
    public int WinnerSlot;
    public bool IsTie => WinnerSlot < 0;
    public int[] FinalScores;
    public int MoneyMatchBounty;

    public GameOverResult(int winnerSlot, int[] finalScores)
    {
        WinnerSlot = winnerSlot;
        FinalScores = finalScores;
        MoneyMatchBounty = 0;
    }
}