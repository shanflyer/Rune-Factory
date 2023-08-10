using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IApBUGClear : MonoBehaviour
{
    private float enterTime;

    private bool isDown;

    public bool isStart;
	// Use this for initialization
	void Start ()
	{
	    isStart = false;
	    isDown = false;
	    enterTime = 0;

	}

    public void DownTimeAction()
    {
        if (isStart)
        {
            isDown = true;
        }
        
    }

    public void UPAction()
    {
        isDown = false;
        enterTime = 0;
    }
	// Update is called once per frame
	void FixedUpdate () {
	    if (isDown)
	    {
	        enterTime += Time.deltaTime;
	        if (enterTime > 5)
	        {
	            isStart = false;
	            isDown = false;
	            enterTime = 0;
                transform.parent.gameObject.SetActive(false);
	        }
	    }
	}
}
