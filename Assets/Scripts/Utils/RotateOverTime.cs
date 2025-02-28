using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public Vector3 rotateDirection;
    public Vector3 startAngle;
    public float startDelay;
    public float rotationTime;
    public bool rotateFromStart;

    protected Transform objTransform;

    private bool _canRotate;
    public bool CanRotate
    {
        set { _canRotate = value; }
    }

    private bool _prepareToChangeRotationTime;
    private float _rotationSpeed = 0;

    public float RotationSpeed
    {
        set { _rotationSpeed = value; }
    }
    private float _newRotationTime;

    public void Rotate()
    {
        _canRotate = true;
        objTransform.localEulerAngles = startAngle;
        _rotationSpeed = 360 / rotationTime;
    }

    public void ChangeRotationTime(float newRotationTime)
    {
        _newRotationTime = newRotationTime;
        _prepareToChangeRotationTime = true;
    }

    public void ResetRotation()
    {
        _canRotate = false;
        objTransform.localEulerAngles = startAngle;
    }

    public void SetRotationTime(float newRotationTime)
    {
        if (Mathf.Abs(newRotationTime) <= 0.001f)
        {
            return;
        }

        _prepareToChangeRotationTime = false;
        rotationTime = newRotationTime;
       objTransform.localEulerAngles = startAngle;
        _rotationSpeed = 360 / rotationTime;
    }

    public void ChangeDirection(Vector3 direction)
    {
        rotateDirection = direction;
    }

    private void Awake()
    {
        objTransform = transform;
        _canRotate = false;
    }

    protected virtual void Start()
    {

        _prepareToChangeRotationTime = false;
        objTransform.localEulerAngles = startAngle;

        if (Mathf.Abs(rotationTime) <= 0.001f)
        {
            rotationTime = 0;
            return;
        }
        else
        {
            _rotationSpeed = 360 / rotationTime;
        }

       
        if (rotateFromStart)
        {
            StartCoroutine(DelayRotate());
        }
    }

    private void FixedUpdate()
    {
        float anglePerFrame = _rotationSpeed * Time.fixedDeltaTime;
        if (_canRotate)
        {

            transform.localEulerAngles += rotateDirection * anglePerFrame;
            if (_prepareToChangeRotationTime)
            {
                if (Mathf.Abs(objTransform.eulerAngles.z) < anglePerFrame * 2f)
                {
                    SetRotationTime(_newRotationTime);
                }
            }
        }
    }

    private IEnumerator DelayRotate()
    {
        yield return new WaitForSeconds(startDelay);
        _canRotate = true;
    }
}
