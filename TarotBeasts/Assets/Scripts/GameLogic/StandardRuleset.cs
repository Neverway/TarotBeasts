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
/// The basic ruleset for the RPS WFR gameplay
/// </summary>
public class StandardRuleset : IRuleset
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public string DisplayName => "Standard";
    
    public const int Empty = 0, Wolf = 1, Fox = 2, Rabbit = 3;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    /*__________[ Checks ]__________*/
    public bool IsPieceUpgraded(BoardState state, int tileIndex)
    {
        var tile = state.Tiles[tileIndex];
        if (tile.player == 0) return false;
 
        Vector2Int pos = state.IndexToGrid(tileIndex);
        Vector2Int[] neighbors = { pos + Vector2Int.up, pos + Vector2Int.down, pos + Vector2Int.left, pos + Vector2Int.right };
 
        foreach (var adjacent in neighbors)
        {
            if (!state.IsInBounds(adjacent)) continue;
            var neighbor = state.Tiles[state.GridToIndex(adjacent)];
            if (neighbor.player == tile.player && Beats(tile, neighbor)) return true;
        }
        return false;
    }
    
    private int GetPointsAgainst(BoardTileData tile, BoardTileData other, bool thisUpgraded, bool otherUpgraded)
    {
        if (other.player == 0) return 0;
 
        if (other.player == tile.player) return 0;
 
        // SU rabbit negates fox scoring
        if (tile.piece == Fox && other.piece == Rabbit && otherUpgraded && !thisUpgraded)
            return 0;
 
        // SU rabbit scores on fox
        if (thisUpgraded && tile.piece == Rabbit && other.piece == Fox && !otherUpgraded)
            return 1;
 
        return Beats(tile, other) ? 1 : 0;
    }
 
    private bool Beats(BoardTileData tile, BoardTileData other)
    {
        if (tile.piece == Wolf && other.piece == Fox) return true;
        if (tile.piece == Fox && other.piece == Rabbit) return true;
        if (tile.piece == Rabbit && other.piece == Wolf) return true;
        return false;
    }

    
    /*__________[ Scoring ]__________*/
    public TileScoringResult ScoreTile(BoardState state, int tileIndex)
    {
        var result = new TileScoringResult { ActiveArrows = new bool[8] };
 
        var currentTile = state.Tiles[tileIndex];
        if (currentTile.player == 0) return result; // empty tile scores nothing
 
        bool upgraded = IsPieceUpgraded(state, tileIndex);
        result.IsUpgraded = upgraded;
 
        Vector2Int position = state.IndexToGrid(tileIndex);
 
        // SU fox checks diagonals too
        Vector2Int[] scoringDirs = (upgraded && currentTile.piece == Fox)
            ? new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up+Vector2Int.right, Vector2Int.up+Vector2Int.left, Vector2Int.down+Vector2Int.right, Vector2Int.down+Vector2Int.left }
            : new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
 
        // All 8 directions in arrow index order
        Vector2Int[] allDirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up+Vector2Int.left, Vector2Int.up+Vector2Int.right, Vector2Int.down+Vector2Int.right, Vector2Int.down+Vector2Int.left };
 
        var scoringSet = new HashSet<Vector2Int>(scoringDirs);
        int totalScore = 0;
 
        for (int d = 0; d < allDirs.Length; d++)
        {
            Vector2Int dir = position + allDirs[d];
            bool inScoringRange = scoringSet.Contains(allDirs[d]);
 
            if (!inScoringRange || !state.IsInBounds(dir))
            {
                result.ActiveArrows[d] = false;
                continue;
            }
 
            int otherIndex = state.GridToIndex(dir);
            bool otherUpgraded = IsPieceUpgraded(state, otherIndex);
            int points = GetPointsAgainst(currentTile, state.Tiles[otherIndex], upgraded, otherUpgraded);
            result.ActiveArrows[d] = points > 0;
            totalScore += points;
        }
 
        // SU wolf bonus +1 per adjacent friendly upgraded wolf
        if (upgraded && currentTile.piece == Wolf)
        {
            Vector2Int[] cardinals = { position+Vector2Int.up, position+Vector2Int.down, position+Vector2Int.left, position+Vector2Int.right };
            foreach (var adj in cardinals)
            {
                if (!state.IsInBounds(adj)) continue;
                var neighbor = state.Tiles[state.GridToIndex(adj)];
                if (neighbor.player == currentTile.player && neighbor.piece == Wolf)
                    totalScore++;
            }
        }
 
        result.Score = totalScore;
        return result;
    }
    
    
    /*__________[ Game Over ]__________*/
    public GameOverResult CheckGameOver(BoardState state, int playerCount, int[] scores)
    {
        if (!state.AllTilesFilled()) return null;
 
        int winnerSlot = -1;
        int topScore = -1;
        bool tie = false;
 
        for (int i = 0; i < playerCount; i++)
        {
            if (scores[i] > topScore)
            {
                topScore = scores[i];
                winnerSlot = i;
                tie = false;
            }
            else if (scores[i] == topScore)
            {
                tie = true;
            }
        }
 
        return new GameOverResult(tie ? -1 : winnerSlot, (int[])scores.Clone());
    }

    
    /*__________[ Valid Moves ]__________*/
    public IReadOnlyList<int> GetValidPlacements(BoardState state, int currentPlayerSlot)
    {
        var valid = new List<int>();
        for (int i = 0; i < state.tileCount; i++)
            if (state.IsTileEmpty(i)) valid.Add(i);
        return valid;
    }
 
    public IReadOnlyList<int> GetValidPieceTypes(BoardState state, int currentPlayerSlot) => new[] { Wolf, Fox, Rabbit };



    #endregion
}
