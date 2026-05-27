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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WB_SoloProfileSelect : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public Transform playerListRoot;
    public GameObject playerEntryPrefab;
    public Button startButton;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<PlayerEntry> _spawnedEntries = new List<PlayerEntry>();
    private PlayerEntry _selectedEntry;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnEnable()
    {
        PopulateProfileList();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public void PopulateProfileList()
    {
        foreach (Transform child in playerListRoot) Destroy(child.gameObject);
        _spawnedEntries.Clear();
        _selectedEntry = null;
        startButton.interactable = false;

        foreach (var profile in GameInstance.Instance.LoadedProfiles)
        {
            var go = Instantiate(playerEntryPrefab, playerListRoot);
            var entry = go.GetComponent<PlayerEntry>();
            entry.Init(profile, OnEntryClicked);
            _spawnedEntries.Add(entry);
        }
    }

    private void OnEntryClicked(PlayerEntry entry)
    {
        if (_selectedEntry != null) _selectedEntry.SetSelected(false);

        if (_selectedEntry == entry)
        {
            _selectedEntry = null;
            startButton.interactable = false;
            return;
        }

        _selectedEntry = entry;
        _selectedEntry.SetSelected(true);
        startButton.interactable = true;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void StartSolo()
    {
        if (_selectedEntry == null) return;
        GameInstance.Instance.StartSoloMatch(_selectedEntry.Profile);
        SceneManager.LoadScene(1);
    }

    public void RefreshPlayerList()
    {
        PopulateProfileList();
    }

    #endregion
}
