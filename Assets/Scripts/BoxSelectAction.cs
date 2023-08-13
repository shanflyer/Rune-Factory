using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSelectAction : MonoBehaviour
{
    private PackageType PackageType;

    private int selectedPackage;
	// Use this for initialization
	void Start () {
		
	}

    public void InitBoxSelectData(PackageType _PackageType)
    {
        PackageType = _PackageType;
        switch (_PackageType)
        {
            case PackageType.背包:
                selectedPackage = GameComponentData.gameData.gameManager.gamePlayer.package;
                break;
            case PackageType.冰箱:
                selectedPackage = GameComponentData.gameData.gameManager.gamePlayer.icebox;
                break;
            case PackageType.杂物箱:
                selectedPackage = GameComponentData.gameData.gameManager.gamePlayer.box;
                break;
        }
    }
    public void ClickOutAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        gameObject.SetActive(false);
        GameComponentData.gameData.warehouseObj.SetActive(true);
        List<WareDisplayType> wareDisplayTypes=new List<WareDisplayType>();
        wareDisplayTypes.Add(WareDisplayType.ALL);
        GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType, wareDisplayTypes, DisplayType.Out);
    }
    public void ClickInAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        gameObject.SetActive(false);
        GameComponentData.gameData.warehouseObj.SetActive(true);
        List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
        wareDisplayTypes.Add(WareDisplayType.ALL);
        GameComponentData.gameData.warehouseAction.InitWareHouseData(PackageType, wareDisplayTypes, DisplayType.In);
    }
    public void ClickNullAction()
    {
        AudioController.instance.PlayAudio(SE.Return);
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
