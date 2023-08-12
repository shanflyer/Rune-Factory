using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using LitJson;
[System.Serializable]
public enum TitleType
{
    种植=0,
    畜牧=1,
    垂钓=2,
    经商=3,
    制造=4,
    烹饪=5,
    探险=6,
    其他=7
}
[System.Serializable]
public class CharactorTitle
{
    public string name;
    public int id;
    public TitleType titleType;
    public int typeValue;
    public string valueStr;
    public string notice;
    public bool isGet;
}


public class CharactorTitleAction : MonoBehaviour
{
    public List<Text> Texts;
    public GameObject Obj;
    public Text TitleNameText, NoticeText;
    public List<CharactorTitle> CharactorTitles;
    public GameObject TitlePro0, TitlePro1, TitlePro2, TitlePro3, TitlePro4, TitlePro5, TitlePro6, TitlePro7, TitlePro8;
    public Transform TitleParent;
    public Toggle PlantingToggle;
    private List<CharactorTitle> GetCharactorTitles;
    [HideInInspector]
    public int plantingExp, livestockExp, fishingExp, manufatureExp, CookingExp, busicessExp;
    [HideInInspector]
    public int fishSellMoney, equipSellMoney, foodSellMoney, farmProduceSellMoney;
    [HideInInspector]
    public int weaponCont, equipCount;
    [HideInInspector]
    public int riceCount, wineCont, fishCookCont, soupCount, vegetableCount,canCount;
    [HideInInspector]
    public int explorCount, killMonsterCount, failureCount;
    [HideInInspector]
    public int sleepDays;
    [HideInInspector]
    public List<Vector2Int> fishTotals;
    [HideInInspector]
    public List<int> getItems;
	// Use this for initialization
	void Start ()
	{
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
    }

    public void Initdata0()
    {
        JsonToData();
        getItems = new List<int>();
        sleepDays = 0;
        explorCount = 0;
        killMonsterCount = 0;
        failureCount = 0;
        plantingExp = 0;
        livestockExp = 0;
        fishingExp = 0;
        manufatureExp = 0;
        CookingExp = 0;
        busicessExp = 0;
        fishSellMoney = 0;
        equipSellMoney = 0;
        foodSellMoney = 0;
        farmProduceSellMoney = 0;
        weaponCont = 0;
        equipCount = 0;
        riceCount = 0;
        wineCont = 0;
        fishCookCont = 0;
        soupCount = 0;
        vegetableCount = 0;
        canCount = 0;
        fishTotals = new List<Vector2Int>();
        GetCharactorTitles=new List<CharactorTitle>();
        foreach (var itemData in GameComponentData.gameData.itemsManager.ItemDataList)
        {
            if (itemData.Type == ItemType.食材 && itemData.typeValue == 6)
            {
                Vector2Int fishData = new Vector2Int(itemData.Id, 0);
                fishTotals.Add(fishData);
            }
        }
        
    }
    public void AddPlantExp(int value)
    {
        plantingExp += value;
        var plantTitles =
            CharactorTitles.FindAll(c => c.titleType == TitleType.种植&&!c.isGet && int.Parse(c.valueStr) <= plantingExp);
        foreach (var plantTitle in plantTitles)
        {
            plantTitle.isGet = true;
           GetCharactorTitles.Add(plantTitle);
        }
        DisplayGetTitle();
    }
    public void AddAnimaltExp(int value)
    {
        livestockExp += value;
        var animalTitles =
            CharactorTitles.FindAll(c => c.titleType == TitleType.畜牧&&!c.isGet && int.Parse(c.valueStr) <= livestockExp);
        foreach (var animalTitle in animalTitles)
        {
            animalTitle.isGet = true;
            GetCharactorTitles.Add(animalTitle);
        }
        DisplayGetTitle();
    }

    public void AddFishingExp(int value)
    {
        fishingExp += value;
        var fishTitles =
            CharactorTitles.FindAll(c => c.titleType == TitleType.垂钓 &&c.typeValue==0&&!c.isGet&& int.Parse(c.valueStr) <= fishingExp);
        foreach (var fishTitle in fishTitles)
        {
            fishTitle.isGet = true;
            GetCharactorTitles.Add(fishTitle);
        }
        DisplayGetTitle();
    }
    public void AddFishCount(int fishId)
    {
        var fishTotal = fishTotals.Find(f => f.x == fishId);
        fishTotal.y++;
        var goldCharactorTitle = CharactorTitles.Find(c => c.titleType == TitleType.垂钓 && c.typeValue == 1 &&
                                                           !c.isGet &&
                                                           fishTotal.x == int.Parse(c.valueStr.Split(',')[0]) &&
                                                           fishTotal.y >= int.Parse(c.valueStr.Split(',')[1]));
        if (goldCharactorTitle != null)
        {
            goldCharactorTitle.isGet = true;
            GetCharactorTitles.Add(goldCharactorTitle);
        }
        DisplayGetTitle();
    }

