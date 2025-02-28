using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool isFlip;
    private Camera _mCamera;

    void Start()
    {
        _mCamera = Camera.main;
    }

    void Update()
    {
        if (isFlip)
        {
            transform.LookAt(_mCamera.transform);
        }
        else
        {
            transform.LookAt(_mCamera.transform);
        }
    }
}