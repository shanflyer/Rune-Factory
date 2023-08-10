using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PasturePanelAction : MonoBehaviour
{
    public Text 
        ReturnText,
        KongjianText,
        MucaoText,
        ChanshuText,
        KongjianButtonText,
        MucaoButtonText,
        ChanchuButtonText;
    public Text PastureNameText;
    public Transform AnimalInformationParent;
    public GameObject AnimalInformationPro;
    public Text CaseCounText,GrassCounText,ItemCounttext;
    public Text CaseCostText;
    public GameObject animalPanel;
    private int CostValue;
    private Pasture pasture;

    public void ClickReturn()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Return");
        gameObject.SetActive(false);
    }
    public void InitPasturePanelData(Pasture _pasture)
    {
        pasture = _pasture;
        PastureNameText.text = _pasture.name;
        int animalsCount = 0;
        if (_pasture.Animals != null)
        {
            foreach (var pastureAnimal in _pasture.Animals)
            {
                animalsCount += pastureAnimal.animalData.caseCount;
            }
        }
        CaseCounText.text = animalsCount + "/" + _pasture.caseCount;
        GrassCounText.text = _pasture.grassCount.ToString();
        int itemsCount = 0;
        if (_pasture.itemPackage.items != null)
        {
            itemsCount = _pasture.itemPackage.items.Count;
        }
        ItemCounttext.text = itemsCount + "/" + _pasture.itemPackage.CaseCount;
        CostValue = GameComponentData.gameData.pastureAction.zeroAnimalCost + (_pasture.caseCount - 2) *
                    GameComponentData.gameData.pastureAction.addAnimalCostPlus;
        CaseCostText.text = CostValue.ToString();
        foreach (Transform child in AnimalInformationParent)
        {
            Destroy(child.gameObject);
        }
        if (_pasture.Animals != null)
        {
            foreach (var pastureAnimal in _pasture.Animals)
            {
                GameObject animalInformation = Instantiate(AnimalInformationPro);
                animalInformation.transform.localScale=Vector3.one;
                animalInformation.transform.SetParent(AnimalInformationParent,false);
                animalInformation.GetComponent<AnimalInformationAction>().InitAnimal(pastureAnimal);
            }
        }
    }


    public void AnimalDeadCheck(Animal _animal)
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        if (pasture == _animal.pasture)
        {
            UpdataPasturePanelData();
            animalPanel.GetComponent<AnimalSetPanelAction>().MovePastureSelectObj.SetActive(false);
            if (animalPanel.GetComponent<AnimalSetPanelAction>().animal == _animal)
            {
                
                animalPanel.SetActive(false);
            }
        }
    }
    public void UpdataPasturePanelData()
    {
        if (pasture != null)
        {
            PastureNameText.text = pasture.name;
            int animalsCount = 0;
            if (pasture.Animals != null)
            {
                foreach (var pastureAnimal in pasture.Animals)
                {
                    animalsCount += pastureAnimal.animalData.caseCount;
                }
            }
            CaseCounText.text = animalsCount + "/" + pasture.caseCount;
            GrassCounText.text = pasture.grassCount.ToString();
            int itemsCount = 0;
            if (pasture.itemPackage.items != null)
            {
                itemsCount = pasture.itemPackage.items.Count;
            }
            ItemCounttext.text = itemsCount + "/" + pasture.itemPackage.CaseCount;
            CostValue = GameComponentData.gameData.pastureAction.zeroAnimalCost + (pasture.caseCount - 2) *
                        GameComponentData.gameData.pastureAction.addAnimalCostPlus;
            CaseCostText.text = CostValue.ToString();
            foreach (Transform child in AnimalInformationParent)
            {
                Destroy(child.gameObject);
            }
            if (pasture.Animals != null)
            {
                foreach (var pastureAnimal in pasture.Animals)
                {
                    GameObject animalInformation = Instantiate(AnimalInformationPro);
                    animalInformation.transform.localScale = Vector3.one;
                    animalInformation.transform.SetParent(AnimalInformationParent, false);
                    animalInformation.GetComponent<AnimalInformationAction>().InitAnimal(pastureAnimal);
                }
            }
        }
        
    }
    public void AddButtonClick()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("牧场空间"), CostValue,  pasture.name + LanguageManage.SwitchStr("增加1个空间"),
            CostType.增加牧场容量,ShopMoneyType.金币);
    }
    public void AddPastureCaseCount()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        GameComponentData.gameData.informationManager.AddInformation(LanguageManage.SwitchStr("*消耗金币") + CostValue + ","+pasture.name+LanguageManage.SwitchStr(" 空间+1"));
        CostValue = GameComponentData.gameData.pastureAction.zeroAnimalCost + (pasture.caseCount - 2) *
                    GameComponentData.gameData.pastureAction.addAnimalCostPlus;
        CaseCostText.text = CostValue.ToString();
        
        pasture.caseCount++;
        int animalsCount = 0;
        if (pasture.Animals != null)
        {
            foreach (var pastureAnimal in pasture.Animals)
            {
                animalsCount += pastureAnimal.animalData.caseCount;
            }
        }
        CaseCounText.text = animalsCount + "/" + pasture.caseCount;
    }
    public void PastureItemButtonClick()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        GameComponentData.gameData.pastureAction.ClickItemPackage(pasture);
        //gameObject.SetActive(false);
    }
    public void PastureGrassButtonClick()
    {
        AudioManager.PlaySE(PlayType.ONCE, "Click");
        GameComponentData.gameData.pastureAction.ClickGrassObj(pasture);
        //gameObject.SetActive(false);
    }
    // Use this for initialization
    void Start ()
    {
        
        ReturnText.text = LanguageManage.SwitchStr(ReturnText.text);
        KongjianText.text = LanguageManage.SwitchStr(KongjianText.text);
        MucaoText.text = LanguageManage.SwitchStr(MucaoText.text);
        ChanshuText.text = LanguageManage.SwitchStr(ChanshuText.text);
        KongjianButtonText.text = LanguageManage.SwitchStr(KongjianButtonText.text);
        MucaoButtonText.text = LanguageManage.SwitchStr(MucaoButtonText.text);
        ChanchuButtonText.text = LanguageManage.SwitchStr(ChanchuButtonText.text);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