    public void AddBusinessExp(int value)
    {
        busicessExp += value;
        var busicessTitles =
            CharactorTitles.FindAll(c => c.titleType == TitleType.经商 && c.typeValue == 0 && !c.isGet && int.Parse(c.valueStr) <= busicessExp);
        foreach (var busicessTitle in busicessTitles)
        {
            busicessTitle.isGet = true;
            GetCharactorTitles.Add(busicessTitle);
        }
        DisplayGetTitle();
    }
    public void AddManufatureExp(int value)
    {
        manufatureExp += value;
        var manufatureTitles =
            CharactorTitles.FindAll(c => c.titleType == TitleType.制造 && c.typeValue == 0 && !c.isGet && int.Parse(c.valueStr) <= manufatureExp);
        foreach (var manufatureTitle in manufatureTitles)
        {
            manufatureTitle.isGet = true;
            GetCharactorTitles.Add(manufatureTitle);
        }
        DisplayGetTitle();
    }
    public void AddCookExp(int value)
    {
        CookingExp += value;
        var CookingTitles =
            CharactorTitles.FindAll(c => c.titleType == TitleType.烹饪 && c.typeValue == 0 && !c.isGet && int.Parse(c.valueStr) <= CookingExp);
        foreach (var CookingTitle in CookingTitles)
        {
            CookingTitle.isGet = true;
            GetCharactorTitles.Add(CookingTitle);
        }
        DisplayGetTitle();
    }

