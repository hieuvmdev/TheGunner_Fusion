using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShapeRotate : MonoBehaviour
{
    public float timeRotate;
    public bool isClockwise;
    void Start()
    {
        Rotate();
    }

    private void Rotate()
    {
        float angle = isClockwise ? 360.0f : -360.0f;
        //Debug.Log(timeRotate);
        transform.DORotate(new Vector3(0, 0, angle), timeRotate + 2, RotateMode.LocalAxisAdd).SetEase(DG.Tweening.Ease.Linear).SetLoops(-1).Play();
    }

    public void StopRotate()
    {
        transform.DOKill();
    }
}
