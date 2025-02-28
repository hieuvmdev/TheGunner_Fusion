using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkStatsDisplay : MonoBehaviour
{
    private TextMeshProUGUI _text;

    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        _text.SetText(string.Format("Ping: {0} ({1})", NetworkManager.Instance.GetPing(), NetworkManager.Instance.GetRegion()));
    }
}
