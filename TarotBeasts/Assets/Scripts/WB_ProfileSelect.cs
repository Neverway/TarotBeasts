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
/// On the player select screen, create a list of all the player profiles, and require the player to select as many profiles as the player count to continue
/// Clicking on a player entry will either add or remove them from the selected players list
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
    public Button continueButton;



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
        continueButton.interactable =
            GameInstance.Instance.SelectedPlayers.Count == GameInstance.Instance.playerCount;
    }

    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void PopulateProfileList()
    {
        // Clear old entries
        foreach (Transform child in playerListRoot) Destroy(child.gameObject);
        spawnedEntries.Clear();
        GameInstance.Instance.SelectedPlayers.Clear();

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
            gi.DeselectPlayer(entry.Profile);
            entry.SetSelected(false);
        }
        else
        {
            gi.SelectPlayer(entry.Profile);
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
