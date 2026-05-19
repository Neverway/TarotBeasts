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

public class PlayerEntry : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public PlayerProfile Profile { get; private set; }

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Action<PlayerEntry>  onClickCallback;

    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_Text playerName; // "<sprite=1> PLAYERNAME"
    public TMP_Text playerGold; // "<sprite=0>GOLD COUNT"
    public Button selectButton;
    public GameObject selectedIndicator;
    public GameObject confirmDeleteWidgetPrefab;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        selectButton.onClick.AddListener(() => onClickCallback?.Invoke(this));
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Init(PlayerProfile profile, Action<PlayerEntry> onClick)
    {
        Profile = profile;
        onClickCallback = onClick;
        playerName.text = $"<sprite=1> {profile.username}";
        playerGold.text = $"<sprite=0>{profile.gold}";
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null) selectedIndicator.SetActive(selected);
    }

    public void DeleteProfile()
    {
        var confirmWidget = Instantiate(confirmDeleteWidgetPrefab, GetComponentInParent<Canvas>().transform);
        confirmWidget.GetComponent<WB_ProfileDelete>().SetTargetUser(Profile.username);
    }

    #endregion
}
