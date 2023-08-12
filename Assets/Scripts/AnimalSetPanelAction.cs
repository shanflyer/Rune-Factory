using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalSetPanelAction : MonoBehaviour
{
    public List<Text> Texts;
    public GameObject MovePastureSelectObj;
    public GameObject selectPastureButtonPro;
    public Transform selectButtonParent;
    public Image animalImage;
    public Text AnimalNameText;
    public Text animalLevel;
    public Text animalAge, animalStatus;
    public Text animalCaseCountText, AnimalItemCounText;
    [HideInInspector]
    public Animal animal;
	// Use this for initialization
	void Start () {
	   
	}

    public void InitAnimalSetPanelData(Animal _animal)
    {
        foreach (var text in Texts)
        {
            LanguageManage.TextFanyi(text);
        }
        animal = _animal;
        animalImage.sprite = animal.Obj.GetComponentInChildren<SpriteRenderer>().sprite;
        AnimalNameText.text = animal.Name;
        animalLevel.text = animal.level.ToString();
        animalAge.text = LanguageManage.SwitchStr(animal.ageStatus.ToString());
        animalStatus.text = LanguageManage.SwitchStr(animal.animalStatus.ToString());
        animalCaseCountText.text =LanguageManage.SwitchStr("占据空间:")+animal.animalData.caseCount.ToString();
        AnimalItemCounText.text = LanguageManage.SwitchStr("总产出:")+animal.totalProduceCount.ToString();
    }

    public void ClickReturn()
    {
        AudioController.instance.PlayAudio(SE.Return);
        animal = null;
        gameObject.SetActive(false);
    }
    public void GetOutToNorture()
    { 
        AudioController.instance.PlayAudio(SE.click);
        Destroy(animal.Obj);
        animal.pasture.Animals.Remove(animal);
        animal.pasture.animalCaseCount -= animal.animalData.caseCount;
        GameComponentData.gameData.pasturePanelAction.UpdataPasturePanelData();
        foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
        {
            npcx.AddFriendlyexp(1);
        }
        
        GameComponentData.gameData.pasturePanelAction.UpdataPasturePanelData(); 
        InformationController.instance.AddInformation("*" + animal.Name +LanguageManage.SwitchStr(" 被放归野外，全体居民好感度加1"));
        gameObject.SetActive(false);
    }
    public void KillAnimal()
    { 
        AudioController.instance.PlayAudio(SE.Item);
        ItemData itemData = GameComponentData.gameData.itemsManager.GetItemDataFromId(animal.animalData.produceItem);

        Item item=new Item(itemData,1);
        if (GameComponentData.gameData.gameManager.gamePlayer.package.IsPackageFill(item))
        {
            GameComponentData.gameData.gameManager.gamePlayer.package.SetItemInPackage(item);
            InformationController.instance.AddInformation("*" + animal.Name + LanguageManage.SwitchStr("被宰杀，获得") + 1 + LanguageManage.SwitchStr("个 ") + itemData.Name);
            animal.pasture.animalCaseCount -= animal.animalData.caseCount;
            Destroy(animal.Obj);
            GameComponentData.gameData.employerManger.AnimalDead(animal.id);
            animal.pasture.Animals.Remove(animal);
            animal.pasture.animalCaseCount -= animal.animalData.caseCount;
            GameComponentData.gameData.pasturePanelAction.UpdataPasturePanelData();
            gameObject.SetActive(false);
            
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr("背包空间不足，不能宰杀！"));
        }
        animal = null;

    }
    
    public void SelectOtherPasture()
    {
        AudioController.instance.PlayAudio(SE.click);
        MovePastureSelectObj.SetActive(true);
        foreach (Transform child in selectButtonParent)
        {
            Destroy(child.gameObject);
        }

        PastureAction pastureAction = GameComponentData.gameData.pastureAction;
        for (int i = 0; i < pastureAction.IsPastures.Length; i++)
        {
            if (pastureAction.IsPastures[i]&&pastureAction.Pastures[i]!=animal.pasture)
            {
                Pasture selectPasture = pastureAction.Pastures[i];
                int animalCase = 0;
                if (selectPasture.Animals != null)
                {
                    foreach (var selectPastureAnimal in selectPasture.Animals)
                    {
                        animalCase += selectPastureAnimal.animalData.caseCount;
                    }
                }
                
                GameObject selectButton = Instantiate(selectPastureButtonPro);
                selectButton.GetComponentInChildren<Text>().text = selectPasture.name + "(" + animalCase + "/" +
                                                                   selectPasture.caseCount + ")";
                
                selectButton.transform.SetParent(selectButtonParent,true);
                selectButton.transform.localScale = Vector3.one;
                selectButton.GetComponent<SelectPastureButtonAction>().pasture = selectPasture;

            }
        }
        GameObject returnButton = Instantiate(selectPastureButtonPro);
        returnButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("返回");
        
        returnButton.transform.SetParent(selectButtonParent, true);
        returnButton.transform.localScale = Vector3.one;
    }

    public void MoveAnimalPasture(Pasture _pasture)
    {
        AudioController.instance.PlayAudio(SE.click);
        if (_pasture != null&&_pasture.id!=0)
        {
            int animalCase = 0;
            if (_pasture.Animals != null)
            {
                foreach (var _animal in _pasture.Animals)
                {
                    animalCase += _animal.animalData.caseCount;
                }
            }
            animalCase += animal.animalData.caseCount;
            if (_pasture.caseCount >= animalCase)
            {
                GameComponentData.gameData.pastureAction.MoveAnimalToPasture(_pasture,animal);
                MovePastureSelectObj.SetActive(false);
                GameComponentData.gameData.pasturePanelAction.UpdataPasturePanelData();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log(_pasture.name+"空间不足！");
            }

        }
        else
        {
            MovePastureSelectObj.SetActive(false);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
