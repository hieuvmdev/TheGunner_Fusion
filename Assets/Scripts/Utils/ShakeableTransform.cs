using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShakeableTransform : MonoBehaviour
{
    /// <summary>
    /// Maximum angle, in degrees, the transform will rotate
    /// during shaking.
    /// </summary>
    [SerializeField]
    Vector3 maximumAngularShake = Vector3.one * 15;

    /// <summary>
    /// Frequency of the Perlin noise function. Higher values
    /// will result in faster shaking.
    /// </summary>
    [SerializeField]
    float frequency = 25;

    private bool canShake;
    private float _timer;

    private void Awake()
    {
        canShake = false;
    }

    public void Shake()
    {
        canShake = true;
        _timer = Random.Range(0,2) == 1 ? 0 : Mathf.PI;
    }

    public void Stop()
    {
        canShake = false;
    }

    private void Update()
    {
        if (canShake == false)
            return;

        Vector3 angularShake = Vector3.zero;
        angularShake.z = maximumAngularShake.z * Mathf.Sin(_timer);
        transform.localRotation = Quaternion.Euler(angularShake);
        _timer += Time.deltaTime * frequency;
    }
}