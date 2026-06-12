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
using static UnityEngine.Audio.ProcessorInstance;

/// <summary>
/// The basic ruleset for the RPS WFR gameplay
/// </summary>
public class StandardRuleset : IRuleset
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public string DisplayName => "Standard";
    
    public const int Empty = 0, Wolf = 1, Fox = 2, Rabbit = 3;

    private readonly SpecialTileMap _specialTiles;

    public StandardRuleset(SpecialTileMap specialTiles = null)
    {
        _specialTiles = specialTiles;
    }


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
        return tile.piece is CommonAnimal commonAnimal && commonAnimal.IsUpgraded;

        //if (tile.player == 0) return false;
        //
        //if (_specialTiles != null && _specialTiles.Types[tileIndex] == SpecialTileType.AutoUpgrade) return true;
        //
        //Vector2Int pos = state.IndexToGrid(tileIndex);
        //Vector2Int[] neighbors = { pos + Vector2Int.up, pos + Vector2Int.down, pos + Vector2Int.left, pos + Vector2Int.right };
        //
        //foreach (var adjacent in neighbors)
        //{
        //    if (!state.IsInBounds(adjacent)) continue;
        //    var neighbor = state.Tiles[state.GridToIndex(adjacent)];
        //    if (neighbor.player == tile.player && Beats(tile, neighbor)) return true;
        //}
        //return false;
    }
    
    private int GetPointsAgainst(BoardTileData tile, BoardTileData other, bool thisUpgraded, bool otherUpgraded)
    {
        if (other.player == 0) return 0;
 
        if (other.player == tile.player) return 0;
 
        // SU rabbit negates fox scoring
        if (tile.piece.Is<Fox>() && other.piece.Is<Rabbit>() && otherUpgraded && !thisUpgraded)
            return 0;
 
        // SU rabbit scores on fox
        if (thisUpgraded && tile.piece.Is<Rabbit>() && other.piece.Is<Fox>() && !otherUpgraded)
            return 1;
 
        return Beats(tile, other) ? 1 : 0;
    }

    private bool Beats(BoardTileData tile, BoardTileData other)
        => tile.piece != null && other.piece != null
        && tile.piece.Is(out Animal animal) && other.piece.Is(out Animal otherAnimal)
        && animal.Beats(otherAnimal);

    private int ApplyScoreMultiplier(int raw, int tileIndex)
    {
        if (_specialTiles == null) return raw;
        return _specialTiles.Types[tileIndex] switch
        {
            SpecialTileType.DoubleScore => raw * 2,
            SpecialTileType.TripleScore => raw * 3,
            SpecialTileType.NullScore => raw * 0,
            _ => raw,
        };
    }

    
    /*__________[ Scoring ]__________*/
    public TileScoringResult ScoreTile(BoardState state, int tileIndex)
    {
        var result = new TileScoringResult { ActiveArrows = new bool[8] };
 
        var currentTile = state.Tiles[tileIndex];
        if (currentTile.player == 0) return result; // empty tile scores nothing
        if (currentTile.piece == null) return result;
 
        bool upgraded = IsPieceUpgraded(state, tileIndex);
        result.IsUpgraded = upgraded;
 
        Vector2Int position = state.IndexToGrid(tileIndex);
 
        // SU fox checks diagonals too
        
        Vector2Int[] scoringDirs = (upgraded && currentTile.piece.Is<Fox>())
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
        if (upgraded && currentTile.piece.Is<Wolf>())
        {
            foreach (var dir in BoardDir.Cardinals)
            {
                Vector2Int adj = position + dir;
                if (!state.IsInBounds(adj)) continue;
                var neighbor = state.Tiles[state.GridToIndex(adj)];
                //if (neighbor.player == currentTile.player && neighbor.piece == Wolf) // Errynei suggested that I SMITE the rule here and make it team-agnostic
                if (neighbor.piece != null && neighbor.piece.Is<Wolf>())
                    totalScore++;
            }
        }
 
        result.Score = ApplyScoreMultiplier(totalScore, tileIndex);
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
        {
            if (state.IsTileEmpty(i)) valid.Add(i);
        }
        return valid;
    }
 
    public IReadOnlyList<int> GetValidPieceTypes(BoardState state, int currentPlayerSlot) => new[] { Wolf, Fox, Rabbit };


    #endregion
}

//public static partial class GameFuncs
//{
//    public const int Empty = 0, Wolf = 1, Fox = 2, Rabbit = 3;
//
//    public static bool Beats(BoardTileData tile, BoardTileData other)
//    {
//        ProcessInput(ref tile, ref other);
//        bool output = false;
//        
//        if (tile.piece == Wolf && other.piece == Fox) output = true;
//        else if (tile.piece == Fox && other.piece == Rabbit) output = true;
//        else if (tile.piece == Rabbit && other.piece == Wolf) output = true;
//    
//        return ProcessOutput(tile, other, output);
//    }
//}

