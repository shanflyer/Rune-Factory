using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneInfo : MonoBehaviour
{
    [SerializeField]
    Text info;  
    [SerializeField]
    Animation animation;

    public void SetTextValue(string str)
    {
        info.enabled = true;
        info.text = str;
        animation.Play();
    }
    
}
