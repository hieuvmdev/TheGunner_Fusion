using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotateOverTime : MonoBehaviour
{
    public bool CanRotate;

    public Vector3 maxAnglePerFrame;
    public float rotationTime;
    public float startDelay;
    public bool rotateFromStart;

    protected Transform objTransform;

    private float _rotationSpeed;

    private void Awake()
    {
        objTransform = transform;
        CanRotate = false;
        _rotationSpeed = 360 / rotationTime;
    }

    protected void Start()
    {
        _rotationSpeed = 360 / rotationTime;
        if (rotateFromStart)
        {
            StartCoroutine(DelayRotate());
        }
    }

    private void FixedUpdate()
    {
        if (CanRotate)
        {
            float anglePerFrame = _rotationSpeed * Time.fixedDeltaTime;
            Vector3 rotateDirection = Vector3.zero;
            rotateDirection.x = Random.Range(0, maxAnglePerFrame.x);
            rotateDirection.y = Random.Range(0, maxAnglePerFrame.y);
            rotateDirection.z = Random.Range(0, maxAnglePerFrame.z);

            objTransform.Rotate(rotateDirection * anglePerFrame);
        }
    }

    private IEnumerator DelayRotate()
    {
        yield return new WaitForSeconds(startDelay);
        CanRotate = true;
    }
}
