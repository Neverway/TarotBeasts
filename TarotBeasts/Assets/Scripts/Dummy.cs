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

public class Dummy : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        GameFuncs.ResetRegisteredProcessors();
    }

    private void OnEnable()
    {
        GameFuncs.ResetRegisteredProcessors();
        GameFuncs.NewOutputProcessor<BoardTileData, BoardTileData, bool>(nameof(GameFuncs.Beats), InvertedBeats).Register();
    }

    private void OnDisable()
    {
        GameFuncs.ResetRegisteredProcessors();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void InvertedBeats(BoardTileData tile, BoardTileData other, ref bool output)
    {
        // If the pieces are not the same, flip the outcome of the Beats function
        if (tile.piece != other.piece)
            output = !output;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
