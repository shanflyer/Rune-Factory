using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLine : MonoBehaviour
{
    public Transform startPos, endPos;

    public LineRenderer lineRenderer;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		lineRenderer.SetPosition(0,startPos.position);
        lineRenderer.SetPosition(1,endPos.position);
	}
}
