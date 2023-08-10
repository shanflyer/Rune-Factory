using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCamera : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    InitCameraSize();

	}

    public void InitCameraSize()
    {
        float h = -transform.position.z;
        float w = Camera.main.orthographicSize;
        float a = Mathf.Atan(w / h);
        float view = a / Mathf.Deg2Rad;
        GetComponent<Camera>().fieldOfView = 2 * view;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
