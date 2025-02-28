using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public ConfigMasterData ConfigMasterData 
    { 
        get
        {
            return configMasterData;
        }
    }

    public TeamMasterData TeamMasterData
    {
        get
        {
            return teamMasterData;
        }
    }

    [SerializeField] private ConfigMasterData configMasterData;
    [SerializeField] private TeamMasterData teamMasterData;
    [SerializeField] private List<SkinMasterData> skinsMasterData;

    [SerializeField] private Dictionary<int, SkinMasterData> _dicSkinMasterData;

    public void Init()
    {
        _dicSkinMasterData = new Dictionary<int, SkinMasterData>();

        foreach (var skin in skinsMasterData)
        {
            _dicSkinMasterData.Add(skin.Id, skin);
        }
    }

    public SkinMasterData GetSkinMasterData(int id)
    {
        if(_dicSkinMasterData.ContainsKey(id))
        {
            return _dicSkinMasterData[id];
        } else
        {
            Debug.LogError("SkinMasterData with id " + id + " not found");
        }

        return null;
    }
}