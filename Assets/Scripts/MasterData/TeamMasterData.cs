using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamMasterData", menuName = "TeamData", order = 2)]
public class TeamMasterData : ScriptableObject
{
    public Team[] TeamData;
}

/// <summary>
/// Defines properties of a team.
/// </summary>
[System.Serializable]
public class Team
{
    public string Name;
    public Color32 TeamColor;
    public Material TeamMaterial;

    // Character Mat
    public Material HeadMat;
    public Material BodyMat;
    public Material WeaponMat;
}
