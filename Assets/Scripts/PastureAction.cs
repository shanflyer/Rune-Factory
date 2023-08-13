using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.IO;

[System.Serializable]
public enum AnimalStatus
{
    正常=0,
    饥饿=1,
    高兴=2,
    悲伤=3,
    死亡=4
}

[System.Serializable]
public enum AgeStatus
{
    幼年=0,
    成年=1,
    老年=2
}
public class Animal :Charactor
{
    public AnimalData animalData;
    public int nowAge;
    public int hungerDays;
    public AnimalStatus animalStatus;
    public AgeStatus ageStatus;
    public int oldProduceDays;
    public int totalProduceCount;
    public Pasture pasture;
    public Animal() { }

 
    public Animal(AnimalSaveData animalSaveData,Pasture _pasture)
    {
        
        animalData = GameComponentData.gameData.pastureAction.AnimalDatas.Find(a => a.animalId == animalSaveData.id/1000);
        nowAge = animalSaveData.nowAge;
        hungerDays = animalSaveData.hungerDays;
        animalStatus = animalSaveData.animalStatus;
        ageStatus = animalSaveData.ageStatus;
        oldProduceDays = animalSaveData.oldProduceDays;
        totalProduceCount = animalSaveData.totalProduceCount;
        pasture = _pasture;
        id = animalSaveData.id;
        Name = animalSaveData.name;
        mapId = animalSaveData.mapId;
        coordinate=new Vector2Int(animalSaveData.coordinateX,animalSaveData.coordinateY);
        professionData =
            GameComponentData.gameData.charactorDataAction.professionDatas0.Find(p => p.id == animalData.professionId);
        level = animalSaveData.level;
        property = professionData.ZeroProperty+professionData.GetPropertyFromLevel(level);
        property.HP = animalSaveData.Hp;
        property.EXP = animalSaveData.exp;
    }
    public Animal(int _id, string _name, GameObject _Obj, int _mapId, Vector2Int _coordinate,
         ProfessionData _professionData,int _level,AnimalData _animalData,int _nowAge,int _hugerDays,AnimalStatus _animalStatus,Pasture _pasture) : 
        base(_id, _name, _Obj, _mapId, _coordinate,_professionData,_level)
    {
        totalProduceCount = 0;
        animalData = _animalData;
        nowAge = _nowAge;
        hungerDays = _hugerDays;
        animalStatus = _animalStatus;
        oldProduceDays = 0;
        pasture = _pasture;
        ageStatus=AgeStatus.成年;
        property = professionData.ZeroProperty + professionData.GetPropertyFromLevel(level);

    }
 
    public void DayUpData()
    {
        property.HP = property.MaxHP;
        nowAge++;
        if (nowAge >= animalData.deathAge)
        {
            MonoBehaviour.Destroy(Obj);
            animalStatus = AnimalStatus.死亡;

            InformationController.instance.AddInformation(
                LanguageManage.SwitchStr("*一只动物") + Name + LanguageManage.SwitchStr("死亡"));
            pasture.animalCaseCount -= animalData.caseCount;

           // CreatItem();
        }
        else
        {
            if (nowAge >= animalData.oldAge && ageStatus != AgeStatus.老年)
            {
                ageStatus = AgeStatus.老年;

            }
            if (nowAge >= animalData.youngAge && ageStatus != AgeStatus.成年)
            {
                ageStatus = AgeStatus.成年;

            }
            oldProduceDays++;
            int prodeceCDPlus = 1;
            if (animalStatus == AnimalStatus.悲伤)
            {
                prodeceCDPlus = 2;
            }
            if (animalStatus == AnimalStatus.饥饿)
            {
                hungerDays++;
                oldProduceDays = 0;
            }
            else
            {
                if (oldProduceDays >= animalData.produceCD * prodeceCDPlus)
                {
                    oldProduceDays = 0;
                    CreatItem();
                }
            }
            if (hungerDays >= animalData.oldAge / 6)
            {
                MonoBehaviour.Destroy(Obj);
                animalStatus = AnimalStatus.死亡;

                InformationController.instance.AddInformation(LanguageManage.SwitchStr("*一只动物") + Name + LanguageManage.SwitchStr("饿死。"));
                pasture.animalCaseCount -= animalData.caseCount;

               // CreatItem();
            }
        }
        
        
    }
    public async void CreatItem()
    {
        int itemCount = 1;
        if (animalStatus == AnimalStatus.高兴)
        {
            itemCount = 2;
           
        }
        ItemData itemData=new ItemData();
        if (animalStatus ==AnimalStatus.死亡)
        {
            //itemData = await GameDataManager.instance.GetAsyncObjectData<ItemData>(animalData.deadItem);
        }
        else
        {
            itemData =await GameDataManager.instance.GetAsyncObjectData<ItemData>(animalData.produceItem.ToString()); 
        }
        if (itemData.id != 0)
        {
            Item item = ItemManager.instance.CreatItem(itemData.id,itemCount);
            PackageManager.instance.SetItemInPackage(item, pasture.itemPackage); 
            totalProduceCount += itemCount;
            GameComponentData.gameData.charactorTitleAction.AddAnimaltExp(1);

            if (animalStatus == AnimalStatus.死亡)
            {
                //InformationController.instance.AddInformation("*动物 " + Name + " 死后剩下" + itemCount + "个" + itemData.Name);
            }
            else
            {
                InformationController.instance.AddInformation("*"+LanguageManage.SwitchStr("动物") + Name + LanguageManage.SwitchStr("产出")
                    + itemCount + LanguageManage.SwitchStr("个") + itemData.name);
            }
        }
       

    }
}

