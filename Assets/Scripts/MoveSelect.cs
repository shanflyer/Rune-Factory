using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelect : MonoBehaviour
{
    public Animator OtherAnimator;
    public Animator animator;
	// Use this for initialization
	void Start () {
		
	}

    public void Click()
    {
        if (animator.GetBool("IsUp"))
        {
            AudioManager.PlaySE(PlayType.ONCE,"Return");
            animator.SetBool("IsUp", false);
            OtherAnimator.SetBool("IsUp", false);
        }
        else
        {
            AudioManager.PlaySE(PlayType.ONCE, "Click");
            animator.SetBool("IsUp", true);
            OtherAnimator.SetBool("IsUp", false);
        }
        
    }
   
    // Update is called once per frame
    void Update () {
		
	}
}
