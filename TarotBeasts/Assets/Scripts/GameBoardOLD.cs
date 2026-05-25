//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The primary script for handling the game board
/// </summary>
public class GameBoardOLD : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Board Settings")] 
    public int boardWidth = 6;
    public int boardHeight = 6;
    public int playerCount = 2;
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("Const value that links a piece name to its ID")]
    public const int empty = 0, wolf = 1, fox = 2, rabbit = 3;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("The int index of the current selected tile in BoardTile[]")]
    private int currentlySelectedTile;
    [Tooltip("The id of the player who's turn it is (this is currently 0 index but it probably shouldn't be when I add new the ability for more than two players)")]
    private int currentTurn;
    [Tooltip("The scores of each of the player's")]
    private int[] scores;
    [Tooltip("The history of moves made on the board, used for undo-ing moves")]
    private Stack<BoardTileDataOLD[]> boardHistory = new Stack<BoardTileDataOLD[]>();
    [Tooltip("The history of turns during moves made on the board, used for undo-ing moves")]
    private Stack<int> turnHistory = new Stack<int>();
    
    private int[] turnToPlayerMap;
    private int[] piecePlacedCounts;
    private int[] pieceUpgradedCounts;
    private PlayerGoldEntry[] goldEntries;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Header("References")]
    public GameObject GameOverEffects;
    public TMP_Text gameOverText;
    public TMP_Text gameOverPlayerStats;
    public TMP_Text gameOverTileStats;
    public GameObject boardTilePrefab;
    public Transform boardTileParent;
    public GameObject goldPlayerEntryPrefab;
    public Transform goldShelfParent;
    public TMP_Text goldBountyCounter;
    public GameObject pieceContainer;
    public Button[] pieces;
    public TMP_Text titleText, scoreText;
    public Sprite wolfSprite, foxSprite, rabbitSprite;
    [Tooltip("These are the colors that are assigned to things related to each player")]
    public Color[] playerColors;

    private BoardTileDataOLD[] boardData;
    private BoardTile[] boardTiles;
    private Sprite[] pieceSprites;
    private GameInstance gameInstance;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        // Get the board settings from the game instance
        gameInstance = FindFirstObjectByType<GameInstance>();
        boardWidth = gameInstance.boardWidth;
        boardHeight = gameInstance.boardHeight;
        playerCount = gameInstance.playerCount;
        
        // Randomly assign turn order from selected profiles
        turnToPlayerMap = new int[playerCount];
        List<int> slots = new List<int>();
        for (int i = 0; i < playerCount; i++) slots.Add(i);
        for (int i = 0; i < playerCount; i++)
        {
            int r = UnityEngine.Random.Range(0, slots.Count);
            turnToPlayerMap[i] = slots[r];
            slots.RemoveAt(r);
        }
        
        // Set Piece sprite references
        pieceSprites = new[] { wolfSprite, foxSprite, rabbitSprite };
        
        // Initialize the info text and reset the turn just in case it broke or somethin
        currentTurn = 0;
        titleText.color = GetPlayerColor(0);
        titleText.text = $"{GetCurrentProfileName()}'s Turn";
        
        scores = new int[playerCount];
        piecePlacedCounts = new int[3];
        pieceUpgradedCounts = new int[3];

        // Set up the grid layout for the new board width and height
        var grid = boardTileParent.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = boardWidth;
        }
        
        // Create and initialize the board tiles
        boardData = new BoardTileDataOLD[boardWidth * boardHeight];
        boardTiles = new BoardTile[boardWidth * boardHeight];
        for (int i = 0; i < boardTiles.Length; i++)
        {
            GameObject obj = Instantiate(boardTilePrefab, boardTileParent);
            boardTiles[i] = obj.GetComponent<BoardTile>();
        }
        
        // Subscribe all tiles to the select tile function with their index
        for (int i = 0; i < boardTiles.Length; i++)
        {
            int index = i;
            boardTiles[i].button.onClick.AddListener(() => { SelectTile(index); }); 
        }
        
        // Subscribe all pieces to the select piece function with their index
        for (int i = 0; i < pieces.Length; i++)
        {
            int index = i;
            pieces[i].onClick.AddListener(() => { SelectPiece(index); }); 
        }
        
        // Try to fix the cell size for the new board width and height
        var rect = boardTileParent.GetComponent<RectTransform>().rect;
        float cellWidth = rect.width / boardWidth;
        float cellHeight = rect.height / boardHeight;
        grid.cellSize = new Vector2(cellWidth, cellHeight);
        
        // Populate the gold shelf
        foreach (Transform child in goldShelfParent) Destroy(child.gameObject);
        goldEntries = new PlayerGoldEntry[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            var profile = GetProfileForTurnSlot(i);
            var go = Instantiate(goldPlayerEntryPrefab, goldShelfParent);
            goldEntries[i] = go.GetComponent<PlayerGoldEntry>();
            goldEntries[i].Init(profile.username, profile.gold, GetPlayerColor(i));
        }
        
        ScoreAllTiles();
    }

    
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Returns true if the tile at the specified index is upgraded
    /// </summary>
    private bool IsPieceUpgraded(int tileIndex)
    {
        var tile = boardData[tileIndex];
        if (tile.player == 0) return false;
        Vector2Int pos = TileIndexToGridPosition(tileIndex);
        Vector2Int[] adjacent = { pos+Vector2Int.up, pos+Vector2Int.down, pos+Vector2Int.left, pos+Vector2Int.right };
        foreach (var adj in adjacent)
        {
            if (!IsGridPositionInBounds(adj)) continue;
            var neighbor = boardData[GridPositionToTileIndex(adj)];
            if (neighbor.player == tile.player && tile.Beats(neighbor)) return true;
        }

        return false;
    }
    
    /// <summary>
    /// Set the selected tile to a specified piece and update the scores
    /// </summary>
    private void SetTile(Sprite icon, int pieceIndex)
    {
        boardHistory.Push((BoardTileDataOLD[])boardData.Clone());
        turnHistory.Push(currentTurn);
        
        // Set the boardData
        int placedPiece = pieceIndex + 1;
        boardData[currentlySelectedTile].piece = placedPiece;
        boardData[currentlySelectedTile].player = currentTurn + 1;
        piecePlacedCounts[pieceIndex]++;
        // Set the icon
        boardTiles[currentlySelectedTile].icon.sprite = icon;
        boardTiles[currentlySelectedTile].icon.enabled = true;
        // Set the color
        Color turnColor = GetPlayerColor(currentTurn);
        ApplyTileColor(currentlySelectedTile, turnColor);
        // Disable the button
        boardTiles[currentlySelectedTile].button.interactable = false;
        // Score the tile
        ScoreAllTiles();
    }
    
    /// <summary>
    /// Colorize a tile's icon, score, and upgrade fxs
    /// </summary>
    private void ApplyTileColor(int tileIndex, Color color)
    {
        var tile = boardTiles[tileIndex];
        tile.icon.color = color;
        tile.score.color = color;
        var mainModule = tile.upgradedFX.GetComponentInChildren<ParticleSystem>().main;
        mainModule.startColor = color;
    }
    
    /// <summary>
    /// End the game or switch players
    /// </summary>
    private void GoToNextTurn()
    {
        // Game over if all tiles are filled
        if (CheckGameOver()) return;
        
        currentTurn = (currentTurn + 1) % playerCount;
        titleText.color = GetPlayerColor(currentTurn);
        titleText.text = $"{GetCurrentProfileName()}'s Turn";
    }
    
    /// <summary>
    /// Returns true if the game is over and displays the game over fx
    /// </summary>
    private bool CheckGameOver()
    {
        bool allTilesFilled = true;
        
        for (int i = 0; i < boardData.Length; i++)
        {
            if (boardData[i].player == 0)
            {
                allTilesFilled = false;
                break;
            }
        }

        if (allTilesFilled)
        {
            titleText.color = Color.yellow;
            titleText.text = "GAME OVER";
            GameOverEffects.SetActive(true);

            int winnerIndex = -1;
            int topScore = -1;
            bool tie = false;
            for (int i = 0; i < playerCount; i++)
            {
                if (scores[i] > topScore)
                {
                    topScore = scores[i]; winnerIndex = i; tie = false;
                }
                else if (scores[i] == topScore) tie = true;
            }

            if (tie)
            {
                gameOverText.color = Color.yellow; gameOverText.text = "It's a @#%$! Tie?!";
            }
            else
            {
                gameOverText.color = GetPlayerColor(winnerIndex); 
                gameOverText.text = $"{GetProfileForTurnSlot(winnerIndex).username} Won!";
            }
            
            // MORE GOLD!!!!!!
            var statsBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < playerCount; i++)
            {
                var profile = GetProfileForTurnSlot(i);
                int goldEarned = (i == winnerIndex && !tie) ? scores[i] * 2 : scores[i];
                profile.gold += goldEarned;
                gameInstance.UpdatePlayerProfile(profile);

                goldEntries[i].SetGold(profile.gold, profile.username, GetPlayerColor(i));
                
                string hex = ColorUtility.ToHtmlStringRGB(GetPlayerColor(i));
                statsBuilder.Append($"<color=#{hex}>{profile.username}<br>S:{scores[i]}<br><sprite=0>+{goldEarned}</color>");
                if (i < playerCount - 1) statsBuilder.Append("<br><br>");
            }
            gameOverPlayerStats.text = statsBuilder.ToString();
            
            // PIECE STATS
            string[] pieceNames = { "Wolves", "Foxes", "Rabbits" };
            string[] spriteIds  = { "1", "2", "3" };
            for (int p = 0; p < 3; p++) pieceUpgradedCounts[p] = 0;
            for (int i = 0; i < boardData.Length; i++)
            {
                if (boardData[i].piece > 0 && IsPieceUpgraded(i))
                    pieceUpgradedCounts[boardData[i].piece - 1]++;
            }
            var pieceStatsBuilder = new System.Text.StringBuilder();
            for (int p = 0; p < 3; p++)
            {
                pieceStatsBuilder.Append($"<sprite={spriteIds[p]}>{pieceNames[p]}<br>{piecePlacedCounts[p]} ({pieceUpgradedCounts[p]})<br>");
                if (p < 2) pieceStatsBuilder.Append("<br>");
            }
            gameOverTileStats.text = pieceStatsBuilder.ToString();

            for (int i = 0; i < boardTiles.Length; i++)
            {
                boardTiles[i].button.enabled = false;
            }
        }

        return allTilesFilled;
    }
    
    /// <summary>
    /// Iterates over the board updating the tile and player scores
    /// </summary>
    private void ScoreAllTiles()
    {
        for (int i = 0; i < scores.Length; i++) scores[i] = 0;
        for (int i = 0; i < boardData.Length; i++)
        {
            int tileScore = ScoreTile(i);
            int owner = boardData[i].player;
            if (owner > 0) scores[owner - 1] += tileScore;
        }
        var stringBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < playerCount; i++)
        {
            Color c = GetPlayerColor(i);
            stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>P{i + 1} Score:<br>{scores[i]}<br>");
            if (i < playerCount - 1) stringBuilder.Append("<br>");
        }
        scoreText.text = stringBuilder.ToString();
    }
    
    /// <summary>
    /// Returns the total score of a specified tile
    /// </summary>
    private int ScoreTile(int tileIndex)
    {
        var currentTile = boardData[tileIndex];
        bool upgraded = IsPieceUpgraded(tileIndex);
        boardTiles[tileIndex].upgradedFX.SetActive(upgraded);

        Vector2Int position = TileIndexToGridPosition(tileIndex);
        Vector2Int[] adjacentTiles = (upgraded && currentTile.piece == GameBoardOLD.fox)
            ? new[] {
                position+Vector2Int.up,    position+Vector2Int.down,
                position+Vector2Int.left,  position+Vector2Int.right,
                position+Vector2Int.up+Vector2Int.right, position+Vector2Int.up+Vector2Int.left,
                position+Vector2Int.down+Vector2Int.right, position+Vector2Int.down+Vector2Int.left
            }
            : new[] {
                position+Vector2Int.up,   position+Vector2Int.down,
                position+Vector2Int.left, position+Vector2Int.right
            };

        Vector2Int[] allDirections = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            Vector2Int.up+Vector2Int.left, Vector2Int.up+Vector2Int.right,
            Vector2Int.down+Vector2Int.right, Vector2Int.down+Vector2Int.left
        };
        var scoringDirections = new System.Collections.Generic.HashSet<Vector2Int>(adjacentTiles);
        var arrows = boardTiles[tileIndex].scoringArrows;
        Color tileColor = currentTile.player > 0 ? GetPlayerColor(currentTile.player - 1) : Color.white;
        for (int d = 0; d < allDirections.Length && d < arrows.Length; d++)
        {
            Vector2Int dir = position + allDirections[d];
            if (!scoringDirections.Contains(position + allDirections[d]) || !IsGridPositionInBounds(dir))
            {
                arrows[d].SetActive(false);
                continue;
            }
            int otherIndex = GridPositionToTileIndex(dir);
            bool otherUpgraded = IsPieceUpgraded(otherIndex);
            int points = currentTile.GetPointsAgainst(boardData[otherIndex], upgraded, otherUpgraded);
            arrows[d].SetActive(points > 0);
            if (points > 0)
            {
                var img = arrows[d].GetComponent<Image>();
                if (img != null) img.color = GetArrowColor(tileColor);
            }
        }

        int tileScore = 0;
        foreach (var adjacentTile in adjacentTiles)
        {
            if (!IsGridPositionInBounds(adjacentTile)) continue;
            int otherIndex = GridPositionToTileIndex(adjacentTile);
            bool otherUpgraded = IsPieceUpgraded(otherIndex);
            tileScore += currentTile.GetPointsAgainst(boardData[otherIndex], upgraded, otherUpgraded);
        }

        if (upgraded && currentTile.piece == GameBoardOLD.wolf)
        {
            Vector2Int[] cardinals = { position+Vector2Int.up, position+Vector2Int.down, position+Vector2Int.left, position+Vector2Int.right };
            foreach (var adj in cardinals)
            {
                if (!IsGridPositionInBounds(adj)) continue;
                var neighbor = boardData[GridPositionToTileIndex(adj)];
                if (neighbor.player == currentTile.player && neighbor.piece == GameBoardOLD.wolf) tileScore++;
            }
        }

        boardTiles[tileIndex].score.text = tileScore.ToString();
        return tileScore;
    }

    /// <summary>
    /// Returns the color of a player
    /// </summary>
    private Color GetPlayerColor(int playerIndex)
    {
        return playerColors != null && playerIndex < playerColors.Length ? playerColors[playerIndex] : Color.white;
    }

    private PlayerProfile GetProfileForTurnSlot(int turnSlot)
    {
        return gameInstance.SelectedPlayers[turnToPlayerMap[turnSlot]];
    }

    private string GetCurrentProfileName()
    {
        return GetProfileForTurnSlot(currentTurn).username;
    }
    
    private Color GetArrowColor(Color playerColor)
    {
        Color.RGBToHSV(playerColor, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * 0.5f, Mathf.Min(v * 1.4f, 1f));
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    
    /// <summary>
    /// Converts an int index of a tile to the coresponding grid position
    /// </summary>
    public Vector2Int TileIndexToGridPosition(int index)
    {
        return new Vector2Int(index % boardWidth, Mathf.FloorToInt(index / boardWidth));
    }
    
    /// <summary>
    /// Converts an x,y grid position to the int index of the tile
    /// </summary>
    public int GridPositionToTileIndex(Vector2Int gridPosition)
    {
        return (gridPosition.y * boardWidth)+gridPosition.x;
    }

    /// <summary>
    /// Checks to see if an x,y grid positon is in bounds
    /// </summary>
    public bool IsGridPositionInBounds(Vector2Int gridPosition)
    {
        return gridPosition.y < boardHeight && gridPosition.y >= 0 && gridPosition.x < boardWidth && gridPosition.x >= 0;
    }
    
    /// <summary>
    /// When a tile is pressed, show the pieces, and set the selected tile index
    /// </summary>
    public void SelectTile(int tileIndex)
    {
        pieceContainer.SetActive(true);
        currentlySelectedTile = tileIndex;
        boardTiles[currentlySelectedTile].button.Select();
    }
    
    /// <summary>
    /// When a piece is selected, set the tile, end the turn
    /// </summary>
    public void SelectPiece(int pieceIndex)
    {
        if (pieceIndex < 0 || pieceIndex >= pieceSprites.Length) return;
        SetTile(pieceSprites[pieceIndex], pieceIndex);
        pieceContainer.SetActive(false);
        GoToNextTurn();
    }
    
    /// <summary>
    /// Reverts the last move
    /// </summary>
    public void Undo()
    {
        if (boardHistory.Count == 0) return;
        
        int lastPiece = boardData[currentlySelectedTile].piece;
        if (boardHistory.Count > 0 && lastPiece > 0) piecePlacedCounts[lastPiece - 1]--;
        
        boardData = boardHistory.Pop();
        currentTurn = turnHistory.Pop();

        Sprite[] sprites = { null, wolfSprite, foxSprite, rabbitSprite };
        Color turnColor = GetPlayerColor(currentTurn);
        for (int i = 0; i < boardTiles.Length; i++)
        {
            var data = boardData[i];
            if (data.player == 0)
            {
                boardTiles[i].icon.enabled = false;
                boardTiles[i].icon.sprite = null;
                boardTiles[i].score.text = "";
                boardTiles[i].button.interactable = true;
                boardTiles[i].upgradedFX.SetActive(false);
            }
            else
            {
                //boardTiles[i].icon.sprite = sprites[data.piece];
                //boardTiles[i].icon.color = colors[data.player - 1];
                //boardTiles[i].score.color = colors[data.player - 1];
                boardTiles[i].icon.sprite = pieceSprites[data.piece - 1];
                ApplyTileColor(i, GetPlayerColor(data.player - 1));
                boardTiles[i].button.interactable = false;
            }
        }

        titleText.color = GetPlayerColor(currentTurn);
        titleText.text = $"{GetCurrentProfileName()}'s Turn";

        ScoreAllTiles();
    }
    
    /// <summary>
    /// Resets the board for another game
    /// </summary>
    public void ResetBoard()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Exits the current game and returns to the title screen
    /// </summary>
    public void ReturnToTitleScreen()
    {
        SceneManager.LoadScene(0);
    }


    #endregion
}

