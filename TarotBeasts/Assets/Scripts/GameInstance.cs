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

public class GameInstance : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Header("Board Settings")] 
    public int boardWidth = 6;
    public int boardHeight = 6;
    public int playerCount = 2;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private string ProfilesDirectory => Path.Combine(Application.persistentDataPath, "Profiles");
    private string ProfilePath(string username) => Path.Combine(ProfilesDirectory, $"{username}.json");


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public static GameInstance Instance { get; private set; }
    public List<PlayerProfile> LoadedProfiles { get; private set; } = new List<PlayerProfile>();
    public List<PlayerProfile> SelectedPlayers { get; private set; } = new List<PlayerProfile>();


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllProfiles();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void LoadAllProfiles()
    {
        LoadedProfiles.Clear();
        if (!Directory.Exists(ProfilesDirectory)) return;
        foreach (string file in Directory.GetFiles(ProfilesDirectory, "*.json"))
        {
            string json = File.ReadAllText(file);
            if (!string.IsNullOrEmpty(json))
                LoadedProfiles.Add(JsonUtility.FromJson<PlayerProfile>(json));
        }
    }

    public void SaveProfile(PlayerProfile profile)
    {
        if (!Directory.Exists(ProfilesDirectory)) Directory.CreateDirectory(ProfilesDirectory);
        File.WriteAllText(ProfilePath(profile.username), JsonUtility.ToJson(profile, true));
    }

    public bool ProfileExists(string username)
    {
        return LoadedProfiles.Exists(p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public void CreateProfile(string username)
    {
        var profile = new PlayerProfile(username);
        LoadedProfiles.Add(profile);
        SaveProfile(profile);
    }

    public void UpdateProfile(PlayerProfile profile)
    {
        int idx = LoadedProfiles.FindIndex(p => p.username == profile.username);
        if (idx >= 0) { LoadedProfiles[idx] = profile; SaveProfile(profile); }
    }

    public void DeleteProfile(string username)
    {
        LoadedProfiles.RemoveAll(p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase));
        string path = ProfilePath(username);
        if (File.Exists(path)) File.Delete(path);
    }

    public void SelectPlayer(PlayerProfile profile)
    {
        if (!SelectedPlayers.Contains(profile) && SelectedPlayers.Count < playerCount)
            SelectedPlayers.Add(profile);
    }

    public void DeselectPlayer(PlayerProfile profile)
    {
        SelectedPlayers.Remove(profile);
    }

    #endregion
}

[Serializable]
public class PlayerProfile
{
    public string username;
    public int gold;
    public int xp;

    public PlayerProfile(string username)
    {
        this.username = username;
        this.gold = 0;
        this.xp = 0;
    }
}