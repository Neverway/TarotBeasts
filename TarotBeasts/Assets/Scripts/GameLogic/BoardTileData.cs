//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using UnityEngine;


[Serializable]
public struct BoardTileData
{
    [Tooltip("The id of the player that owns this tile (0 = no owner)")]
    public int player => (piece == null) ? 0 : piece.ownedByPlayerID;

    public bool isOccupied => piece != null;

    [Tooltip("The piece on this tile (null = no piece)")]
    public Piece piece;
}
