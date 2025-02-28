using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GameData
{
    private SaveGameManager _saveGameMgr;
    private GameEvents _gameE;
    private DateTime _startTime;
    private float _cacheRealtimeSession;


    public GameData(SaveGameManager saveGameMgr, GameEvents gameE)
    {
        _saveGameMgr = saveGameMgr;
        _gameE = gameE;

        NickName = "Guest - " + UnityEngine.Random.Range(100000, 999999);
    }

    public void SetStartTime(DateTime start)
    {
        _cacheRealtimeSession = Time.realtimeSinceStartup;
        _startTime = start;
    }

    public string NickName;

    public DateTime UTCNow
    {
        get
        {
            return _startTime.AddSeconds(Time.realtimeSinceStartup - _cacheRealtimeSession);
        }
    }

    public bool IsSoundActive
    {
        get
        {
            return PlayerPrefs.GetInt("IsSoundActive", 0) == 0;
        }
        set
        {
            PlayerPrefs.SetInt("IsSoundActive", value ? 0 : 1);
        }
    }

    public bool IsMusicActive
    {
        get
        {
            return PlayerPrefs.GetInt("IsMusicActive", 0) == 0;
        }
        set
        {
            PlayerPrefs.SetInt("IsMusicActive", value ? 0 : 1);
        }
    }

    public int ActiveSkin
    {
        get
        {
            return PlayerPrefs.GetInt("ActiveSkin", 0);
        }
        set
        {
            PlayerPrefs.SetInt("ActiveSkin", value);
        }
    }

    public int ActiveWeapon
    {
        get
        {
            return PlayerPrefs.GetInt("ActiveWeapon", 0);
        }
        set
        {
            PlayerPrefs.SetInt("ActiveWeapon", value);
        }
    }
}