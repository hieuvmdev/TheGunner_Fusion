using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SortLayer : MonoBehaviour
{
    public int order;

    public string  name;

    public void Sort()
    {
        GetComponent<MeshRenderer>().sortingLayerID = SortingLayer.NameToID(name);
        GetComponent<MeshRenderer>().sortingOrder = order;
    }
}
