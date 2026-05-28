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
using DG.Tweening;
using UnityEngine;

public class TarotCard_Strength : SelectableCard
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public GameObject daggerPrefab;
    public float stabDepth;
    public float stabSpeed = 0.15f;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private GameBoard gameBoard;
    private MatchController matchController;
    private GameObject spawnedDagger;
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private IEnumerator StabRoutine(int tileIndex)
    {
        // Release the dagger and set it to the tile position
        spawnedDagger.transform.SetParent(null);
        Vector3 tileWorldPos = gameBoard.GetTileWorldPosition(tileIndex);
        spawnedDagger.transform.position = new Vector3(tileWorldPos.x, spawnedDagger.transform.position.y, tileWorldPos.z);
        
        // STAB THEM BABY!
        float stabY = tileWorldPos.y-stabDepth;
        yield return spawnedDagger.transform.DOMoveY(-(stabY*2), 0.15f*3).SetEase(Ease.OutCubic).WaitForCompletion();
        yield return spawnedDagger.transform.DOMoveY(stabY, 0.15f).SetEase(Ease.InCubic).WaitForCompletion();

        gameBoard.ApplyStab(tileIndex, spawnedDagger);
        
        // Effin stupid bs
        var cachedMatch = matchController;
        var cachedHand = hand;
        
        // Burn the card and end the turn
        Expend();
        cachedHand.FinishCardTurn();
        cachedMatch.SubmitCardMove(tileIndex);
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public override void Activate(GameBoard _board, MatchController _matchController)
    {
        gameBoard = _board;
        matchController = _board.matchController;
        
        // Hide the card hand
        _board.cardHand.gameObject.SetActive(false);
        
        // GIVE THEM THE BLADE
        var arm = _board.playerArmController;
        spawnedDagger = Instantiate(daggerPrefab, arm.transform);
        spawnedDagger.transform.localPosition = Vector3.zero;
        
        _board.BeginStrengthTargeting(this);
    }

    public void OnTileStabbed(int tileIndex)
    {
        gameBoard.StartCoroutine(StabRoutine(tileIndex));
    }


    #endregion
}
