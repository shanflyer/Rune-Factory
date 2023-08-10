using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattelcharactorCavasAction : MonoBehaviour
{
    public Slider hpSlider;
    public GameObject NuObj;

    public void Inite(bool isNu)
    {
        NuObj.SetActive(isNu);
        hpSlider.value = 1;
    }
    public void SetHpDisplay(float HpValue)
    {
        hpSlider.value = HpValue;
    }
    public void SetNuDisplay(int nuValue)
    {
        NuObj.transform.GetChild(0).GetComponent<Image>().fillAmount = nuValue / 100.0f;
    }
}
