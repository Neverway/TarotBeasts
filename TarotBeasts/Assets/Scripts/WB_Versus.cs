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
using TMPro;
using UnityEngine;

public class WB_Versus : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private int defaultGridWidth = 6, defaultGridHeight = 6, defaultPlayerCount = 2;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_InputField gridWidthField, gridHeightField, playerCountField;
    private GameInstance gameInstance;
    



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Confirm()
    {
        gameInstance = FindFirstObjectByType<GameInstance>();
        gameInstance.boardWidth = int.Parse(gridWidthField.text);
        gameInstance.boardHeight = int.Parse(gridHeightField.text);
        gameInstance.playerCount = int.Parse(playerCountField.text);
    }

    public void Cancel()
    {
        gridWidthField.text = defaultGridWidth.ToString();
        gridHeightField.text = defaultGridHeight.ToString();
        playerCountField.text = defaultPlayerCount.ToString();
    }


    #endregion
}
