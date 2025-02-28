using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    public Transform target;
    public float speedCoefficient;

    private Transform _transform;
    private Vector3 _lastPos;
    private bool _canMove;

    private void Awake()
    {
        //this.RegisterListener(EventID.BackgroundMove, (param) => Move());
        //this.RegisterListener(EventID.BackgroundStop, (param) => Stop());
    }

    private void Start()
    {
        _transform = transform;
        _lastPos = target.position;
        _canMove = true;
    }

    private void Update()
    {
        if (_canMove)
        {
            Vector3 position = target.position;
            Vector3 newPos = _transform.position - new Vector3((_lastPos.x - position.x) * speedCoefficient, 0, 0);
            _transform.position = newPos;
            _lastPos = position;
        }
    }

    private void Move()
    {
        _canMove = true;
    }

    private void Stop()
    {
        _canMove = false;
    }
}