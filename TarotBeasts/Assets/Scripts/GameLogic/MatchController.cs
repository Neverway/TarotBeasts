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
    public event Action<float[]> OnTimersUpdated;
    public event Action<int> OnPlayerTimedOut; 
    
    public BoardState boardState { get; private set; }
    public IRuleset ruleset { get; private set; }
    public int currentTurnSlot { get; private set; }
    public bool isGameOver { get; private set; }
    public SpecialTileMap SpecialTiles { get; private set; }
    
    public int PlayerCount => agents.Count;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<IPlayerAgent> agents = new List<IPlayerAgent>();
    private int[] scores;
    private int[] turnToPlayerMap;
    private TileScoringResult[] lastScoringResults;
    private PlayerTimers _timers;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Update()
    {
        if (_timers == null || isGameOver) return;
        bool ranOut = _timers.Tick(currentTurnSlot, Time.deltaTime);
        OnTimersUpdated?.Invoke(_timers.RemainingTime);
        if (ranOut) TryEndGameByTime();
    }
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    /*__________[ Turn functions ]__________*/
    private void BeginCurrentTurn()
    {
        if (_timers != null)
        {
            int attempts = 0;
            while (_timers.Eliminated[currentTurnSlot] && attempts < PlayerCount)
            {
                currentTurnSlot = (currentTurnSlot + 1) % PlayerCount;
                attempts++;
            }
        }
        
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
    
    private void HandlePlayerTimedOut(int slot)
    {
        OnPlayerTimedOut?.Invoke(slot);
        GetCurrentAgent().OnMoveChosen -= HandleMoveChosen;
        GetCurrentAgent().CancelTurn();
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
        int bounty = gameInstance.moneyMatchBounty;

        if (bounty > 0)
        {
            for (int slot = 0; slot < PlayerCount; slot++)
            {
                var profile = GetAgent(slot).Profile;
                profile.gold = Mathf.Max(0, profile.gold - bounty);
                gameInstance.UpdatePlayerProfile(profile);
            }
        }
        
        for (int slot = 0; slot < PlayerCount; slot++)
        {
            var profile = GetAgent(slot).Profile;
            bool isWinner = slot == result.WinnerSlot && !result.IsTie;
            int earned = isWinner ? result.FinalScores[slot] * 2 : result.FinalScores[slot];
            if (isWinner) earned += bounty * PlayerCount;
            profile.gold += earned;
            gameInstance.UpdatePlayerProfile(profile);
        }

        result.MoneyMatchBounty = bounty;
        gameInstance.moneyMatchBounty = 0;
    }

    private GameOverResult CheckGameOverByTime()
    {
        int winnerSlot = -1;
        int topScore = -1;
        bool tie = false;
        for (int i = 0; i < PlayerCount; i++)
        {
            if (_timers.Eliminated[i]) continue;
            if (scores[i] > topScore) 
            { 
                topScore = scores[i];
                winnerSlot = i;
                tie = false;
            }
            else if (scores[i] == topScore)
            {
                tie = true;
            }
        }

        return new GameOverResult(tie ? -1 : winnerSlot, (int[])scores.Clone());
    }
    
    private void TryEndGameByTime()
    {
        if (_timers.ActivePlayerCount() <= 1)
        {
            var result = CheckGameOverByTime();
            if (result != null) { isGameOver = true; ApplyEndOfMatchGold(result); OnGameOver?.Invoke(result); }
            return;
        }
        NextTurn();
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void InitMatch(IRuleset _ruleset, List<IPlayerAgent> _agents, int _boardWidth, int _boardHeight)
    {
        ruleset = _ruleset;
        agents = _agents;

        boardState = new BoardState(_boardWidth, _boardHeight);
        scores = new int[_agents.Count];
        lastScoringResults = new TileScoringResult[boardState.tileCount];
        
        var gameInstace = GameInstance.Instance;

        // Special Tiles
        SpecialTiles = gameInstace.specialTilesEnabled ? SpecialTileMap.Generate(boardState.tileCount) : null;
        if (ruleset is StandardRuleset && SpecialTiles != null)
        {
            ruleset = new StandardRuleset(SpecialTiles);
        }
        
        // Timers
        _timers = gameInstace.timeLimitEnabled ? new PlayerTimers(agents.Count, gameInstace.timeLimitDuration) : null;
        if (_timers != null) _timers.OnPlayerEliminated += HandlePlayerTimedOut;
        
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

    public bool IsPlayerEliminated(int slot) => _timers != null && _timers.Eliminated[slot];


    #endregion
}
