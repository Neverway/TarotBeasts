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
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles turn order, player lists, ruleset references, and fires game events
/// It's essentially the turn logic
/// </summary>
public class MatchController : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public event Action<BoardState, int[]> OnMatchStarted;
    public event Action<BoardState, int[], TileScoringResult[]> OnBoardChanged;
    public event Action<int, IPlayerAgent> OnTurnChanged;
    public event Action<int[]> OnScoresUpdated;
    public event Action<GameOverResult> OnGameOver;
    public event Action<BoardState, int, int[]> OnUndoPerformed;
    
    public BoardState boardState { get; private set; }
    public IRuleset ruleset { get; private set; }
    public int currentTurnSlot { get; private set; }
    public bool isGameOver { get; private set; }
    
    public int PlayerCount => agents.Count;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<IPlayerAgent> agents = new List<IPlayerAgent>();
    private int[] scores;
    private int[] turnToPlayerMap;
    private TileScoringResult[] lastScoringResults;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    /*__________[ Turn functions ]__________*/
    private void BeginCurrentTurn()
    {
        var agent = GetCurrentAgent();
        agent.OnMoveChosen += HandleMoveChosen;
        agent.BeginTurn(boardState, ruleset, currentTurnSlot);
        OnTurnChanged?.Invoke(currentTurnSlot, agent);
    }

    private void HandleMoveChosen(int tileIndex, int pieceType)
    {
        var agent = GetCurrentAgent();
        agent.OnMoveChosen -= HandleMoveChosen;
        
        // Save move to history
        boardState.UpdateMoveHistory(currentTurnSlot);
        boardState.PlacePiece(tileIndex, currentTurnSlot + 1, pieceType);
        
        RefreshScores();
        OnBoardChanged?.Invoke(boardState, scores, lastScoringResults);

        // Check game over conditions
        var gameOverResult = ruleset.CheckGameOver(boardState, PlayerCount, scores);
        if (gameOverResult != null)
        {
            isGameOver = true;
            ApplyEndOfMatchGold(gameOverResult);
            OnGameOver?.Invoke(gameOverResult);
            return;
        }
        
        NextTurn();
    }

    private void NextTurn()
    {
        currentTurnSlot = (currentTurnSlot + 1) % PlayerCount;
        BeginCurrentTurn();
    }
    
    /*__________[ Undo functions ]__________*/
    public void RequestUndo()
    {
        if (!boardState.CanUndo || isGameOver) return;
        
        // Cancel the current player's turn
        GetCurrentAgent().OnMoveChosen -= HandleMoveChosen;
        GetCurrentAgent().CancelTurn();

        int restoredSlot = boardState.PopMoveHistory();
        if (restoredSlot < 0) return;

        currentTurnSlot = restoredSlot;
        RefreshScores();
        OnUndoPerformed?.Invoke(boardState, currentTurnSlot, scores);
        BeginCurrentTurn();
    }
    
    /*__________[ Scoring functions ]__________*/
    private void RefreshScores()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            scores[i] = 0;
        }

        for (int i = 0; i < boardState.tileCount; i++)
        {
            var result = ruleset.ScoreTile(boardState, i);
            lastScoringResults[i] = result;
            int owner = boardState.Tiles[i].player;
            if (owner > 0) scores[owner - 1] += result.Score;
        }

        OnScoresUpdated?.Invoke(scores);
    }
    
    /*__________[ Game Over functions ]__________*/
    protected virtual void ApplyEndOfMatchGold(GameOverResult result)
    {
        var gameInstance = GameInstance.Instance;
        for (int slot = 0; slot < PlayerCount; slot++)
        {
            var profile = GetAgent(slot).Profile;
            int earned = (slot == result.WinnerSlot && !result.IsTie)
                ? result.FinalScores[slot] * 2
                : result.FinalScores[slot];
            profile.gold += earned;
            gameInstance.UpdatePlayerProfile(profile);
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void InitMatch(IRuleset _ruleset, List<IPlayerAgent> _agents, int _boardWidth, int _boardHeight)
    {
        ruleset = _ruleset;
        agents = _agents;

        boardState = new BoardState(_boardWidth, _boardHeight);
        scores = new int[_agents.Count];
        lastScoringResults = new TileScoringResult[boardState.tileCount];
        
        // Randomize the turn order when the match begins
        turnToPlayerMap = new int[_agents.Count];
        var slots = new List<int>();
        for (int i = 0; i < _agents.Count; i++)
        {
            slots.Add(i);
        }

        for (int i = 0; i < agents.Count; i++)
        {
            int randomOrder = UnityEngine.Random.Range(0, slots.Count);
            turnToPlayerMap[i] = slots[randomOrder];
            slots.RemoveAt(randomOrder);
        }

        currentTurnSlot = 0;
        isGameOver = false;
        
        RefreshScores();
        OnMatchStarted?.Invoke(boardState, scores);
        BeginCurrentTurn();
    }

    public IPlayerAgent GetCurrentAgent()
    {
        return GetAgent(currentTurnSlot);
    }

    public IPlayerAgent GetAgent(int turnSlot)
    {
        return agents[turnToPlayerMap[turnSlot]];
    }

    public TileScoringResult[] GetLastScoringResult()
    {
        return lastScoringResults;
    }

    public int[] GetScores()
    {
        return (int[])scores.Clone();
    }


    #endregion
}
