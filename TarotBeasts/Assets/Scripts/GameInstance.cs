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
using System.IO;
using UnityEngine;

/// <summary>
/// The instance of all persistant game variables
/// </summary>
public class GameInstance : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Header("Board Settings")] 
    public int boardWidth = 6;
    public int boardHeight = 6;
    public int playerCount = 2;
    public bool timeLimitEnabled = true;
    public int timeLimitDuration = 420;
    public bool specialTilesEnabled = true;
    public int moneyMatchBounty = 0;
    
    [Header("Solo Settings")] 
    [Tooltip("Flag to keep track of if the game is in solo mode (this crap gets referenced a lot)")]
    public bool IsSoloMode { get; private set; }
    [Tooltip("The current round in solo mode")]
    public int SoloRound { get; private set; }
    [Tooltip("The current amount of lives the player has in solo")]
    public int SoloLives { get; private set; }
    [Tooltip("The starting amount of lives the player is given in solo")]
    public const int SoloStartingLives = 3;
    [Tooltip("The round where special tiles are introduced in solo")]
    public const int SoloSpecialTilesRound = 5;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("Helper method to get the path to the saved profiles")]
    private string ProfilesDirectory => Path.Combine(Application.persistentDataPath, "Profiles");
    [Tooltip("Helper method to get the profile file name")]
    private string ProfilePath(string username) => Path.Combine(ProfilesDirectory, $"{username}.json");


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public static GameInstance Instance { get; private set; }
    [Tooltip("A list of all the saved player profiles")]
    public List<PlayerProfile> LoadedProfiles { get; private set; } = new List<PlayerProfile>();
    [Tooltip("A list of the players who are participating in the match")]
    public List<PlayerProfile> SelectedPlayers { get; private set; } = new List<PlayerProfile>();


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        // Make the game instance persistent
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize
        LoadAllPlayerProfiles();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    
    /*__________[ Player Profile Stuffs ]__________*/
    /// <summary>
    /// Called when the game starts, loads all existing player profiles into LoadedProfiles
    /// </summary>
    public void LoadAllPlayerProfiles()
    {
        // Ensure the loaded profiles just to be safe 
        LoadedProfiles.Clear();
        
        // Exit if there are no profiles yet
        if (!Directory.Exists(ProfilesDirectory)) return;
        
        // Find each saved profile, ensure it's valid, and add it to LoadedProfiles
        foreach (string file in Directory.GetFiles(ProfilesDirectory, "*.json"))
        {
            string json = File.ReadAllText(file);
            if (!string.IsNullOrEmpty(json)) LoadedProfiles.Add(JsonUtility.FromJson<PlayerProfile>(json));
        }
    }
    
    public void SavePlayerProfile(PlayerProfile profile)
    {
        if (!Directory.Exists(ProfilesDirectory)) Directory.CreateDirectory(ProfilesDirectory);
        File.WriteAllText(ProfilePath(profile.username), JsonUtility.ToJson(profile, true));
    }

    public bool PlayerProfileExists(string username)
    {
        return LoadedProfiles.Exists(p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public void CreatePlayerProfile(string username)
    {
        var profile = new PlayerProfile(username);
        LoadedProfiles.Add(profile);
        SavePlayerProfile(profile);
    }

    public void UpdatePlayerProfile(PlayerProfile profile)
    {
        int idx = LoadedProfiles.FindIndex(p => p.username == profile.username);
        if (idx >= 0) { LoadedProfiles[idx] = profile; SavePlayerProfile(profile); }
    }

    public void DeletePlayerProfile(string username)
    {
        LoadedProfiles.RemoveAll(p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase));
        string path = ProfilePath(username);
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>
    /// Used by the profile select screen for choosing which players will be participating
    /// </summary>
    public void SelectPlayerProfile(PlayerProfile profile)
    {
        if (!SelectedPlayers.Contains(profile) && SelectedPlayers.Count < playerCount)
            SelectedPlayers.Add(profile);
    }

    /// <summary>
    /// Used by the profile select screen for choosing which players will be participating
    /// </summary>
    public void DeselectPlayerProfile(PlayerProfile profile)
    {
        SelectedPlayers.Remove(profile);
    }

    public void StartSoloMatch(PlayerProfile humanProfile)
    {
        IsSoloMode = true;
        boardWidth = 6;
        boardHeight = 6;
        playerCount = 2;
        timeLimitEnabled = true;
        timeLimitDuration = 420;
        specialTilesEnabled = false;
        moneyMatchBounty = 0;
        SoloRound = 1;
        SoloLives = SoloStartingLives;

        SelectedPlayers.Clear();
        SelectedPlayers.Add(humanProfile);
    }

    public void SoloContinue(bool playerWon)
    {
        if (!playerWon) SoloLives--;
        SoloRound++;
        if (SoloRound >= SoloSpecialTilesRound) specialTilesEnabled = true;
    }
    
    public void ClearSoloMode()
    {
        IsSoloMode = false;
        SoloRound = 0;
        SoloLives = 0;
        specialTilesEnabled = false;
    }
    
    #endregion
}

[Serializable]
public class PlayerProfile
{
    [Tooltip("The username of the player profile")]
    public string username;
    [Tooltip("The gold of the player profile")]
    public int gold;
    [Tooltip("The xp of the player profile (Not currently being used)")]
    public int xp;
    [Tooltip("The highest round the player profile has managed to make it to in solo")]
    public int soloHighestRound;

    [Tooltip("Constructor method for creating new instances of a player profile")]
    public PlayerProfile(string username)
    {
        this.username = username;
        this.gold = 0;
        this.xp = 0;
        this.soloHighestRound = 0;
    }
}