[System.Serializable]
public enum CameraPosType
{
    左=0,
    中=1,
    右=2
}
[System.Serializable]
public class AnimalData
{
    public string name;
    public int animalId;
    public int professionId;
    public int shopItem;
    public string image;
    public string childObj,youngObj,oldObj;
    public int youngAge, oldAge;
    public int startAge;
    public int deathAge;
    public int produceItem;
    public int produceCD;
    public int caseCount;
}
[System.Serializable]
public class Pasture
{
    public int id;
    public string name;
    public int mapid;
    public Vector2Int restCoordinate;
    public Vector2Int startCoordinate, endCoordinate;
    public List<Animal> Animals;
    public int itemPackage;
    public int grassCount;
    public int caseCount;
    public int animalCaseCount;

    public Pasture()
    {
        Animals=new List<Animal>();
    }

    public void RemoveAnimal(int animalId)
    {
        Animal animal = Animals.Find(a => a.id == animalId);
        Animals.Remove(animal);
        animalCaseCount -= animal.animalData.caseCount;
    }
    public void InitPasture(PastureSaveData pastureSaveData)
    {
        grassCount = pastureSaveData.grassCount;
        caseCount = pastureSaveData.caseCount;
        animalCaseCount = 0;
        itemPackage = pastureSaveData.packageId;
         
        name = pastureSaveData.name;
        if (Animals != null)
        {
            foreach (var animal in Animals)
            {
                MonoBehaviour.Destroy(animal.Obj);
            }
            Animals.Clear();
        }
        else
        {
            Animals=new List<Animal>();
        }
        
        foreach (var animalSaveData in pastureSaveData.animalSaveDatas)
        {
            Animal animal=new Animal(animalSaveData,this);
            animalCaseCount += animal.animalData.caseCount;
            Animals.Add(animal);
        }


    }
    public void GrassCostAction()
    {
        if (Animals != null)
        {
            int costGrass = 0;
            foreach (var animal in Animals)
            {
                //animal.nowAge++;
                animal.animalStatus = AnimalStatus.饥饿;
                if (grassCount > 0)
                {
                    animal.animalStatus = AnimalStatus.正常;
                    grassCount--;
                    costGrass++;
                    animal.hungerDays = 0;
                }
                else
                {
                    InformationController.instance.AddInformation("*" + name + LanguageManage.SwitchStr(" 的 ")
                        + animal.Name + LanguageManage.SwitchStr(" 处于饥饿状态。"));
                }
            }
            if (costGrass > 0)
            {
                InformationController.instance.AddInformation("*" + name + LanguageManage.SwitchStr(" 消耗牧草:")
                    + costGrass + LanguageManage.SwitchStr(",剩余牧草:") + grassCount);
            }
           
        }
       
    }
}
public class PastureAction : MonoBehaviour
{
    public float leftPosx, rightPosx;
    private CameraPosType cameraPosType;
    public GameObject MoveCameraButtonObj;
    public Button LeftButton, RightButton;
    public int zeroAddCase, addcasePlus;
    public int zeroAnimalCount, zeroAnimalCost, addAnimalCostPlus;
    public GameObject GrassPanel;
    public InputField pastureNameInput;
    public GameObject PastureBuildPanel;
    public bool[] IsPastures={false,false,false,false};
    public int goldCost0;
    public Text goldCostText;
    public List<Pasture> Pastures;
    public float costPlaus;
    private int goldCost;
    [HideInInspector]
    public int pastureNum;
    private List<GameObject> UnBuildPanels;
    private List<GameObject> tipsList;
    private List<GameObject> Houses;
    public List<AnimalData> AnimalDatas;
    public int pastureIndex;

