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
            AudioController.instance.PlayAudio(SE.Return);
            animator.SetBool("IsUp", false);
            OtherAnimator.SetBool("IsUp", false);
        }
        else
        {
            AudioController.instance.PlayAudio(SE.click);
            animator.SetBool("IsUp", true);
            OtherAnimator.SetBool("IsUp", false);
        }
        
    }
   
    // Update is called once per frame
    void Update () {
		
	}
}
