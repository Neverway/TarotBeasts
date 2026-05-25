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
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// The interface widget on the versus setup, for selecting which players are participating in the match
/// </summary>
public class WB_ProfileSelect : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<PlayerEntry> spawnedEntries = new List<PlayerEntry>();

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Transform playerListRoot;
    public GameObject playerEntryPrefab;
    public Button continueButton, moneyMatchButton;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void OnEnable()
    {
        PopulateProfileList();
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void RefreshContinueButton()
    {
        continueButton.interactable = GameInstance.Instance.SelectedPlayers.Count == GameInstance.Instance.playerCount;
        moneyMatchButton.interactable = GameInstance.Instance.SelectedPlayers.Count == GameInstance.Instance.playerCount;
    }

    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void PopulateProfileList()
    {
        // Clear old entries
        foreach (Transform child in playerListRoot) Destroy(child.gameObject);
        spawnedEntries.Clear();
        GameInstance.Instance.SelectedPlayers.Clear();
        moneyMatchButton.interactable = false;
        continueButton.interactable = false;

        foreach (var profile in GameInstance.Instance.LoadedProfiles)
        {
            var go = Instantiate(playerEntryPrefab, playerListRoot);
            var entry = go.GetComponent<PlayerEntry>();
            entry.Init(profile, OnEntryClicked);
            spawnedEntries.Add(entry);
        }

        RefreshContinueButton();
    }

    private void OnEntryClicked(PlayerEntry entry)
    {
        var gi = GameInstance.Instance;
        if (gi.SelectedPlayers.Contains(entry.Profile))
        {
            gi.DeselectPlayerProfile(entry.Profile);
            entry.SetSelected(false);
        }
        else
        {
            gi.SelectPlayerProfile(entry.Profile);
            entry.SetSelected(gi.SelectedPlayers.Contains(entry.Profile));
        }
        RefreshContinueButton();
    }

    public void Continue()
    {
        SceneManager.LoadScene(1);
    }


    #endregion
}
