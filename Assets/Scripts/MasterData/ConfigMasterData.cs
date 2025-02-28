using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ConfigData", order = 1)]
public class ConfigMasterData : ScriptableObject
{
    public int CAMERA_DISTANCE = 10;
    public int CAMERA_HEIGHT = 5;

    public int RESPAWN_TIME = 5;
    public float SHIELD_DECREASE_SPEED = 0.5f;
}
