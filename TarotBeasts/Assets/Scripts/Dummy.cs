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
        GameFuncs.ResetRegisteredProcessing();
    }

    private void OnEnable()
    {
        GameFuncs.ResetRegisteredProcessing();
        GameFuncs.RegisterOutputProcessing<BoardTileData, BoardTileData, bool>(nameof(GameFuncs.Beats), InvertedBeats);
    }

    private void OnDisable()
    {
        GameFuncs.ResetRegisteredProcessing();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private bool InvertedBeats(BoardTileData tile, BoardTileData other, bool output)
    {
        // Modify the interactions between tile and other to come up with a new output
        return !output;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
