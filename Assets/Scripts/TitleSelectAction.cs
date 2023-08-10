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
            AudioManager.PlaySE(PlayType.ONCE,"Return");
            animator.SetBool("IsDown", false);
        }
        else
        {
            AudioManager.PlaySE(PlayType.ONCE,"Click");
            animator.SetBool("IsDown", true);
        }
        
    }

    public void SelectManufacturingDisplay(int x)
    {
        PackageType packageType = (PackageType) x;
        GameComponentData.gameData.manufacturingAction.DisplayMyBox(packageType);
        animator.SetBool("IsDown", false);
    }
    public void SelectButton(int index)
    {
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        animator.SetBool("IsDown", false);
        List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
        wareDisplayTypes.Add(WareDisplayType.ALL);
        GameComponentData.gameData.warehouseAction.InitWareHouseData((PackageType)index,wareDisplayTypes, DisplayType.Sell);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
