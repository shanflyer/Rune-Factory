using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeMove : MonoBehaviour {
    public float eyeMoveSpeed;
    private Animator animator;
    private float eyeAngle;
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        StartCoroutine("cycleMove");
	}
    IEnumerator CycleMove()
    {
        while (true)
        {
            
            eyeAngle += eyeMoveSpeed * Time.deltaTime;
            if (eyeAngle > 1)
            {
                eyeAngle = 0;
            }
            animator.SetFloat("eyeAngle", eyeAngle);
            yield return new WaitForFixedUpdate();
        }
       
    }
	// Update is called once per frame
	void Update () {
		
	}
}
