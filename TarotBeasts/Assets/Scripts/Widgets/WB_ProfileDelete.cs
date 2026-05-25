//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The interface widget on the versus setup, for deleting saved player profiles
/// </summary>
public class WB_ProfileDelete : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private string username;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Button confirmDeleteButton;
    public TMP_Text profileNameText;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetTargetUser(string _username)
    {
        username = _username;
        confirmDeleteButton.interactable = true;
    }
    
    public void CloseWidget()
    {
        Destroy(gameObject);
    }

    public void DeleteProfile()
    {
        var gameInstance = FindFirstObjectByType<GameInstance>();
        gameInstance.DeletePlayerProfile(username);
        FindFirstObjectByType<WB_ProfileSelect>().PopulateProfileList();
        Destroy(gameObject);
    }


    #endregion
}
