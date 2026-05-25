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

public interface IPlayerAgent
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    PlayerProfile Profile { get; }
    
    event Action<int, int> OnMoveChosen;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    void BeginTurn(BoardState state, IRuleset ruleset, int mySlot);
    
    void CancelTurn();


    #endregion
}

public class LocalPlayerAgent : IPlayerAgent
{
    public PlayerProfile Profile { get; }
 
    public event Action<int, int> OnMoveChosen;
 
    private bool waitingForInput;
 
    public LocalPlayerAgent(PlayerProfile profile)
    {
        Profile = profile;
    }
 
    public void BeginTurn(BoardState state, IRuleset ruleset, int mySlot)
    {
        waitingForInput = true;
    }
 
    public void CancelTurn()
    {
        waitingForInput = false;
    }
 
    public void ReceiveUIMove(int tileIndex, int pieceType)
    {
        if (!waitingForInput) return;
        waitingForInput = false;
        OnMoveChosen?.Invoke(tileIndex, pieceType);
    }
}

public class CPUPlayerAgent : IPlayerAgent
{
    public PlayerProfile Profile { get; }
    public event Action<int, int> OnMoveChosen;
 
    private readonly MonoBehaviour _coroutineHost;
    private readonly float _thinkTime;
    private Coroutine _thinkRoutine;
 
    public CPUPlayerAgent(PlayerProfile profile, MonoBehaviour coroutineHost, float thinkTime = 0.8f)
    {
        Profile = profile;
        _coroutineHost = coroutineHost;
        _thinkTime = thinkTime;
    }
 
    public void BeginTurn(BoardState state, IRuleset ruleset, int mySlot)
    {
        _thinkRoutine = _coroutineHost.StartCoroutine(Think(state, ruleset, mySlot));
    }
 
    public void CancelTurn()
    {
        if (_thinkRoutine != null) _coroutineHost.StopCoroutine(_thinkRoutine);
    }
 
    private IEnumerator Think(BoardState state, IRuleset ruleset, int mySlot)
    {
        yield return new WaitForSeconds(_thinkTime);
 
        var validTiles = ruleset.GetValidPlacements(state, mySlot);
        var validPieces = ruleset.GetValidPieceTypes(state, mySlot);
 
        if (validTiles.Count == 0 || validPieces.Count == 0) yield break;
 
        int tile = validTiles [UnityEngine.Random.Range(0, validTiles.Count)];
        int piece = validPieces[UnityEngine.Random.Range(0, validPieces.Count)];
 
        OnMoveChosen?.Invoke(tile, piece);
    }
}

