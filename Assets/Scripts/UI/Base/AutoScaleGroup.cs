using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class AutoScaleGroup : MonoBehaviour
{
    [SerializeField] private Image backgroundImg;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform container;

    private List<GameObject> _objects;

    private int _activeNumher = -1;

    public void Init()
    {
        _objects = new List<GameObject>();
    }

    public void ActiveNextObject(Action<GameObject> onComplete)
    {
        _activeNumher++;

        if(_objects.Count <= _activeNumher)
        {
            SpawnNewObject();
        }
        _objects[_activeNumher].SetActive(true);

        onComplete?.Invoke(_objects[_activeNumher]);

        if (_objects.Count > 0)
        {
            //backgroundImg.enabled = true;
        } else
        { 
            //backgroundImg.enabled = false; 
        }  
    }

    public void Reset()
    {
        _activeNumher = -1;
        //backgroundImg.enabled = false;
        for (int i = 0; i < _objects.Count; i++)
        {
            _objects[i].SetActive(false);
        }
    }

    private void SpawnNewObject()
    {
        GameObject newObj = Instantiate(prefab, container);

        _objects.Add(newObj);
    }
}
