using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraselectMove : MonoBehaviour
{
    public Transform left, right;
    public GameObject FunctionObj;
    private bool isRight;
	// Use this for initialization
	void Start () {
		
	}

    public void SwitchPos()
    {
        if (isRight)
        {
            FunctionObj.transform.parent = left;
            FunctionObj.transform.localPosition=Vector3.zero;
            isRight = false;
        }
        else
        {
            FunctionObj.transform.parent = right;
            FunctionObj.transform.localPosition = Vector3.zero;
            isRight = true;
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
