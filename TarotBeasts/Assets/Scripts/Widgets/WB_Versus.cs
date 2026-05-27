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
using UnityEngine.UI;

/// <summary>
/// The interface widget for the versus setup screen
/// </summary>
public class WB_Versus : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private int defaultGridWidth = 6;
    [SerializeField] private int defaultGridHeight = 6;
    [SerializeField] private int defaultPlayerCount = 2;
    [SerializeField] private bool defaultTimeLimitEnabled = true;
    [SerializeField] private int defaultTimeLimitDuration = 180;
    [SerializeField] private bool defaultSpecialTilesEnabled = true;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_InputField gridWidthField;
    public TMP_InputField gridHeightField;
    public TMP_InputField playerCountField;
    public Toggle timeLimitEnabled;
    public TMP_InputField timeLimitDurationField;
    public Toggle specialTilesEnabled;
    private GameInstance gameInstance;

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnEnable()
    {
        RefreshTimeLimitInteractable();
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void RefreshTimeLimitInteractable()
    {
        timeLimitDurationField.interactable = timeLimitEnabled.isOn;
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void OnTimeLimitToggleChanged()
    {
        RefreshTimeLimitInteractable();
    }
    
    public void Confirm()
    {
        gameInstance = GameInstance.Instance;
        if (int.TryParse(gridWidthField.text, out int result1)) gameInstance.boardWidth = int.Parse(gridWidthField.text);
        if (int.TryParse(gridHeightField.text, out int result2)) gameInstance.boardHeight = int.Parse(gridHeightField.text);
        if (int.TryParse(playerCountField.text, out int result3)) gameInstance.playerCount = int.Parse(playerCountField.text);
        gameInstance.timeLimitEnabled = timeLimitEnabled.isOn;
        if (int.TryParse(timeLimitDurationField.text, out int result4)) gameInstance.timeLimitDuration = int.Parse(timeLimitDurationField.text);
        gameInstance.specialTilesEnabled = specialTilesEnabled.isOn;
    }

    public void Cancel()
    {
        gridWidthField.text = defaultGridWidth.ToString();
        gridHeightField.text = defaultGridHeight.ToString();
        playerCountField.text = defaultPlayerCount.ToString();
        timeLimitEnabled.isOn = defaultTimeLimitEnabled;
        timeLimitDurationField.text = defaultTimeLimitDuration.ToString();
        specialTilesEnabled.isOn = defaultSpecialTilesEnabled;
        
        RefreshTimeLimitInteractable();
    }


    #endregion
}
