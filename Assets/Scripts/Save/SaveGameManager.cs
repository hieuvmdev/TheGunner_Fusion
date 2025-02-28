using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveGameManager
{
    static string saveGamePath = Application.dataPath + "/playerSave.json";

    public PlayerSavePack SavedPack { get { return savedPack; } }

    private PlayerSavePack savedPack;
    private bool isDirty;
    private bool isRequestSave;
    private bool useThreadSave;
    private bool isThreadSaving;


    private string stringSavedPack;
    private GameEvents _gameEvents;
    private GameData _gameD;

    public SaveGameManager(bool useThreadSave = true)
    {
#if UNITY_EDITOR
        saveGamePath = Application.dataPath + "/playerSave.json";
#else
        saveGamePath = Application.persistentDataPath + "/playerSave.json";
#endif

        this.useThreadSave = useThreadSave;
        this.isThreadSaving = false;
        this.isDirty = false;
        this.isRequestSave = false;


    }

    ~SaveGameManager()
    {


    }

    public void Init(GameEvents gameEvents, GameData gameD)
    {
        _gameD = gameD;
        _gameEvents = gameEvents;
    }

    private void DoSaveGame()
    {
        if (isThreadSaving)
        {
            isDirty = true;
            return;
        }

        stringSavedPack = JsonUtility.ToJson(savedPack);

#if IGNORE_SAVEGAME
        isDirty = false;
        isRequestSave = false;
        return;
#endif
        if (useThreadSave)
        {

            //Thread parseThread = new Thread(
            //    new ThreadStart(ThreadSave)
            //);
            //parseThread.Start();
            ThreadPool.QueueUserWorkItem(ThreadSave);
            //Task.Run(ThreadSave);
        }
        else
        {
            ThreadSave(null);
        }

        //bf.Serialize(file, Global.Instance.SavedPack);

        //file.Close();
        isDirty = false;
        isRequestSave = false;
    }

    public void DoLoadSavedGame()
    {
        if (File.Exists(saveGamePath))
        {
            savedPack =
                JsonUtility.FromJson<PlayerSavePack>(File.ReadAllText(saveGamePath, System.Text.Encoding.ASCII));
        }
        else
        {
            savedPack = new PlayerSavePack();
            RequestSaveGame();
        }
    }

    public void RequestSaveGame()
    {
        isRequestSave = true;
    }

    public void OnMarkDirty()
    {
        isDirty = true;
    }

    public void SaveIfDirty()
    {
        if (isDirty)
        {
            isRequestSave = true;
        }
    }

    public void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (isDirty)
            {
                DoSaveGame();
            }
        }
    }

    public void OnLateUpdate()
    {

        if (isRequestSave)
        {
            DoSaveGame();
#if ENABLE_LOG_API
            Debug.LogFormat("Memo: {0} - Token: {1} - Heart: {2}", userData.Memo, userData.DifToken, userData.DifHeart);
#endif
        }
    }


    void ThreadSave(object stateInfo)
    {
        isThreadSaving = true;
        try
        {
            File.WriteAllText(saveGamePath, stringSavedPack, System.Text.Encoding.ASCII);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
        isThreadSaving = false;
    }

#if UNITY_EDITOR
    [MenuItem("HelpFunction/ClearSave")]
    static void DoSomething()
    {
        //File.WriteAllText(saveGamePath, "", System.Text.Encoding.ASCII);
        File.Delete(saveGamePath);
        PlayerPrefs.DeleteAll();
    }
#endif
}
