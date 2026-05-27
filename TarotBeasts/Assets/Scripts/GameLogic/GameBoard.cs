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
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the board tiles, inputs, and listens to events from MatchController
/// </summary>
public class GameBoard : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private MatchController _match;
    private BoardTile[] _boardTiles;
    private PlayerGoldEntry[] _goldEntries;
    private Sprite[] _pieceSprites;
 
    private int _selectedTileIndex = -1;
    private bool _waitingForPieceInput = false;
 
    private int[] _piecePlacedCounts = new int[3];
    private GameOverResult _lastGameOverResult;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Header("Board Layout")]
    public GameObject boardTilePrefab;
    public Transform  boardTileParent;
 
    [Header("Piece Buttons")]
    public GameObject pieceContainer;
    public Button[] pieces;
    public Sprite wolfSprite, foxSprite, rabbitSprite;
 
    [Header("Gold Shelf")]
    public GameObject goldPlayerEntryPrefab;
    public Transform goldShelfParent;
    public TMP_Text goldBountyCounter;
 
    [Header("HUD")]
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text roundText;
    public TMP_Text livesText;
 
    [Header("Game Over")]
    public GameObject GameOverEffects;
    public TMP_Text gameOverText;
    public TMP_Text gameOverPlayerStats;
    public TMP_Text gameOverTileStats;
    public Button continueButton;

    [Header("CPU Visuals")] 
    public ArmController cpuArmController;
 
    [Header("Player Colors")]
    public Color[] playerColors;
    public Color eliminatedColor = Color.gray;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        var gameInstance = GameInstance.Instance;

        var agents = new List<IPlayerAgent>();
        if (gameInstance.IsSoloMode)
        {
            agents.Add(new LocalPlayerAgent(gameInstance.SelectedPlayers[0]));
            var cpuProfile = new PlayerProfile("D.D.");
            agents.Add(new PAgent_CPUSmart(cpuProfile, this, gameInstance.SoloRound));
        }
        else
        {
            foreach (var profile in gameInstance.SelectedPlayers)
                agents.Add(new LocalPlayerAgent(profile));
        }

        IRuleset ruleset = new StandardRuleset();

        _pieceSprites = new[] { wolfSprite, foxSprite, rabbitSprite };
        _piecePlacedCounts = new int[_pieceSprites.Length];

        SetupBoardTiles(gameInstance.boardWidth, gameInstance.boardHeight);
        SetupGoldShelf(agents, gameInstance.boardWidth, gameInstance.boardHeight);
        SubscribePieceButtons();
        
        if (gameInstance.IsSoloMode)
        {
            if (roundText != null) roundText.text = $"Round {gameInstance.SoloRound}";
            if (livesText != null) livesText.text  = $"<sprite=2> {gameInstance.SoloLives}";
        }
        else
        {
            if (roundText != null) roundText.text = "VS";
            if (livesText != null) livesText.text = "";
        }

        if (continueButton != null) continueButton.gameObject.SetActive(false);
        
        _match = gameObject.AddComponent<MatchController>();
        _match.OnMatchStarted += HandleMatchStarted;
        _match.OnBoardChanged += HandleBoardChanged;
        _match.OnTurnChanged += HandleTurnChanged;
        _match.OnScoresUpdated += HandleScoresUpdated;
        _match.OnGameOver += HandleGameOver;
        _match.OnUndoPerformed += HandleUndoPerformed;
        _match.OnTimersUpdated += HandleTimersUpdated;
        _match.OnPlayerTimedOut += HandlePlayerTimedOut;
        
        _match.InitMatch(ruleset, agents, gameInstance.boardWidth, gameInstance.boardHeight);
    }
    
    private void OnDestroy()
    {
        if (_match == null) return;
        _match.OnMatchStarted -= HandleMatchStarted;
        _match.OnBoardChanged -= HandleBoardChanged;
        _match.OnTurnChanged -= HandleTurnChanged;
        _match.OnScoresUpdated -= HandleScoresUpdated;
        _match.OnGameOver -= HandleGameOver;
        _match.OnUndoPerformed -= HandleUndoPerformed;
        _match.OnTimersUpdated -= HandleTimersUpdated;
        _match.OnPlayerTimedOut -= HandlePlayerTimedOut;
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    /*__________[ Setup ]__________*/
    private void SetupBoardTiles(int width, int height)
    {
        var grid = boardTileParent.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width;
        }

        int count = width * height;
        _boardTiles = new BoardTile[count];
        for (int i = 0; i < count; i++)
        {
            var newBoardTile = Instantiate(boardTilePrefab, boardTileParent);
            _boardTiles[i] = newBoardTile.GetComponent<BoardTile>();
        }

        for (int i = 0; i < _boardTiles.Length; i++)
        {
            int index = i;
            _boardTiles[i].button.onClick.AddListener(() => SelectTile(index));
        }

        if (grid != null)
        {
            var rect = boardTileParent.GetComponent<RectTransform>().rect;
            grid.cellSize = new Vector2(rect.width / width, rect.height / height);
        }
    }

    private void SetupGoldShelf(List<IPlayerAgent> agents, int _width, int _height)
    {
        foreach (Transform child in goldShelfParent)
        {
            Destroy(child.gameObject);
        }

        _goldEntries = new PlayerGoldEntry[agents.Count];
        for (int i = 0; i < agents.Count; i++)
        {
            var profile = agents[i].Profile;
            var newPlayerEntry = Instantiate(goldPlayerEntryPrefab, goldShelfParent);
            _goldEntries[i] = newPlayerEntry.GetComponent<PlayerGoldEntry>();
            _goldEntries[i].Init(profile.username, profile.gold, GetPlayerColor(i));
        }

        goldBountyCounter.text = $"<sprite=0>{GameInstance.Instance.moneyMatchBounty.ToString()}";
    }

    private void SubscribePieceButtons()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            int index = i;
            pieces[i].onClick.AddListener(() => SelectPiece(index));
        }
    }

    private void ApplySpecialTileVisuals()
    {
        var map = _match.SpecialTiles;
        if (map == null) return;

        for (int i = 0; i < _boardTiles.Length; i++)
        {
            var type = map.Types[i];
            if (type == SpecialTileType.None) continue;
            SetSpecialTileVisual(_boardTiles[i], type);
        }
    }
    
    
    
    /*__________[ Inputs ]__________*/
    public void SelectTile(int tileIndex)
    {
        if (_match.isGameOver) return;
 
        _selectedTileIndex = tileIndex;
        _waitingForPieceInput = true;
        pieceContainer.SetActive(true);
        _boardTiles[tileIndex].button.Select();
    }
 
    public void SelectPiece(int pieceIndex)
    {
        if (!_waitingForPieceInput) return;
        if (pieceIndex < 0 || pieceIndex >= _pieceSprites.Length) return;
 
        _waitingForPieceInput = false;
        pieceContainer.SetActive(false);
 
        _piecePlacedCounts[pieceIndex]++;
 
        if (_match.GetCurrentAgent() is LocalPlayerAgent human)
            human.ReceiveUIMove(_selectedTileIndex, pieceIndex + 1);
    }
 
    public void Undo()
    {
        _waitingForPieceInput = false;
        pieceContainer.SetActive(false);
        _match.RequestUndo();
    }
 
    public void ResetBoard() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
 
    public void ReturnToTitleScreen()
    { 
        GameInstance.Instance.ClearSoloMode();
        SceneManager.LoadScene(0);
    }

    public void SoloContinue()
    {
        bool playerWon = DidLocalPlayerWin(_lastGameOverResult);
        GameInstance.Instance.SoloContinue(playerWon);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    
    
    /*__________[ Match Events ]__________*/
    private void HandleMatchStarted(BoardState state, int[] scores)
    {
        RefreshAllTileVisuals(state);
        RefreshScoreText(scores);
        ApplySpecialTileVisuals();
    }
 
    private void HandleBoardChanged(BoardState state, int[] scores, TileScoringResult[] scoringResults)
    {
        RefreshAllTileVisuals(state, scoringResults);
        RefreshScoreText(scores);
    }
 
    private void HandleTurnChanged(int turnSlot, IPlayerAgent agent)
    {
        titleText.color = GetPlayerColor(turnSlot);
        titleText.text  = $"{agent.Profile.username}'s Turn";
        
        bool isPlayer = agent is LocalPlayerAgent;
        if (!isPlayer)
        {
            _waitingForPieceInput = false;
            pieceContainer.SetActive(false);
        }
    }
 
    private void HandleScoresUpdated(int[] scores)
    {
        RefreshScoreText(scores);
    }
 
    private void HandleGameOver(GameOverResult result)
    {
        _lastGameOverResult = result;
        
        titleText.color = Color.yellow;
        titleText.text  = "GAME OVER";
        GameOverEffects.SetActive(true);
        GetComponent<CursorController>().SetCursorVisible(true);
 
        // Disable all tiles
        foreach (var t in _boardTiles) t.button.enabled = false;
 
        // Winner text
        if (result.IsTie)
        {
            gameOverText.color = Color.yellow;
            gameOverText.text  = "It's a @#%$! Tie?!";
        }
        else
        {
            var winner = _match.GetAgent(result.WinnerSlot);
            gameOverText.color = GetPlayerColor(result.WinnerSlot);
            gameOverText.text  = $"{winner.Profile.username} Won!";
        }
 
        // Give players their gooooooold
        var statsBuilder = new StringBuilder();
        for (int i = 0; i < _match.PlayerCount; i++)
        {
            var agent = _match.GetAgent(i);
            int bounty = result.MoneyMatchBounty;
            int earned;
            if (i == result.WinnerSlot && !result.IsTie)
            {
                earned = (result.FinalScores[i] * 2) + (bounty * _match.PlayerCount);
            }
            else
            {
                earned = result.FinalScores[i] - bounty;
            }
            
            // Update the final player scores info
            string hex = ColorUtility.ToHtmlStringRGB(GetPlayerColor(i));
            statsBuilder.Append($"<color=#{hex}>{agent.Profile.username}<br>S:{result.FinalScores[i]}<br><sprite=0>+{earned}</color>");
            if (i < _match.PlayerCount - 1) statsBuilder.Append("<br><br>");
 
            // Update gold shelf display
            _goldEntries[i].SetGold(agent.Profile.gold, agent.Profile.username, GetPlayerColor(i));
        }
        gameOverPlayerStats.text = statsBuilder.ToString();
 
        // Piece stats
        BuildPieceStatsText(_match.boardState);
        
        if (GameInstance.Instance.IsSoloMode)
            HandleSoloGameOver(result);
    }
 
    private void HandleUndoPerformed(BoardState state, int restoredTurnSlot, int[] scores)
    {
        RefreshAllTileVisuals(state);
        RefreshScoreText(scores);
    }

    private void HandleTimersUpdated(float[] remaining)
    {
        if (timerText == null) return;
        int slot = _match.currentTurnSlot;
        bool eliminated = _match.IsPlayerEliminated(slot);
        timerText.color = eliminated ? eliminatedColor : GetPlayerColor(slot);
        timerText.text = eliminated ? "OUT" : FormatTime(remaining[slot]);
    }

    private void HandlePlayerTimedOut(int slot)
    {
        if (_goldEntries != null && slot < _goldEntries.Length)
        {
            _goldEntries[slot].SetGold(_match.GetAgent(slot).Profile.gold, _match.GetAgent(slot).Profile.username, eliminatedColor);
        }
    }
    
    private void HandleSoloGameOver(GameOverResult result)
    {
        var gi = GameInstance.Instance;
        bool playerWon = DidLocalPlayerWin(result);

        // Take their LIVVVVVEEEEESSS
        int livesAfter;
        if (playerWon || result.IsTie)
        {
            livesAfter = gi.SoloLives;
        }
        else
        {
            livesAfter = gi.SoloLives - 1;
        }

        // Save their highest round
        var humanProfile = gi.SelectedPlayers[0];
        if (gi.SoloRound > humanProfile.soloHighestRound)
        {
            humanProfile.soloHighestRound = gi.SoloRound;
            gi.UpdatePlayerProfile(humanProfile);
        }

        // Update the lives counter
        if (livesText != null)
        {
            livesText.text = $"<sprite=2> {Mathf.Max(0, livesAfter)}";
        }

        // Show or hide the continue button
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(livesAfter > 0);
        }
    }
    
    
    
    /*__________[ Visuals ]__________*/
    private void RefreshAllTileVisuals(BoardState state, TileScoringResult[] scoringResults = null)
    {
        Sprite[] spriteMap = { null, wolfSprite, foxSprite, rabbitSprite };
 
        for (int i = 0; i < _boardTiles.Length; i++)
        {
            var data = state.Tiles[i];
            var tile = _boardTiles[i];
 
            if (data.player == 0)
            {
                tile.icon.enabled = false;
                tile.icon.sprite = null;
                tile.score.text = "";
                tile.button.interactable = true;
                tile.upgradedFX.SetActive(false);
                SetArrowsActive(tile, null);
                if (_match.SpecialTiles != null) SetSpecialTileVisual(tile, _match.SpecialTiles.Types[i]);
                else if (tile.special != null) tile.special.text = "";
            }
            else
            {
                tile.icon.sprite = spriteMap[data.piece];
                tile.icon.enabled = true;
                tile.button.interactable = false;
 
                Color c = GetPlayerColor(data.player - 1);
                tile.icon.color = c;
                tile.score.color = c;
                var ps = tile.upgradedFX.GetComponentInChildren<ParticleSystem>();
                if (ps != null) { var m = ps.main; m.startColor = c; }
 
                if (scoringResults != null)
                {
                    var sr = scoringResults[i];
                    tile.upgradedFX.SetActive(sr.IsUpgraded);
                    tile.score.text = sr.Score.ToString();
                    SetArrowsActive(tile, sr.ActiveArrows, GetArrowColor(c));
                }
            }
        }
    }
 
    private void SetArrowsActive(BoardTile tile, bool[] active, Color? arrowColor = null)
    {
        var arrows = tile.scoringArrows;
        for (int d = 0; d < arrows.Length; d++)
        {
            bool on = active != null && d < active.Length && active[d];
            arrows[d].SetActive(on);
            if (on && arrowColor.HasValue)
            {
                var img = arrows[d].GetComponent<Image>();
                if (img != null) img.color = arrowColor.Value;
            }
        }
    }
 
    private void RefreshScoreText(int[] scores)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < scores.Length; i++)
        {
            Color c = GetPlayerColor(i);
            sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{_match.GetAgent(i).Profile.username}:<br>{scores[i]}<br>");
            if (i < scores.Length - 1) sb.Append("<br>");
        }
        scoreText.text = sb.ToString();
    }
 
    private void BuildPieceStatsText(BoardState state)
    {
        string[] pieceNames = { "Wolves", "Foxes", "Rabbits" };
        string[] spriteIds = { "1", "2", "3" };
        int[] upgraded = new int[3];
 
        for (int i = 0; i < state.tileCount; i++)
        {
            int piece = state.Tiles[i].piece;
            if (piece > 0 && _match.ruleset.IsPieceUpgraded(state, i))
                upgraded[piece - 1]++;
        }
 
        var sb = new StringBuilder();
        for (int p = 0; p < 3; p++)
        {
            sb.Append($"<sprite={spriteIds[p]}>{pieceNames[p]}<br>{_piecePlacedCounts[p]} ({upgraded[p]})<br>");
            if (p < 2) sb.Append("<br>");
        }
        gameOverTileStats.text = sb.ToString();
    }
 
    private Color GetPlayerColor(int playerIndex)
    {
        return playerColors != null && playerIndex < playerColors.Length ? playerColors[playerIndex] : Color.white;
    }
 
    private Color GetArrowColor(Color playerColor)
    {
        Color.RGBToHSV(playerColor, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * 0.5f, Mathf.Min(v * 1.4f, 1f));
    }

    private void SetSpecialTileVisual(BoardTile tile, SpecialTileType type)
    {
        if (tile.special == null) return;
        tile.special.text = type switch
        {
            SpecialTileType.DoubleScore => "2x",
            SpecialTileType.TripleScore => "3x",
            SpecialTileType.NullScore => "0x",
            SpecialTileType.AutoUpgrade => "UP^",
            _ => "",
        };
    }

    private static string FormatTime(float seconds)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        return $"{min}:{sec:D2}";
    }
    
    public void NotifyCPUPlacedAt(int tileIndex)
    {
        cpuArmController.gameObject.SetActive(true);
        if (cpuArmController != null && _boardTiles[tileIndex] != null)
            cpuArmController.MoveCPUToTarget(_boardTiles[tileIndex].transform.position);
    }



    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    private bool DidLocalPlayerWin(GameOverResult result)
    {
        if (result.IsTie) return false;
        return _match.GetAgent(result.WinnerSlot) is LocalPlayerAgent;
    }

    #endregion
}
