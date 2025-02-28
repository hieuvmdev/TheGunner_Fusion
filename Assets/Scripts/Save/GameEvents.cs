using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GameEvents
{
    public GameEvents()
    {

    }

    public Action<PanelType> OpenPanel;
    public Action<PanelType, object[]> OpenPanelWithData;
    public Action<bool> ActiveLoading;
    public Action<bool, string> ActiveLoadingWithMessage;

    //Button
    public Action OnGameStart;
    public Action OnGameRestart;

    public Action<float> OnUpdateHealth;
    public Action<bool> OnDisconnect;

    // Sound And Music
    public Action<AudioEnum, float> PlaySFX;
}
