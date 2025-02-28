using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{

    [SerializeField] private List<int> _skinUnlocked;

    public GameSaveData()
    {
        _skinUnlocked = new List<int>() { 0 };
    }

    public bool HadSkin(int id)
    {
        if (_skinUnlocked == null)
        {
            _skinUnlocked = new List<int>() { 0 };
        }


        for (int i = 0; i < _skinUnlocked.Count; i++)
        {
            if (_skinUnlocked[i] == id)
            {
                return true;
            }
        }

        return false;
    }

    public void AddNewSkin(int id)
    {
        if (_skinUnlocked == null)
        {
            _skinUnlocked = new List<int>() { 0 };
        }

        _skinUnlocked.Add(id);
    }
}