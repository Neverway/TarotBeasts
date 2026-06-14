//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the grid data, indexing, and undos
/// </summary>
public class BoardState
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    // Board info
    public int width;
    public int height;
    public int tileCount => width * height;
    public BoardTileData[] Tiles { get; private set; }


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    // Turn stuff
    private Stack<BoardTileData[]> boardHistory = new Stack<BoardTileData[]>();
    private Stack<int> turnHistory = new Stack<int>();

    public bool CanUndo => boardHistory.Count > 0;

    public BoardState(int _width, int _height)
    {
        width = _width;
        height = _height;
        Tiles = new BoardTileData[tileCount];
    }


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    
    public void UpdatePieceInfo()
    {
        Context.PushTag(Context.Tags.UPDATING_PIECE_INFO);

        foreach (var tile in Tiles)
            if (tile.isOccupied) 
                tile.piece.OnUpdatePieceInfo();

        Context.PopTag();
    }

    /*__________[ Undo / Move history ]__________*/
    public void UpdateMoveHistory(int currentTurn)
    {
        boardHistory.Push((BoardTileData[])Tiles.Clone());
        turnHistory.Push(currentTurn);
    }

    public int PopMoveHistory()
    {
        if (boardHistory.Count == 0) return -1;
        Tiles = boardHistory.Pop();
        return turnHistory.Pop();
    }
    
    /*__________[ Grid functions ]__________*/
    public Vector2Int IndexToGrid(int index)
        => new Vector2Int(index % width, Mathf.FloorToInt(index / (float)width));
    public int GridToIndex(Vector2Int position) 
        => (position.y * width) + position.x;
    public bool IsInBounds(Vector2Int position)
        => position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    public bool IsTileEmpty(int index) 
        => Tiles[index].player == 0;

    public bool AllTilesFilled()
    {
        for (int i = 0; i < Tiles.Length; i++)
        {
            if (!Tiles[i].isOccupied) return false;
        }
        return true;
    }

    public void PlacePiece(int tileIndex, int playerSlot, int pieceType)
    {
        Tiles[tileIndex].piece = Piece.Create(pieceType, playerSlot, new BoardPos(this, tileIndex));
    }

    public void ClearTile(int tileIndex)
    {
        Tiles[tileIndex].piece = null;
    }


    #endregion
}
