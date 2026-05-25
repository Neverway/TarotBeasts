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
/// Used on the entries for each player's gold counter on the game board
/// </summary>
public class PlayerGoldEntry : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_Text goldCounterText; // <sprite=0><color=PLAYER COLOR HERE>000



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Init(string playerName, int gold, Color playerColor)
    {
        string hex = ColorUtility.ToHtmlStringRGB(playerColor);
        goldCounterText.text = $"<sprite=0><color=#{hex}>{playerName}: {gold}";
    }

    public void SetGold(int gold, string playerName, Color playerColor)
    {
        string hex = ColorUtility.ToHtmlStringRGB(playerColor);
        goldCounterText.text = $"<sprite=0><color=#{hex}>{playerName}: {gold}";
    }

    #endregion
}
