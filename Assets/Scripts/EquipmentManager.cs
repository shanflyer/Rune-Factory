using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EuqipmentType
{
    
}
[System.Serializable]
public class Euqipment
{
    public string name;
    public int id;
    public string objName;
    public int shopItem;
    public bool isBuy;
}
public class EquipmentManager : MonoBehaviour
{
    public List<Euqipment> Euqipments;

    private Transform FurnitureTransform;
	// Use this for initialization
	void Start () {
	    foreach (var euqipment in Euqipments)
	    {
	        euqipment.name = LanguageManage.SwitchStr(euqipment.name);
	    }
	}

    public void DisplayEuqiqment()
    {
       // FurnitureTransform = GameComponentData.gameData.mapParent.GetChild(0).GetChild(2);
        foreach (var euqipment in Euqipments)
        {
            if (euqipment.objName != "")
            {
               GameObject obj=FurnitureTransform.Find(euqipment.objName).gameObject;
                if (euqipment.id == 2006&&GameComponentData.gameData.gameDebugAction.HaveChildTest)
                {
                    euqipment.isBuy = true;
                }
            if (euqipment.isBuy)
            {
                obj.SetActive(true);
            }
            else
            {
                obj.SetActive(false);
            } 
            }
           
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
