
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueSetDisplay : MonoBehaviour
{
    private List<Image> images;

    void Awake()
    {
        ZeroInit();
    }

    public void ZeroInit()
    {
        images = new List<Image>();
        foreach (Transform child in transform)
        {
            images.Add(child.GetChild(0).GetComponent<Image>());
        }
    }

    public void SetValue(int value)
    {
        if (value > 10)
        {
            value = 10;
        }
        if (value <0)
        {
            value = 0;
        }

        for (int i = 0; i < images.Count; i++)
        {
            if (i < value)
            {
                images[i].fillAmount = 1;
            }
            else
            {
                images[i].fillAmount = 0;
            }
        }
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
