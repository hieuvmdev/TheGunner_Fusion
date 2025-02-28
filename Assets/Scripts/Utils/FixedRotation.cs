using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    public bool xLocked;
    public bool yLocked;
    public bool zLocked;

    public Vector3 rotation;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void FixedUpdate()
    {
        Vector3 curRotation = _transform.eulerAngles;
        if (xLocked)
        {
            curRotation.x = rotation.x;
        }
        if (yLocked)
        {
            curRotation.y = rotation.y;
        }
        if (zLocked)
        {
            curRotation.z = rotation.z;
        }
        _transform.eulerAngles = curRotation;
    }
}
