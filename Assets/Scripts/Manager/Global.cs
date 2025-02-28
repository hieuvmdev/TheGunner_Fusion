using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : SingletonMonoAwake<Global>
{
    public GameEvents GameE;
    public SaveGameManager SaveGameMgr;
    public GameData GameD;
    public DataManager DataMgr;

    [SerializeField] private LoadingPanel loadingPanel;
    [SerializeField] private BasePanel[] panels;

    public override void OnAwake()
    {
        base.OnAwake();
        DontDestroyOnLoad(gameObject);

        GameE = new GameEvents();
        SaveGameMgr = new SaveGameManager(false);
        GameD = new GameData(SaveGameMgr, GameE);
      
        if(SoundManager.Instance != null)
        {
            SoundManager.Instance.Init();
        }

        SaveGameMgr.Init(GameE, GameD);
        DataMgr.Init();

        //// UI Init
        loadingPanel.Init();
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].Init();
        }

        // Event Register
        GameE.OpenPanel += OpenPanel;
        GameE.OpenPanelWithData += OpenPanelWithData;
    }

    private void OnApplicationPause(bool focus)
    {
        SaveGameMgr.OnApplicationPause(focus);
    }

    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        SaveGameMgr.OnLateUpdate();
    }

    private void OpenPanel(PanelType type)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (type == panels[i].Type)
            {
                panels[i].Open();
            }
        }
    }


    private void OpenPanelWithData(PanelType type, object[] data)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (type == panels[i].Type)
            {
                panels[i].SetData(data);
                panels[i].Open();
            }
        }
    }
}

public enum PanelType
{
    NONE, INFORMATION
}