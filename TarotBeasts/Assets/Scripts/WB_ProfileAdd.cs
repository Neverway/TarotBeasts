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
/// The interface widget on the versus setup, for adding new player profiles
/// </summary>
public class WB_ProfileEdit : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public TMP_InputField usernameField;
    public TMP_Text warningMessage;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Called by the profile screen's confirm button
    /// </summary>
    public void Confirm()
    {
        string name = usernameField.text.Trim();

        if (name.Length < 3)
        {
            warningMessage.text = "Username must be at least 3 characters.";
            return;
        }

        if (GameInstance.Instance.PlayerProfileExists(name))
        {
            warningMessage.text = "A profile with that name already exists.";
            return;
        }

        GameInstance.Instance.CreatePlayerProfile(name);
        warningMessage.text = "";
    }

    
    /// <summary>
    /// Called by the input field when it's value changes
    /// </summary>
    public void ClearWarningMessage()
    {
        warningMessage.text = "";
    }


    #endregion
}
