using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MarryNpcAction : MonoBehaviour
{
    public Transform left, right,down;

    public Transform boy, gril;
	// Use this for initialization
	void Start () {
	    foreach (Transform transform1 in left)
	    {
	        transform1.GetComponent<Animator>().SetFloat("X",1);
	        transform1.GetComponent<Animator>().SetFloat("Y", 0);
        }
	    foreach (Transform transform1 in right)
	    {
	        transform1.GetComponent<Animator>().SetFloat("X", -1);
	        transform1.GetComponent<Animator>().SetFloat("Y", 0);
	    }
	    HideNpc();

	}

     
    public void HideNpc()
    {
       

        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
