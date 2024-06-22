using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneInfo : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI info;  
    [SerializeField]
    Animation animation;

    public void SetTextValue(string str)
    {
        info.enabled = true;
        info.text = str;
        animation.Play();
    }
    
}
