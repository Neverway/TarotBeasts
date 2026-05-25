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

/// <summary>
/// The interface widget on the versus setup, for choosing how much money the players want to sacrifice if they lose
/// </summary>
public class WB_MoneyMatch : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("The input field where the players type how much money to sacrifice")]
    public TMP_InputField goldEntryField;
    [Tooltip("The text component that displays the maximum amount of gold that can be sacrificed (This is determined by the gold amount of the poorest player)")]
    public TMP_Text goldMaxText;
    [Tooltip("The text component that is displayed when the sacrifice amount is higher than the max (It's just some text that say 'Amount is too high!')")]
    public TMP_Text errorText;
    private GameInstance gameInstance;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
