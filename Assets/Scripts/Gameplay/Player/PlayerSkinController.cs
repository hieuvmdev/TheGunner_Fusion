using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer headMesh;
    [SerializeField] private SkinnedMeshRenderer bodyMesh;
    [SerializeField] private MeshRenderer weaponMesh;


    public void SetTeam(Team teamData)
    {
        headMesh.materials = new Material[] { teamData.HeadMat };
        bodyMesh.materials = new Material[] { teamData.BodyMat };
        weaponMesh.materials = new Material[] { teamData.WeaponMat };
    }
}
