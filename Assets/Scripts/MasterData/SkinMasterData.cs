using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "SkinMasterData", order = 1)]
public class SkinMasterData : ScriptableObject
{
    public int Id;
    public GameObject SkinPrefab;
    public CharacterStats Stats;
}