using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryAfterTime : MonoBehaviour
{
    public float timeValue;
	// Use this for initialization
	void Start ()
	{
	    StartCoroutine(waitDestory());
	}

    IEnumerator waitDestory()
    {
        yield return new WaitForSeconds(timeValue);
        Destroy(gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
