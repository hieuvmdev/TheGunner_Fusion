using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using Fusion;
using FusionHelpers;
using UnityEditor;
using Fusion.Photon.Realtime;
using ExitGames.Client.Photon;
using System.Net.NetworkInformation;

public class NetworkManager : SingletonMonoAwake<NetworkManager>
{
    public event Action<string, int, int> OnPingRegionCompleted;

    public NetworkRunner LocalRunner
    {
        get
        {
            return levelManager.Runner;
        }
    }

    public LevelManager LevelMgr
    {
        get
        {
            return levelManager;
        }
    }

    public int MainMenuSceneIndex
    {
        get
        {
            return _mainMenuSceneIndex;
        }
    }

    public bool IsKickFromRoom { get; set; }

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameManager gameManagerPrefab;
    [SerializeField] private int _mainMenuSceneIndex;

    private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
    private GameMode _gameMode;
    private FusionLauncher _fusionLauncher;

    public override void OnAwake()
    {
        base.OnAwake();

        levelManager.onStatusUpdate = OnConnectionStatusUpdate;
    }

    private void Start()
    {
        OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");
    }

    private void SetGameMode(GameMode gamemode)
    {
        _gameMode = gamemode;
    }

    public void StartMatch(string region, GameplayMode mode, int mapId)
    {
        var appSettings = BuildCustomAppSetting(region);
        SetGameMode(GameMode.Shared);
        levelManager.SetSelectedLevel(mapId);

        _fusionLauncher = FusionLauncher.Launch(_gameMode, "", appSettings, gameManagerPrefab, levelManager, mapId, OnConnectionStatusUpdate);

        
    }

    public void PingRegion(string region)
    {
        Debug.Log("Ping Region: " + region);
        
    }

    public int GetPing()
    {
        if(LocalRunner == null || LocalRunner.LocalPlayer == null)
        {
            return 0;
        }

        return (int)(LocalRunner.GetPlayerRtt(LocalRunner.LocalPlayer) * 1000);
    }

    public string GetRegion()
    {
        if (LocalRunner == null || LocalRunner.LocalPlayer == null)
        {
            return "";
        }

        return LocalRunner.SessionInfo.Region;
    }


    public void Disconnect()
    {
        //NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        if (LocalRunner != null && !LocalRunner.IsShutdown)
        {
            // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
            LocalRunner.Shutdown(false);
        }
    }

    private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status, string reason)
    {
        if (!this)
            return;

        Debug.Log(status);

        //if (status != _status)
        //{
        //    switch (status)
        //    {
        //        case FusionLauncher.ConnectionStatus.Disconnected:
        //            ErrorBox.Show("Disconnected!", reason, () => { });
        //            break;
        //        case FusionLauncher.ConnectionStatus.Failed:
        //            ErrorBox.Show("Error!", reason, () => { });
        //            break;
        //    }
        //}
        
        _status = status;
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool intro = false;
        bool progress = false;
        bool running = false;

        switch (_status)
        {
            case FusionLauncher.ConnectionStatus.Disconnected:
                //_progress.text = "Disconnected!";
                intro = true;
                break;
            case FusionLauncher.ConnectionStatus.Failed:
                //_progress.text = "Failed!";
                intro = true;
                break;
            case FusionLauncher.ConnectionStatus.Connecting:
                //_progress.text = "Connecting";
                progress = true;
                break;
            case FusionLauncher.ConnectionStatus.Connected:
                //_progress.text = "Connected";
                progress = true;
                break;
            case FusionLauncher.ConnectionStatus.Loading:
                //_progress.text = "Loading";
                progress = true;
                break;
            case FusionLauncher.ConnectionStatus.Loaded:
                running = true;
                break;
        }
    }

    private FusionAppSettings BuildCustomAppSetting(string region, string customAppID = null, string appVersion = "1.0.0")
    {

        var appSettings = PhotonAppSettings.Global.AppSettings.GetCopy(); ;

        appSettings.UseNameServer = true;
        appSettings.AppVersion = appVersion;

        if (string.IsNullOrEmpty(customAppID) == false)
        {
            appSettings.AppIdFusion = customAppID;
        }

        if (string.IsNullOrEmpty(region) == false)
        {
            appSettings.FixedRegion = region.ToLower();
        }

        // If the Region is set to China (CN),
        // the Name Server will be automatically changed to the right one
        // appSettings.Server = "ns.photonengine.cn";

        return appSettings;
    }
}


/// <summary>
/// Network Mode selection for preferred network type.
/// </summary>
public enum NetworkMode
{
    Online = 0,
    LAN = 1,
    Offline = 2
}


/// <summary>
/// This class extends Photon's Room object by custom properties.
/// Provides several methods for setting and getting variables out of them.
/// </summary>
//public static class RoomExtensions
//{       
//    /// <summary>
//    /// The key for accessing team fill per team out of the room properties.
//    /// </summary>
//    public const string size = "size";
        
//    /// <summary>
//    /// The key for accessing player scores per team out of the room properties.
//    /// </summary>
//    public const string score = "score";

//    public const string spawnId = "spawnId";

//    public static int GetSpawnId(this Room room)
//    {
//        return (int) room.CustomProperties[spawnId];
//    }

//    public static int IncreaseSpawnID(this Room room)
//    {
//        int[] sizes = room.GetSize();
//        int curSpawnId = room.GetSpawnId();
//        curSpawnId++;

//        if (curSpawnId >= 4)
//        {
//            curSpawnId = 0;
//        }

//        room.SetCustomProperties(new Hashtable() { { spawnId, curSpawnId } });
//        return curSpawnId;
//    }

//    /// <summary>
//    /// Returns the networked team fill for all teams out of properties.
//    /// </summary>
//    public static int[] GetSize(this Room room)
//    {
//        return (int[])room.CustomProperties[size];
//    }
        
//    /// <summary>
//    /// Increases the team fill for a team by one when a new player joined the game.
//    /// This is also being used on player disconnect by using a negative value.
//    /// </summary>
//    public static int[] AddSize(this Room room, int teamIndex, int value)
//    {
//        int[] sizes = room.GetSize();
//        sizes[teamIndex] += value;

//        room.SetCustomProperties(new Hashtable() {{size, sizes}});
//        return sizes;
//    }
        
//    /// <summary>
//    /// Returns the networked team scores for all teams out of properties.
//    /// </summary>
//    public static int[] GetScore(this Room room)
//    {
//        return (int[])room.CustomProperties[score];
//    }
        
//    /// <summary>
//    /// Increase the score for a team by one when a new player scored a point for his team.
//    /// </summary>
//    public static int[] AddScore(this Room room, int teamIndex, int value)
//    {
//        int[] scores = room.GetScore();
//        scores[teamIndex] += value;
            
//        room.SetCustomProperties(new Hashtable() {{score, scores}});
//        return scores;
//    }

//    public static int[] ResetScore(this Room room, int teamIndex)
//    {
//        int[] scores = room.GetScore();
//        scores[teamIndex] = 0;

//        room.SetCustomProperties(new Hashtable() { { score, scores } });
//        return scores;
//    }
//}

public class GameModeSettings
{
    public int MaxPlayers;
    public int MaxTeam;

    public GameModeSettings(int maxPlayers, int maxTeam)
    {
        this.MaxPlayers = maxPlayers;
        this.MaxTeam = maxTeam;
    }
}