    public void CheckMoney()
    {
        var charactorTitles = CharactorTitles.FindAll(c => c.titleType == TitleType.其他 && !c.isGet &&
                                                           c.typeValue == 0 &&
                                                           int.Parse(c.valueStr) <=
                                                           GameComponentData.gameData.gameManager.gamePlayer.money);
        if (charactorTitles != null)
        {
            foreach (var charactorTitle in charactorTitles)
            {
                if (GetCharactorTitles == null)
                {
                    GetCharactorTitles = new List<CharactorTitle>();
                }
                GetCharactorTitles.Add(charactorTitle);
                charactorTitle.isGet = true;
            }
            DisplayGetTitle();
        }
       
        
    }
    public void AddCookCount(int value, int index)
    {
        int Xcount = 0;
        if (index == 1)
        {
            riceCount += value;
            Xcount = riceCount;
        }
        else if (index == 2)
        {
            wineCont += value;
            Xcount = wineCont;
        }
        else if (index == 3)
        {
            fishCookCont += value;
            Xcount = fishCookCont;
        }
        else if (index == 4)
        {
            soupCount += value;
            Xcount = soupCount;
        }
        else if (index == 5)
        {
            vegetableCount += value;
            Xcount = vegetableCount;
        }
        else if (index == 6)
        {
            canCount += value;
            Xcount = canCount;
        }
        var GoldTitles = CharactorTitles.FindAll(c => !c.isGet && c.titleType == TitleType.烹饪 &&
                                                           c.typeValue == index&&int.Parse(c.valueStr)<=Xcount);
        foreach (var charactorTitle in GoldTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
            
        }
        DisplayGetTitle();
    }
    public void AddManufatureCount(int value,int index)
    {
        if (index == 1)
        {
            weaponCont += value;
            var manufatureTitles =
                CharactorTitles.FindAll(c => c.titleType == TitleType.制造 && c.typeValue == 1 
                && !c.isGet && int.Parse(c.valueStr) <= weaponCont);
            foreach (var manufatureTitle in manufatureTitles)
            {
                manufatureTitle.isGet = true;
                GetCharactorTitles.Add(manufatureTitle);
            }
        }
        else
        {
            equipCount += value;
            var manufatureTitles =
                CharactorTitles.FindAll(c => c.titleType == TitleType.制造 && c.typeValue == 2
                                             && !c.isGet && int.Parse(c.valueStr) <= equipCount);
            foreach (var manufatureTitle in manufatureTitles)
            {
                manufatureTitle.isGet = true;
                GetCharactorTitles.Add(manufatureTitle);
            }
        }
        
        DisplayGetTitle();
    }
    public void AddBusinessMoney(int money,int index)
    {
        if (index == 0)
        {
            fishSellMoney += money;
            CharactorTitle charactorTitle = CharactorTitles.Find(c => c.id == 1306);
            if (int.Parse(charactorTitle.valueStr) <= fishSellMoney)
            {
                charactorTitle.isGet = true;
                GetCharactorTitles.Add(charactorTitle);
            }
            
        }
        else if(index==1)
        {
            equipSellMoney += money;
            CharactorTitle charactorTitle = CharactorTitles.Find(c => c.id == 1307);
            if (int.Parse(charactorTitle.valueStr) <= equipSellMoney)
            {
                charactorTitle.isGet = true;
                GetCharactorTitles.Add(charactorTitle);
            }
           
        }
        else if(index==2)
        {
            foodSellMoney += money;
            CharactorTitle charactorTitle = CharactorTitles.Find(c => c.id == 1308);
            if (int.Parse(charactorTitle.valueStr) <= foodSellMoney)
            {
                charactorTitle.isGet = true;
                GetCharactorTitles.Add(charactorTitle);
            }
            
        }
        else
        {
            farmProduceSellMoney += money;
            CharactorTitle charactorTitle = CharactorTitles.Find(c => c.id == 1309);
            if (int.Parse(charactorTitle.valueStr) <= farmProduceSellMoney)
            {
                charactorTitle.isGet = true;
                GetCharactorTitles.Add(charactorTitle);
            }
           
        }
        DisplayGetTitle();

    }
    public void DisplayGetTitle()
    {
        if (GetCharactorTitles != null)
        {
            if (GetCharactorTitles.Count > 0)
            {
                CharactorTitle charactorTitle = GetCharactorTitles[0];
                Obj.SetActive(true);
                charactorTitle.isGet = true;
                GameObject titlePro = null;
                switch (charactorTitle.titleType)
                {
                    case TitleType.种植:
                        titlePro = TitlePro1;
                        break;
                    case TitleType.畜牧:
                        titlePro = TitlePro2;
                        break;
                    case TitleType.垂钓:
                        titlePro = TitlePro3;
                        break;
                    case TitleType.烹饪:
                        titlePro = TitlePro4;
                        break;
                    case TitleType.制造:
                        titlePro = TitlePro5;
                        break;
                    case TitleType.经商:
                        titlePro = TitlePro6;
                        break;
                    case TitleType.探险:
                        titlePro = TitlePro7;
                        break;
                    case TitleType.其他:
                        titlePro = TitlePro8;
                        break;
                }
                Obj.transform.GetChild(0).GetComponentInChildren<Image>().sprite = titlePro.GetComponentInChildren<Image>().sprite;

                TitleNameText.text = charactorTitle.name;
                string notice = LanguageManage.SwitchStr("获得")+ LanguageManage.SwitchStr("称号") + ":" + charactorTitle.name + " " + charactorTitle.notice;
                NoticeText.text = notice; 
                AudioController.instance.PlayAudio(SE.Get);
                Obj.GetComponent<Animator>().SetBool("Isplay", true);
            }
        }
        GameComponentData.gameData.heritageAction.CheckHeritagesData();
        
    }
    public void AddKillCount()
    {
        killMonsterCount++;
        var charctorTitles = CharactorTitles.FindAll(c => c.titleType == TitleType.探险 && !c.isGet && c.typeValue == 2 &&
                                                          int.Parse(c.valueStr) <= killMonsterCount);
        foreach (var charactorTitle in charctorTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
        }
        DisplayGetTitle();
    }
    public void AddfailureCount()
    {
        failureCount++;
        var charctorTitles = CharactorTitles.FindAll(c => c.titleType == TitleType.探险 && !c.isGet && c.typeValue == 3 &&
                                                          int.Parse(c.valueStr) <= failureCount);
        foreach (var charactorTitle in charctorTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
        }
        DisplayGetTitle();
    }
    public void AddExplorCount()
    {
        explorCount++;
        var charctorTitles = CharactorTitles.FindAll(c => c.titleType == TitleType.探险 && !c.isGet && c.typeValue == 0&&
        int.Parse(c.valueStr)<=explorCount);
        foreach (var charactorTitle in charctorTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
        }
        DisplayGetTitle();
    }
    public void ClickNextButton()
    {
        AudioController.instance.PlayAudio(SE.click);
        Obj.GetComponent<Animator>().SetBool("Isplay", false);
        Obj.SetActive(false);
        GetCharactorTitles.RemoveAt(0);
        if (GetCharactorTitles.Count > 0)
        {
            StartCoroutine("NextCharactorTitle");
        }
    }

