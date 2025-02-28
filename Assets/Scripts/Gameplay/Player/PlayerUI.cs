using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private TextMeshProUGUI labelTxt;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider shieldSlider;

    [SerializeField] private GameObject aimObj;

    public void SetActive(bool active)
    {
        parent.SetActive(active);
    }

    public void SetAimActive(bool active)
    {
        aimObj.SetActive(active);
    }

    public void SetLabel(string label)
    {
        labelTxt.text = label;
    }

    public void UpdateHealthSlider(float value)
    {
        healthSlider.value = value;
    }

    public void UpdateShieldSlider(float value)
    {
        //shieldSlider.value = value;
    }
}
