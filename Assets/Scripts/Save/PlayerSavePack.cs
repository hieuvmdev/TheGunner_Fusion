using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[System.Serializable]
public class PlayerSavePack
{
    public GameSaveData SaveData;

    public static int VERSION = 1;
    public int SAVED_VERSION;

    public PlayerSavePack()
    {
        SAVED_VERSION = VERSION;
        SaveData = new GameSaveData();
    }
}