    public GameObject PastureItemPanel;

    public GameObject PasturePanel;
	// Use this for initialization
	void Start ()
	{
	    JsonToData();

	}
    public void DataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/" + "AnimalDatas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(AnimalDatas);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset file = Resources.Load<TextAsset>("Datas/AnimalDatas");
        if (file != null)
        {
            AnimalDatas = JsonMapper.ToObject<List<AnimalData>>(file.text);

            foreach (var animalData in AnimalDatas)
            {
                animalData.name = LanguageManage.SwitchStr(animalData.name);
                
            }
        }
        else
        {
            Debug.Log(file.name + "不存在");
        }


    }
    public void DayUpdata()
    {
        foreach (var pasture in Pastures)
        {
            pasture.GrassCostAction();
            if (pasture.Animals != null)
            {
                foreach (var pastureAnimal in pasture.Animals)
                {
                    pastureAnimal.DayUpData();
                }
                var deadAnimals = pasture.Animals.FindAll(a => a.animalStatus == AnimalStatus.死亡);
                if (deadAnimals != null)
                {
                    foreach (var deadAnimal in deadAnimals)
                    {
                        GameComponentData.gameData.employerManger.AnimalDead(deadAnimal.id);
                        GameComponentData.gameData.pasturePanelAction.AnimalDeadCheck(deadAnimal);
                        pasture.Animals.Remove(deadAnimal);
                    }
                }
               
            }
        }
        if (GameComponentData.gameData.pasturePanelAction.gameObject.activeSelf)
        {
            GameComponentData.gameData.pasturePanelAction.UpdataPasturePanelData();
        }
        
    }
    public void HideAnimal()
    {
        foreach (var pasture in Pastures)
        {
            if (pasture.Animals != null)
            {
                foreach (var animal in pasture.Animals)
                {
                    if (animal.Obj != null)
                    {
                        animal.Obj.SetActive(false);
                    }
                    
                }
            }
           
        }
    }
    public void DisplayAnimal()
    {
        foreach (var pasture in Pastures)
        {
            if (pasture.Animals != null)
            {
                foreach (var animal in pasture.Animals)
                {
                    if (animal.Obj != null)
                    {
                        animal.Obj.SetActive(true);
                        animal.Obj.GetComponent<NPCAnimationAction>().MoveRandom();
                    }
                    else
                    {
                        Vector3 pos = AStarTest.CoordinateToPos(animal.coordinate);
                        animal.Obj = CreatAnimalObj(animal.animalData, pos, 0);

                    }

                    List<Cell> RangeCells = new List<Cell>();
                    for (int i = pasture.startCoordinate.x; i <= pasture.endCoordinate.x; i++)
                    {
                        for (int j = pasture.startCoordinate.y; j <= pasture.endCoordinate.y; j++)
                        {
                            Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                            RangeCells.Add(cell);
                        }
                    }
                    for (int i = pasture.startCoordinate.x; i <= pasture.endCoordinate.x; i++)
                    {
                        for (int j = pasture.startCoordinate.y; j <= pasture.endCoordinate.y; j++)
                        {
                            Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                            RangeCells.Add(cell);
                        }
                    }
                    animal.Obj.GetComponent<NPCAnimationAction>().InitAnimalAnimationData(animal,
                        GameComponentData.gameData.peopleAction.costTime, RangeCells);

                    animal.Obj.GetComponent<NPCAnimationAction>().MoveRandom();


                }
            }
           
        }
    }
    public void ClickAnimal(int index)
    {
        if (IsPastures[index])
        {
            AnimalData _animalData = AnimalDatas.Find(a => a.animalId == 2000);
            ProfessionData _professionData = CharactorDataAction.professionDatas.Find(p => p.id == 2000);
            Pasture _pasture = Pastures[index];
            int animalCaseCount = 0;
            if (_pasture.Animals != null)
            {
                foreach (var pastureAnimal in _pasture.Animals)
                {
                    animalCaseCount += pastureAnimal.animalData.caseCount;
                }
            }
            if (animalCaseCount < _pasture.caseCount)
            {
                CreatAnimal(_animalData, _pasture, _professionData);
            }
           
        }
        
    }

    public void CreatAnimal(Pasture _pasture,AnimalData _animalData, int count)
    {
        if (_pasture.Animals == null)
        {
            _pasture.Animals = new List<Animal>();
        }
        for (int index = 0; index < count; index++)
        {
            Vector2Int coordinate = new Vector2Int(Random.Range(_pasture.startCoordinate.x, _pasture.endCoordinate.x), Random.Range(_pasture.startCoordinate.y, _pasture.endCoordinate.y));
            List<Animal> x=new List<Animal>();
            foreach (var pasture1 in Pastures)
            {
                if (pasture1.Animals != null)
                {
                    x.AddRange(pasture1.Animals);
                }
                
            }
            List<int> ids = new List<int>();
            foreach (var animal in x)
            {
                ids.Add(animal.id);
            }
            
            var ids0 = ids.FindAll(i => i / 1000 == _animalData.animalId);
            ids0.Sort();
            int animalId = _animalData.animalId * 1000;
            if (ids0.Count != 0)
            {
                animalId = ids0[ids0.Count - 1] + 1;
            }
            ProfessionData _professionData= CharactorDataAction.professionDatas.Find(p => p.id == _animalData.professionId);
            
            Animal _animal = new Animal(animalId, _animalData.name, null, _pasture.mapid, coordinate,
                _professionData,1, _animalData, 0, 0, AnimalStatus.正常, _pasture);
            _pasture.animalCaseCount += _animalData.caseCount;
            _pasture.Animals.Add(_animal);
            GameComponentData.gameData.employerManger.AddAnimal(_animal);
        }
        
    }
    public void MoveAnimalToPasture(Pasture _pasture,Animal _animal)
    {
        _animal.Obj.GetComponent<NPCAnimationAction>().StopCoroutine("AnimalMoving");
        if (_pasture.Animals == null)
        {
            _pasture.Animals = new List<Animal>();
        }
        Vector2Int coordinate = new Vector2Int(Random.Range(_pasture.startCoordinate.x, _pasture.endCoordinate.x), Random.Range(_pasture.startCoordinate.y, _pasture.endCoordinate.y));

        List<Cell> RangeCells = new List<Cell>();
        for (int i = _pasture.startCoordinate.x; i <= _pasture.endCoordinate.x; i++)
        {
            for (int j = _pasture.startCoordinate.y; j <= _pasture.endCoordinate.y; j++)
            {
                Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                RangeCells.Add(cell);
            }
        }
        for (int i = _pasture.startCoordinate.x; i <= _pasture.endCoordinate.x; i++)
        {
            for (int j = _pasture.startCoordinate.y; j <= _pasture.endCoordinate.y; j++)
            {
                Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                RangeCells.Add(cell);
            }
        }
        _animal.rangeCells = RangeCells;
        _animal.Obj.GetComponent<NPCAnimationAction>().AnimalCells = RangeCells;
        _animal.coordinate = coordinate;
        _animal.Obj.transform.position = AStarTest.CoordinateToPos(coordinate);
        _animal.pasture.Animals.Remove(_animal);
        _animal.pasture.animalCaseCount -= _animal.animalData.caseCount;
        _animal.pasture = _pasture;
        _pasture.Animals.Add(_animal);
        _pasture.animalCaseCount += _animal.animalData.caseCount;
        _animal.Obj.GetComponent<NPCAnimationAction>().MoveRandom();
        InformationController.instance.AddInformation("* "+_animal.Name+" 已经移动到 "+_pasture.name);
        
    }
    
    public void CreatAnimal(AnimalData _animalData, Pasture _pasture,ProfessionData _professionData)
    {
        if (_pasture.Animals == null)
        {
            _pasture.Animals=new List<Animal>();
        }
        Vector2Int coordinate=new Vector2Int(Random.Range(_pasture.startCoordinate.x,_pasture.endCoordinate.x), Random.Range(_pasture.startCoordinate.y, _pasture.endCoordinate.y));
        
        List<Cell> RangeCells=new List<Cell>();
        for (int i = _pasture.startCoordinate.x; i <= _pasture.endCoordinate.x; i++)
        {
            for (int j = _pasture.startCoordinate.y; j <= _pasture.endCoordinate.y; j++)
            {
                Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                RangeCells.Add(cell);
            }
        }
        for (int i = _pasture.startCoordinate.x; i <= _pasture.endCoordinate.x; i++)
        {
            for (int j = _pasture.startCoordinate.y; j <= _pasture.endCoordinate.y; j++)
            {
                Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                RangeCells.Add(cell);
            }
        }

        List<Animal> x = _pasture.Animals.FindAll(a => a.id / 1000 == _animalData.animalId);
        List<int> ids=new List<int>();
        foreach (var animal in x)
        {
            ids.Add(animal.id);
        }
        ids.Sort();
        int animalId = _animalData.animalId * 1000;
        if (ids.Count != 0)
        {
            animalId = ids[ids.Count - 1] + 1;
        }
        GameObject animalObj=null;
        Vector3 pos = AStarTest.CoordinateToPos(coordinate);
        animalObj = CreatAnimalObj(_animalData, pos, 0);
         Animal _animal=new Animal(animalId,_animalData.name, animalObj,_pasture.mapid,coordinate,
            _professionData,1,_animalData,0,0,AnimalStatus.正常,_pasture);
        animalObj.GetComponent<NPCAnimationAction>().InitAnimalAnimationData(_animal,GameComponentData.gameData.peopleAction.costTime,RangeCells);
        _pasture.Animals.Add(_animal);
    }

    public void ClickItemPackage(Pasture pasture)
    {
        PastureItemPanel.SetActive(true);
        PastureItemPanel.GetComponent<PastureItemPanelAction>().InitPastureItemPanelData(pasture);
    }
    
    public GameObject CreatAnimalObj(AnimalData _animalData,Vector3 pos,int Age)
    {
        string objName = "";
        if (Age >= _animalData.oldAge)
        {
            objName = _animalData.oldObj;
        }
        else if (Age >= _animalData.youngAge)
        {
            objName = _animalData.youngObj;
        }
        else
        {
            objName = _animalData.childObj;
        }
        GameObject animalPro = Resources.Load<GameObject>("charactor/" + objName);
        GameObject animalObj = Instantiate(animalPro, pos, Quaternion.identity);
        animalObj.transform.SetParent(GameComponentData.gameData.NpcParent);
        return animalObj;
    }
    public void PanelInputPasturnName(InputField inputField)
    {
        string nameStr = inputField.text;
        nameStr = InputFieldAction.Ctr(nameStr, 15);
        inputField.text = nameStr;
    }
    public void InitPastureData(Transform buildParent)
    {
        UnBuildPanels = new List<GameObject>();
        tipsList=new List<GameObject>();
       
        Houses=new List<GameObject>();
        foreach (Transform child in buildParent.parent.GetChild(3))
        {
            child.gameObject.SetActive(true);
        }
        foreach (Transform child in buildParent.parent.GetChild(2))
        {
            child.gameObject.SetActive(true);
            tipsList.Add(child.gameObject);
        }
       
        foreach (Transform child in buildParent.parent.GetChild(4))
        {
            child.gameObject.SetActive(true);
            Houses.Add(child.gameObject);
        }
        foreach (Transform build in buildParent)
        {
            build.gameObject.SetActive(true);
            UnBuildPanels.Add(build.gameObject);
        }
        for (int i = 0; i < IsPastures.Length; i++)
        {
            UnBuildPanels[i].SetActive(!IsPastures[i]);
            //eventObjs[i].SetActive(IsPastures[i]);
            tipsList[i].SetActive(IsPastures[i]);
            Houses[i].SetActive(IsPastures[i]);
        }

        MoveCameraButtonObj.SetActive(true);
        cameraPosType=CameraPosType.中;
        Camera.main.transform.position=new Vector3(0,0,-10);
        LeftButton.gameObject.SetActive(true);
        RightButton.gameObject.SetActive(true);
    }

    public void MoveToRight()
    {
        switch (cameraPosType)
        {
            case CameraPosType.中:
                cameraPosType=CameraPosType.右;
                GameComponentData.gameData.cameraMove.MoveX(new Vector3(rightPosx,0,-10));
                LeftButton.gameObject.SetActive(true);
                RightButton.gameObject.SetActive(false);
                break;
            case CameraPosType.左:
                cameraPosType=CameraPosType.中;
                GameComponentData.gameData.cameraMove.MoveX(new Vector3(0, 0, -10));
                LeftButton.gameObject.SetActive(true);
                RightButton.gameObject.SetActive(true);
                break;
            case CameraPosType.右:
                break;
        }
    }
    public void MoveToLeft()
    {
        switch (cameraPosType)
        {
            case CameraPosType.中:
                cameraPosType = CameraPosType.左;
                GameComponentData.gameData.cameraMove.MoveX(new Vector3(leftPosx, 0, -10));
                LeftButton.gameObject.SetActive(false);
                RightButton.gameObject.SetActive(true);
                break;
            case CameraPosType.右:
                cameraPosType = CameraPosType.中;
                GameComponentData.gameData.cameraMove.MoveX(new Vector3(0, 0, -10));
                LeftButton.gameObject.SetActive(true);
                RightButton.gameObject.SetActive(true);
                break;
            case CameraPosType.左:
                break;
        }
    }

    public void MoveToCenter()
    {
        switch (cameraPosType)
        {
            case CameraPosType.中:
                
                break;
            case CameraPosType.左:
                cameraPosType = CameraPosType.中;
                GameComponentData.gameData.cameraMove.MoveX(new Vector3(0, 0, -10));
                LeftButton.gameObject.SetActive(true);
                RightButton.gameObject.SetActive(true);
                break;
            case CameraPosType.右:
                cameraPosType = CameraPosType.中;
                GameComponentData.gameData.cameraMove.MoveX(new Vector3(0, 0, -10));
                LeftButton.gameObject.SetActive(true);
                RightButton.gameObject.SetActive(true);
                break;
        }
    }
    public void ClickBuild(GameObject obj)
    {
        AudioController.instance.PlayAudio(SE.Click2);
        pastureIndex = int.Parse(obj.name);
        PastureBuildPanel.SetActive(true);
        pastureNameInput.text = LanguageManage.SwitchStr("新牧场");
        
        goldCost = Mathf.RoundToInt(goldCost0 * Mathf.Pow(1 + costPlaus, pastureNum));
        
        goldCostText.text = goldCost.ToString();

    }

    public void ClickGrassObj(Pasture pasture)
    {
        GrassPanel.SetActive(true);
        GrassPanel.GetComponent<PastureGrassAddPanelAction>().InitPanelData(pasture);
    }
 
    public void Builded()
    {
        AudioController.instance.PlayAudio(SE.click);
        
        GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("新建牧场"),goldCost,LanguageManage.SwitchStr("新建一座牧场"),CostType.增加牧场, ShopMoneyType.金币);      


    }

    public void BuildSucessful()
    {
        UnBuildPanels[pastureIndex].SetActive(false);
        IsPastures[pastureIndex] = true;
        tipsList[pastureIndex].SetActive(true);
        Houses[pastureIndex].SetActive(true);
        GameComponentData.gameData.mapParent.GetComponentInChildren<PastureNameClick>().SetPastureName(pastureIndex, pastureNameInput.text);
        Pastures[pastureIndex].name = pastureNameInput.text;
        Pastures[pastureIndex].caseCount = zeroAnimalCount;
        pastureNum++;
        InformationController.instance.AddInformation(LanguageManage.SwitchStr("*新建牧场:") + Pastures[pastureIndex].name + LanguageManage.SwitchStr(",消耗:金币") + goldCost);
    }
    public void ClickPastureHouse(GameObject Obj)
    {
        AudioController.instance.PlayAudio(SE.click);
        int index = Houses.FindIndex(h => h == Obj);
        Pasture pasture = Pastures[index];
        PasturePanel.SetActive(true);
        PasturePanel.GetComponent<PasturePanelAction>().InitPasturePanelData(pasture);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
