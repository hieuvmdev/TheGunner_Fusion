using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private Transform _transform;
    private Camera _camera;

    private void Start()
    {
        Cache();
    }

    private void Cache()
    {
        _transform = transform;
        _camera = Camera.main;
    }


    private void Update()
    {

        if (Input.GetMouseButton(0))
        {

            Vector3 pos = _transform.position;
            Vector3 mousePos = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                -_camera.transform.position.z));
            pos.x = mousePos.x;
            _transform.position = pos;
        }
        else
        {
            transform.DOMove(Vector3.zero, 0.1f).Play();
        }
    }

    public void BackDefaultPosition()
    {
        transform.DOMove(Vector3.zero, 0.1f).Play();
    }
}