using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookAction : MonoBehaviour
{
    public GameObject obj0, obj1, obj2, obj3, obj4, obj5, obj6;
	// Use this for initialization
	void Start () {
		
	}

    public void CloseButtonClick()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Return");
        obj0.SetActive(false);
        obj1.SetActive(false);
        obj2.SetActive(false);
        obj3.SetActive(false);
        obj4.SetActive(false);
        obj5.SetActive(false);
        obj6.SetActive(false);
        gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