[Serializable]
public struct BoardTileDataOLD
{
    [Tooltip("The id of the player that owns this tile (0 = no owner)")]
    public int player;
    [Tooltip("The id of the piece on this tile")]
    public int piece;

    public int GetPointsAgainst(BoardTileDataOLD otherTile, bool thisUpgraded = false, bool otherUpgraded = false)
    {
        if (otherTile.player == 0) return 0;

        // Sudo wolf
        if (otherTile.player == player)
        {
            if (piece == GameBoardOLD.wolf && otherTile.piece == GameBoardOLD.wolf && otherUpgraded)
                return 1;
            return 0;
        }

        // Fox nulled by SU rabbity
        if (piece == GameBoardOLD.fox && otherTile.piece == GameBoardOLD.rabbit && otherUpgraded && !thisUpgraded)
            return 0;

        // SU rabbity beats fox
        if (thisUpgraded && piece == GameBoardOLD.rabbit && otherTile.piece == GameBoardOLD.fox && !otherUpgraded)
            return 1;

        // Normal crap
        if (Beats(otherTile)) return 1;
        return 0;
    }

    public bool Beats(BoardTileDataOLD otherTile)
    {
        if (piece == GameBoardOLD.wolf && otherTile.piece == GameBoardOLD.fox) return true;
        if (piece == GameBoardOLD.fox && otherTile.piece == GameBoardOLD.rabbit) return true;
        if (piece == GameBoardOLD.rabbit && otherTile.piece == GameBoardOLD.wolf) return true;
        return false;
    }
}