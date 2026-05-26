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
    private int maxBounty;
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("The input field where the players type how much money to sacrifice")]
    public TMP_InputField goldEntryField;
    [Tooltip("The text component that displays the maximum amount of gold that can be sacrificed (This is determined by the gold amount of the poorest player)")]
    public TMP_Text goldMaxText;
    [Tooltip("The text component that is displayed when the sacrifice amount is higher than the max (It's just some text that say 'Amount is too high!')")]
    public TMP_Text errorText;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnEnable()
    {
        maxBounty = int.MaxValue;
        foreach (var profile in GameInstance.Instance.SelectedPlayers)
        {
            if (profile.gold < maxBounty)
                maxBounty = profile.gold;
        }

        if (maxBounty == int.MaxValue) maxBounty = 0;

        goldMaxText.text = $" / {maxBounty}";
        errorText.gameObject.SetActive(false);

        goldEntryField.text = GameInstance.Instance.moneyMatchBounty > 0 ? GameInstance.Instance.moneyMatchBounty.ToString() : "0";
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private bool TryGetEnteredAmount(out int amount)
    {
        return int.TryParse(goldEntryField.text, out amount) && amount >= 0;
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void OnAmountChanged()
    {
        if (!TryGetEnteredAmount(out int amount))
        {
            errorText.text = "Enter a valid number!";
            errorText.gameObject.SetActive(true);
            return;
        }
        errorText.gameObject.SetActive(amount > maxBounty);
        errorText.text = "Amount is too high!";
    }

    public void Confirm()
    {
        if (!TryGetEnteredAmount(out int amount))
        {
            errorText.text = "Enter a valid number!";
            errorText.gameObject.SetActive(true);
            return;
        }

        if (amount > maxBounty)
        {
            errorText.text = "Amount is too high!";
            errorText.gameObject.SetActive(true);
            return;
        }

        GameInstance.Instance.moneyMatchBounty = amount;
        errorText.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        GameInstance.Instance.moneyMatchBounty = 0;
    }


    #endregion
}
