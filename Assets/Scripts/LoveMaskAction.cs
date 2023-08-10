using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoveMaskAction : MonoBehaviour {
    public void PlayEnd()
    {
        GetComponentInChildren<Animator>().SetBool("IsPlay",false);
        gameObject.SetActive(false);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
