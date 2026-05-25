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
    public int player;

    [Tooltip("The id of the piece on this tile")]
    public int piece;
}
