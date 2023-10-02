using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSelectAction : MonoBehaviour
{
    private Animator animator;  
	// Use this for initialization
	void Start ()
	{
	    animator = GetComponent<Animator>();
	    animator.SetBool("IsDown", false);
    }

    public void ClickTitle()
    {
        if (animator.GetBool("IsDown"))
        {
            AudioController.instance.PlayAudio(SE.Return);
            animator.SetBool("IsDown", false);
        }
        else
        {
            AudioController.instance.PlayAudio(SE.click);
            animator.SetBool("IsDown", true);
        }
        
    }

    
    public void SelectButton(int index)
    {
        //AudioController.instance.PlayAudio(SE.click);
        //animator.SetBool("IsDown", false);
        //List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
        //wareDisplayTypes.Add(WareDisplayType.ALL);
        //GameComponentData.gameData.warehouseAction.InitWareHouseData((PackageType)index,wareDisplayTypes, DisplayType.Sell);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