    public void ClicnReturenButton()
    {
        AudioController.instance.PlayAudio(SE.Return);
        GameComponentData.gameData.intelligencePanelAction.CharactortitleObj.SetActive(false);
    }
    public void AddSleepDays()
    {
        sleepDays++;
        var charactorTitles = CharactorTitles.FindAll(c => !c.isGet && c.titleType == TitleType.其他 && c.typeValue == 3 &&
                                                        int.Parse(c.valueStr) <= sleepDays);
        foreach (var charactorTitle in charactorTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
        }
        DisplayGetTitle();
    }
    public void CheckFriendly()
    {
        var charactorTitles = CharactorTitles.FindAll(c => !c.isGet && c.titleType == TitleType.其他 &&
                                                           c.typeValue == 2 &&
                                                           !GameComponentData.gameData.NpcManager.Npcxs.Exists(
                                                               n => n.npcData.friendlyLevel <= int.Parse(c.valueStr)));
        foreach (var charactorTitle in charactorTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
        }
        DisplayGetTitle();
    }
    public void AddGetItems(int itemId)
    {
        if (!getItems.Contains(itemId))
        {
           getItems.Add(itemId);
            CheckGetItems();
        }
    }
    public void CheckGetItems()
    {
        var charactorTitles = CharactorTitles.FindAll(c => c.titleType == TitleType.其他 && c.typeValue == 1 &&
                                                           !c.isGet && int.Parse(c.valueStr) <= getItems.Count);
        foreach (var charactorTitle in charactorTitles)
        {
            charactorTitle.isGet = true;
            GetCharactorTitles.Add(charactorTitle);
        }
        DisplayGetTitle();
    }
    IEnumerator NextCharactorTitle()
    {
        Obj.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        DisplayGetTitle();
    }
    public void InitData()
    {
        PlantingToggle.isOn = true;

        
    }
    public void DataToJson()
    {
        CharactorTitles=new List<CharactorTitle>();
        string filePath = Application.dataPath + @"/Resources/Datas/CharactorTitles.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(CharactorTitles);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset fileText = Resources.Load<TextAsset>("Datas/" + "CharactorTitles");
        if (fileText == null)
        {
            // Debug.LogError("No" + "ArmData");
        }
        else
        {
            string jsonStr = fileText.text;
            CharactorTitles = new List<CharactorTitle>();
            CharactorTitles = JsonMapper.ToObject<List<CharactorTitle>>(jsonStr);
            foreach (var charactorTitle in CharactorTitles)
            {
                charactorTitle.name = LanguageManage.SwitchStr(charactorTitle.name);
                charactorTitle.notice = LanguageManage.SwitchStr(charactorTitle.notice);
            }
            
        }
    }
    public void SwitchType(int i)
    {
        AudioController.instance.PlayAudio(SE.select);
        TitleType titleType = (TitleType) i;
        foreach (Transform child in TitleParent)
        {
            Destroy(child.gameObject);
        }
        var x = CharactorTitles.FindAll(c => c.titleType == titleType);
        GameObject titlePro = null;
        switch (titleType)
        {
                case TitleType.种植:
                    titlePro = TitlePro1;
                break;
                case TitleType.畜牧:
                    titlePro = TitlePro2;
                break;
                case TitleType.垂钓:
                    titlePro = TitlePro3;
                break;
                case TitleType.烹饪:
                    titlePro = TitlePro4;
                break;
                case TitleType.制造:
                    titlePro = TitlePro5;
                break;
                case TitleType.经商:
                    titlePro = TitlePro6;
                break;
                case TitleType.探险:
                    titlePro = TitlePro7;
                break;
                case TitleType.其他:
                    titlePro = TitlePro8;
                break;
        }
        foreach (var charactorTitle in x)
        {
            if (charactorTitle.isGet)
            {
                GameObject titleobj = Instantiate(titlePro);
                titleobj.transform.SetParent(TitleParent);
                titleobj.transform.localScale=Vector3.one;
                titleobj.GetComponentInChildren<Text>().text = charactorTitle.name;
            }
            else
            {
                GameObject titleobj = Instantiate(TitlePro0);
                titleobj.transform.SetParent(TitleParent);
                titleobj.transform.localScale = Vector3.one;
            }
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
