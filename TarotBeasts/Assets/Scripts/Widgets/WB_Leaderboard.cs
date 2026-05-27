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

public class WB_Leaderboard : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<PlayerEntry> spawnedEntries = new List<PlayerEntry>();


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Transform playerListRoot;
    public GameObject playerEntryPrefab;
    public TMP_Text emptyLabel;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void OnEnable()
    {
        PopulateLeaderboard();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void PopulateLeaderboard()
    {
        foreach (Transform child in playerListRoot) Destroy(child.gameObject);
        spawnedEntries.Clear();

        var profiles = new List<PlayerProfile>(GameInstance.Instance.LoadedProfiles);
        profiles.Sort((a, b) => b.soloHighestRound.CompareTo(a.soloHighestRound));

        bool anyEntries = profiles.Count > 0;
        if (emptyLabel != null) emptyLabel.gameObject.SetActive(!anyEntries);

        foreach (var profile in profiles)
        {
            var newPlayerEntry = Instantiate(playerEntryPrefab, playerListRoot);
            var entry = newPlayerEntry.GetComponent<PlayerEntry>();
            entry.Init(profile, null);
            entry.ShowHighestRound();
            spawnedEntries.Add(entry);
        }
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Refresh()
    {
        PopulateLeaderboard();
    }

    #endregion
}
