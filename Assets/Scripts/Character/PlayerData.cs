using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class UserGameSaveDataList : IReferenceData
{
    public CommonSaveData commonSaveData;  
    public UserGameSaveData nowSaveData;
    public List<UserGameSaveData> userGameSaveDatas = new List<UserGameSaveData>();
    public UserGameSaveDataList()
    {
        commonSaveData = new CommonSaveData();
    }

    public void UnPack()
    {
    }
}

public class CommonSaveData
{
    public int diamond;
}
[Serializable]
public class UserGameSaveData : IReferenceData
{
    public void UnPack()
    {
        playerData.Unpack();
    }
    public UserGameSaveData() 
    {
        otherSaveData = new OtherSaveData
        {
            playerPackages = new List<int>(),

        };
        packageSaveDatas = new List<PackageSaveData>();
        endGuideFilmIndex = -1;
    }
    public UserGameSaveData(UserGameSaveData userGameSaveData)
    {
        playerData = new CharacterSaveData(userGameSaveData.playerData);
        otherSaveData = new OtherSaveData(userGameSaveData.otherSaveData);
        dateData = userGameSaveData.dateData;
        characterSaveDatas.CopyData(userGameSaveData.characterSaveDatas); 
        packageSaveDatas.AddRange(userGameSaveData.packageSaveDatas);
        friendSaveData = userGameSaveData.friendSaveData;
        removeMapItemOperates.AddRange(userGameSaveData.removeMapItemOperates);
        addMapItemOperates.AddRange(userGameSaveData.addMapItemOperates);
        RemoveMapItemCollider.AddRange(userGameSaveData.RemoveMapItemCollider);
        mapLineSaveData.CopyData(userGameSaveData.mapLineSaveData);
        chapters.CopyData(userGameSaveData.chapters);
        fishSaveDatas.CopyData(userGameSaveData.fishSaveDatas);
        plantSaveDatas.CopyData(userGameSaveData.plantSaveDatas);
        changeMapItems.CopyData(userGameSaveData.changeMapItems);
        SetAnimationStateMapItems.CopyData(userGameSaveData.SetAnimationStateMapItems);
        mapHomeEquips.CopyData(userGameSaveData.mapHomeEquips);
        animals.CopyData(userGameSaveData.animals);
        pastures.CopyData(userGameSaveData.pastures);
        manufatures.CopyData(userGameSaveData.manufatures);
        storeCounters.CopyData(userGameSaveData.storeCounters);
        fields.CopyData(userGameSaveData.fields);
       
        shops.CopyData(userGameSaveData.shops);
        shopLists.CopyData(userGameSaveData.shopLists);
        saveTime = userGameSaveData.saveTime;

        specialMapItemList.Clear();
        specialMapItemList.AddRange(userGameSaveData.specialMapItemList);

        nextWeathers.Clear();
        nowWeathers.Clear();
        nextWeathers.AddRange(userGameSaveData.nextWeathers);
        nowWeathers.AddRange(userGameSaveData.nowWeathers);

        openFormulas.AddRange(userGameSaveData.openFormulas);

        npcBirthDays.CopyData(userGameSaveData.npcBirthDays);
        npcSleepTime.CopyData(userGameSaveData.npcSleepTime);
        endGuideFilmIndex = userGameSaveData.endGuideFilmIndex; 
    }
    public void NewPlayer()
    {

    }
    public string saveTime;
    public int index;

    public int endGuideFilmIndex; 
    public CharacterSaveData playerData=new CharacterSaveData();
    public OtherSaveData otherSaveData=new OtherSaveData();
    public GameDateSaveData dateData;
    public IntCharacterSaveDataDictionary characterSaveDatas = new IntCharacterSaveDataDictionary();
    public List<PackageSaveData> packageSaveDatas = new List<PackageSaveData>(); 

    public List<Weather> nowWeathers = new List<Weather>();
    public List<Weather> nextWeathers = new List<Weather>();

    public FriendSaveData friendSaveData;
    public IntInt2Dictionary npcBirthDays = new IntInt2Dictionary();
    public IntIntDictionary npcSleepTime = new();
    public IntChapterSaveDictionary chapters = new IntChapterSaveDictionary();
    public IntIntDictionary mapLineSaveData = new IntIntDictionary();

    public IntPlantSaveDataDictionary plantSaveDatas = new IntPlantSaveDataDictionary();
    public IntFishSaveDataDataDictionary fishSaveDatas = new IntFishSaveDataDataDictionary(); //OK
    public IntHomeEquipSaveDataDictionary mapHomeEquips = new IntHomeEquipSaveDataDictionary();
    public IntAnimalSaveDataDictionary animals = new IntAnimalSaveDataDictionary();
    public IntPastureSaveDataDictionary pastures = new IntPastureSaveDataDictionary();
    public IntManufatureSaveDataDictionary manufatures = new IntManufatureSaveDataDictionary();
    public IntStoreCounterSaveDataDictionary storeCounters = new IntStoreCounterSaveDataDictionary();
    public IntFieldSaveDataDictionary fields = new IntFieldSaveDataDictionary();
    public IntShopSaveDataDictionary shops = new IntShopSaveDataDictionary();//
    public StringShopListSaveDataDictionary shopLists = new StringShopListSaveDataDictionary();

    public List<int> openFormulas = new List<int>();
    public IntInt3Dictionary changeMapItems = new IntInt3Dictionary();
    public IntInt2Dictionary SetAnimationStateMapItems = new IntInt2Dictionary();
    public List<int2> removeMapItemOperates = new List<int2>();
    public List<int2> addMapItemOperates = new List<int2>(); 
    public List<int> RemoveMapItemCollider = new List<int>();

    public List<int3> specialMapItemList = new List<int3>();


    private HashSet<int> RemoveMapItemColliderSet = new HashSet<int>();
    private HashSet<int2> removeMapItemOperatesSet = new HashSet<int2>();
    private HashSet<int2> addMapItemOperatesSet = new HashSet<int2>();

    private Dictionary<int, List<int>> removeMapItemOperatesDic = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> addMapItemOperatesDic = new Dictionary<int, List<int>>();
    private Dictionary<int2,int> specialMapItem = new Dictionary<int2, int>();

    public bool GetSpecialMapItem(int2 key,out int value)
    {
        return specialMapItem.TryGetValue(key, out value);
    }
    public int2 GetNpcBirthDay(int npdId)
    {
        if(npcBirthDays.TryGetValue(npdId,out var int2))
        {
            return int2;
        }
        return new int2(-1, -1);
    }

    public int GetNpcSleepHour(int npdId)
    {
        if (npcSleepTime.TryGetValue(npdId, out var hour)) return hour;
        return -1;
    }
    public void InitMapItemSaveData(int id)
    {
        if(removeMapItemOperatesDic.TryGetValue(id,out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                RemoveMapItemOperate removeMapItemOperate = new RemoveMapItemOperate
                {
                    mapItemId = id,
                    removeOperateId = list[i]
                };
                GameActionManager.instance.QueueAction(removeMapItemOperate);
            }
        }
        if(addMapItemOperatesDic.TryGetValue(id,out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                AddMapItemOperate addMapItemOperate = new AddMapItemOperate
                {
                    mapItemId = id,
                    addeOperateId = list[i]
                };
                GameActionManager.instance.QueueAction(addMapItemOperate);
            }
        }
        if (RemoveMapItemColliderSet.Contains(id))
        {
            RemoveMapItemCollider removeMapItemCollider = new RemoveMapItemCollider
            {
                mapItemInstanceId = id
            };
            GameActionManager.instance.QueueAction(removeMapItemCollider);
        }
    }

    public void Init()
    {
        removeMapItemOperatesDic.Clear();
        addMapItemOperates.Clear();
        

        RemoveMapItemColliderSet.Clear();
        for (int i = 0; i < RemoveMapItemCollider.Count; i++)
        {
            RemoveMapItemColliderSet.Add(RemoveMapItemCollider[i]);
        }
        removeMapItemOperatesSet.Clear();
        addMapItemOperatesSet.Clear();

       
        for (int i = 0; i < removeMapItemOperates.Count; i++)
        {
            removeMapItemOperatesSet.Add(removeMapItemOperates[i]);
            if (removeMapItemOperatesDic.TryGetValue(removeMapItemOperates[i].x,out var list))
            {
                list.Add(removeMapItemOperates[i].y);
            }
            else
            {
                removeMapItemOperatesDic.Add(removeMapItemOperates[i].x,new List<int> { removeMapItemOperates[i].y});
            }
        }
        for (int i = 0; i < addMapItemOperates.Count; i++)
        {
            addMapItemOperatesSet.Add(addMapItemOperates[i]);
            if (addMapItemOperatesDic.TryGetValue(addMapItemOperates[i].x, out var list))
            {
                list.Add(addMapItemOperates[i].y);
            }
            else
            {
                addMapItemOperatesDic.Add(addMapItemOperates[i].x, new List<int> { addMapItemOperates[i].y });
            }
        }
        specialMapItem.Clear();
        for(int i = 0; i < specialMapItemList.Count; i++)
        {
            specialMapItem[specialMapItemList[i].xy] = specialMapItemList[i].z;
        }
    }

    public void SaveData()
    {
        RemoveMapItemCollider.Clear();
        foreach (var itemId in RemoveMapItemColliderSet)
        {
            RemoveMapItemCollider.Add(itemId);
        }
        removeMapItemOperates.Clear();
        addMapItemOperates.Clear();
        foreach (var id in removeMapItemOperatesSet)
        {
            removeMapItemOperatesSet.Add(id);
        }
        foreach (var id in addMapItemOperatesSet)
        {
            addMapItemOperates.Add(id);
        }
        specialMapItemList.Clear();
        foreach (var e in specialMapItem)
        {
            specialMapItemList.Add(new int3(e.Key.xy, e.Value));
        }
        saveTime = DateTime.Now.ToString("s");
    }

    public void SetMapLineData(int id,bool isInit)
    {
        mapLineSaveData[id] = isInit?1:0;
    }

    public void RemoveMapItemOperate(int3 itemOperate,int instanceId)
    {
        if (itemOperate.y == 0)
            return;
        specialMapItem[itemOperate.xy] = instanceId;
        int2 value = new int2(instanceId, itemOperate.z);
        removeMapItemOperatesSet.Add(value);
        addMapItemOperatesSet.Remove(value);
    }

    public void AddMapItemOperate(int3 itemOperate, int instanceId)
    {
        if (itemOperate.y == 0)
            return;
        specialMapItem[itemOperate.xy] = instanceId;
        int2 value=new int2(instanceId,itemOperate.z);
        addMapItemOperatesSet.Add(value);
        removeMapItemOperatesSet.Remove(value);
    }

    public void AddRemoveMapItemColliderData(int2 id, int instanceId)
    {
        if (id.y == 0)
            return;
        specialMapItem[id] = instanceId;
        RemoveMapItemColliderSet.Add(instanceId);
    }
    public void SaveSpecialMapItem(int2 id,int instanceId)
    {
        specialMapItem[id] = instanceId;
    }
    public void AddReSetMapItemColliderData(int2 id, int instanceId)
    {
        if (id.y == 0)
            return;
        specialMapItem[id] = instanceId;
        RemoveMapItemColliderSet.Remove(instanceId);
    }

    public void ReMoveHomeEquip(int id)
    {
        mapHomeEquips.Remove(id);
    }

    public void SetFightChapter(FightChapter fightChapter)
    {
        if (chapters.TryGetValue(fightChapter.mapId, out var chapterSave))
        {
            chapterSave.SetChapterSave(fightChapter);
        }
        else
        {
            chapterSave = new ChapterSave(fightChapter);
            chapters.Add(fightChapter.mapId, chapterSave);
        }
    }

    public void SetPastureData(Pasture pasture)
    {
        if (pastures.TryGetValue(pasture.instanceId, out var pastureSaveData))
        {
            pastureSaveData.SetParture(pasture);
        }
        else
        {
            pastureSaveData = new PastureSaveData(pasture);
            pastures.Add(pastureSaveData.instanceId, pastureSaveData);
        }
    }

    public void DeletePasture(int id)
    {
        pastures.Remove(id);
    }

    public void DeleteAnimal(int id)
    {
        animals.Remove(id);
    }

    public void SetAnimalData(Animal animal)
    {
        if (animals.TryGetValue(animal.instanceId, out var animalSaveData))
        {
            animalSaveData.SetAnimal(animal);
        }
        else
        {
            animalSaveData = new AnimalSaveData(animal);
            animals.Add(animal.instanceId, animalSaveData);
        }
    }

    public void SetFieldData(Field field)
    {
        if (fields.TryGetValue(field.instanceId, out var fieldSaveData))
        {
            fieldSaveData.SetField(field);
        }
        else
        {
            fieldSaveData = new FieldSaveData(field);
            fields.Add(field.instanceId, fieldSaveData);
        }
    }
    public void SetShopSaveData(Shop shop)
    {
        if(shops.TryGetValue(shop.shopId,out var shopSaveData))
        {
            shopSaveData.SetData(shop);
        }
        else
        {
            shopSaveData = new ShopSaveData(shop);
            shops.Add(shop.shopId, shopSaveData);
        }
    }
    public void SetNpcBirthDay(int  npcId,Season season,int day)
    {
        npcBirthDays[npcId] = new int2((int)season, day);
    }

    public void SetNpcSleepTime(int npcId, int hour)
    {
        npcSleepTime[npcId] = hour;
    }
    public void SetMapHomeEquipData(HomeEquip homeEquip)
    {
        int key = homeEquip.instanceId;

        if (mapHomeEquips.TryGetValue(key, out var homeEquipSaveData))
        {
            homeEquipSaveData.SetHomeEquip(homeEquip);
        }
        else
        {
            homeEquipSaveData = new HomeEquipSaveData(homeEquip);
            mapHomeEquips.Add(key, homeEquipSaveData);
        }
    }
     
    public void SetManufature(Manufature manufature)
    {
        if (manufatures.TryGetValue(manufature.instanceId, out var manufatureSaveData))
        {
            manufatureSaveData.SetManufature(manufature);
        }
        else
        {
            manufatureSaveData = new ManufatureSaveData(manufature);
            manufatures.Add(manufature.instanceId, manufatureSaveData);
        }
    }

    public void DeleteStoreCounter(int id)
    {
        storeCounters.Remove(id);
    }

    public void SetStoreCounterSaveData(RuntimeStoreCounter runtimeStoreCounter)
    {
        if (storeCounters.TryGetValue(runtimeStoreCounter.instanceId, out var storeCounterSaveData))
        {
            storeCounterSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
        }
        else
        {
            storeCounterSaveData = new StoreCounterSaveData(runtimeStoreCounter);
            storeCounters.Add(runtimeStoreCounter.instanceId, storeCounterSaveData);
        }
    }

    public static UserGameSaveData CreatSaveData(int index)
    {
        OtherSaveData otherSaveData = new OtherSaveData
        {
            playerPackages = new List<int>(),
            
        };

        UserGameSaveData userGameSaveData = new UserGameSaveData
        {
            otherSaveData = otherSaveData, 
            packageSaveDatas = new List<PackageSaveData>(), 
            index = index,
            endGuideFilmIndex=-1
        };

        return userGameSaveData;
    }

    public void AddSpecialMapItem(int2 editorKey, int instanceId)
    {
        if (editorKey.y == 0)
            return;
        specialMapItem[editorKey] = instanceId;
    }
    public void AddAnimationStateMapItem(int2 value,int2 editorKey,int instanceId)
    {
        if (editorKey.y == 0)
            return;
        specialMapItem[editorKey] = instanceId; 
        SetAnimationStateMapItems[instanceId] = value;
    }

    public void AddChangeMapItem(int3 value,int2 editorKey,int instanceId)
    {
        if (changeMapItems == null)
        {
            changeMapItems = new IntInt3Dictionary();
        }
        if (editorKey.y == 0)
            return;
        specialMapItem[editorKey] = instanceId;
        changeMapItems[instanceId] = value;
    }
}

public class AnimalSaveData
{
    public int instaceId;
    public string name;
    public int pasture;
    public int dataId;
    public int growthStage;
    public int growthDay;
    public bool setFood;
    public AnimalState animalState;
    public int nowCD;
    public int linkCharacterData;

    public AnimalSaveData() { }
    public AnimalSaveData(AnimalSaveData animalSaveData)
    {
        instaceId = animalSaveData.instaceId;
        name = animalSaveData.name;
        pasture = animalSaveData.pasture;
        growthStage = animalSaveData.growthStage;
        growthDay = animalSaveData.growthDay;
        setFood = animalSaveData.setFood;
        animalState = animalSaveData.animalState;
        nowCD = animalSaveData.nowCD;
        linkCharacterData = animalSaveData.linkCharacterData;
        dataId = animalSaveData.dataId;
    }
    public AnimalSaveData(Animal animal)
    {
        SetAnimal(animal);
    }

    public void SetAnimal(Animal animal)
    {
        instaceId = animal.instanceId;
        name = animal.name;
        pasture = animal.pasture;
        growthStage = animal.growthStage;
        growthDay = animal.growthDay;
        setFood = animal.setFood;
        animalState = animal.animalState;
        nowCD = animal.nowCD;
        linkCharacterData = animal.linkCharacterData;
        dataId = animal.animalData.id;
    }
}

public class PastureSaveData
{
    public int instanceId;
    public string name;
    public int linkItem;
    public PastureState pastureState;
    public int dataId;
    public int foodPackage;
    public int productPackage;

    public int level;
    public int index;
    public int animalCase;
    public int linkRoom;
    public PastureSaveData() { }
    public PastureSaveData(PastureSaveData pastureSaveData)
    {
        name = pastureSaveData.name;
        instanceId = pastureSaveData.instanceId;
        level = pastureSaveData.level;
        index = pastureSaveData.index;
        linkItem = pastureSaveData.linkItem;
        pastureState = pastureSaveData.pastureState;
        dataId = pastureSaveData.dataId;
        foodPackage = pastureSaveData.foodPackage;
        productPackage = pastureSaveData.productPackage;
        linkRoom = pastureSaveData.linkRoom;
    }
    public PastureSaveData(Pasture pasture)
    {
        SetParture(pasture);
    }

    public void SetParture(Pasture pasture)
    {
        name = pasture.name;
        instanceId = pasture.instanceId;
        level = pasture.level;
        index = pasture.index;
        linkItem = pasture.linkItem;
        pastureState = pasture.pastureState;
        dataId = pasture.pastureData.id;
        foodPackage = pasture.foodPackage;
        productPackage = pasture.productPackage;
        linkRoom = pasture.linkRoom;
    }
}
public class ShopListSaveData
{
    public string name;
    public List<int> binders = new List<int>();
    public ShopListSaveData(ShopList shopList)
    {
        name = shopList.groupName;
        binders.AddRange(shopList.bindCharacters);
    }
    public ShopListSaveData(ShopListSaveData shopListSaveData)
    {
        name=shopListSaveData.name;
        binders.AddRange(shopListSaveData.binders);
    }
    public void SetData(ShopList shopList)
    {
        binders.Clear();
        binders.AddRange(shopList.bindCharacters);
    }
} 
public class ShopSaveData
{
    public int shopId;
    public List<int> openItems=new List<int>(); 
    public ShopSaveData(Shop shop) 
    { 
        shopId = shop.shopId;
        SetData(shop);
    }
    public ShopSaveData(ShopSaveData shopSaveData)
    {
        shopId = shopSaveData.shopId;
        openItems.AddRange(shopSaveData.openItems);
    }
    public void SetData(Shop shop)
    {
        openItems.Clear();
        var shopItems= shop.GetOpenShopItem();
        for(int i = 0; i < shopItems.Count; i++)
        {
            openItems.Add(shopItems[i].item);
        }
    }
}
public class FieldSaveData
{
    public int instanceId;
    public int mapInstance;
    public int2 coordinate;
    public int editorInstanceId;
    public FieldState fieldState;
    public bool isSetWater;
    public int waterHour;

    public int PlantinstaceId;
    public int PlantDataId;
    public int growthStage;
    public float growthHour;
    public PlantState plantState;
    public int nowCycle;
    public FieldSaveData() { }
    public FieldSaveData(FieldSaveData fieldSaveData)
    {
        instanceId = fieldSaveData.instanceId;
        mapInstance = fieldSaveData.mapInstance;
        editorInstanceId = fieldSaveData.editorInstanceId;
        fieldState = fieldSaveData.fieldState;
        isSetWater = fieldSaveData.isSetWater;
        waterHour = fieldSaveData.waterHour;

        PlantinstaceId = fieldSaveData.PlantinstaceId;
        PlantDataId = fieldSaveData.PlantDataId;
        growthStage = fieldSaveData.growthStage;
        growthHour = fieldSaveData.growthHour;
        plantState = fieldSaveData.plantState;
        nowCycle = fieldSaveData.nowCycle;
        coordinate = fieldSaveData.coordinate;
    }
    public FieldSaveData(Field field)
    {
        SetField(field);
    }

    public void SetField(Field field)
    {
        instanceId = field.instanceId;
        mapInstance = field.mapInstance;
        editorInstanceId = field.editorInstanceId;
        fieldState = field.fieldState;
        isSetWater = field.isSetWater;
        coordinate = field.coordinate;
        waterHour = field.waterHour;
        if (field.plant == null)
        {
            PlantinstaceId = 0;
            PlantDataId = 0;
        }
        else
        {
            PlantinstaceId = field.plant.instanceId;
            PlantDataId = field.plant.PlantData.id;
            growthStage = field.plant.growthStage;
            growthHour = field.plant.growthHour;
            plantState = field.plant.plantState;
            nowCycle = field.plant.nowCycle;
        }
    }
}

public class StoreCounterSaveData
{
    public int instanceId;
    public int dataId;
    public int itemDataId;
    public int count;
    public StoreCounterSaveData() { }
    public StoreCounterSaveData(StoreCounterSaveData storeCounterSaveData)
    {
        instanceId = storeCounterSaveData.instanceId;
        dataId = storeCounterSaveData.dataId;
        itemDataId = storeCounterSaveData.itemDataId;
        count = storeCounterSaveData.count;
    }
    public StoreCounterSaveData(RuntimeStoreCounter runtimeStoreCounter)
    {
        SetStoreCounterSaveData(runtimeStoreCounter);
    }

    public void SetStoreCounterSaveData(RuntimeStoreCounter runtimeStoreCounter)
    {
        instanceId = runtimeStoreCounter.instanceId;
        dataId = runtimeStoreCounter.storeCounterData.id;
        if (runtimeStoreCounter.itemData != null)
        {
            itemDataId = runtimeStoreCounter.itemData.id;
        }

        count = runtimeStoreCounter.count;
    }
}

public class ManufatureSaveData
{
    public int instanceId;
    public int dataId;
    public int2[] materials;
    public int3 product;
    public int waitTime;
    public int startTime;
    public int matchFormula; 

    public ManufatureSaveData() { }
    public ManufatureSaveData(ManufatureSaveData manufatureSaveData)
    {
        instanceId = manufatureSaveData.instanceId;
        dataId = manufatureSaveData.dataId;
        materials = manufatureSaveData.materials.ToArray();
        product = manufatureSaveData.product;
        waitTime = manufatureSaveData.waitTime;
        startTime = manufatureSaveData.startTime;
        matchFormula = manufatureSaveData.matchFormula; 
    }
    public ManufatureSaveData(Manufature manufature)
    {
        SetManufature(manufature);
    }

    public void SetManufature(Manufature manufature)
    {
        instanceId = manufature.instanceId;
        dataId = manufature.dataId;
        materials = manufature.materials.ToArray();
        product = manufature.product;
        waitTime = manufature.waitTime;
        startTime = manufature.startTime; 
    }
}

public class HomeEquipSaveData
{
    public int instanceId;
    public int equipDataId;
    public int mapEditorInstance;
    public int mapInstance;
    public int2 coordinate;
    public int characterId;

    public HomeEquipSaveData() { }
    public HomeEquipSaveData(HomeEquipSaveData homeEquipSaveData)
    {
        instanceId = homeEquipSaveData.instanceId;
        equipDataId = homeEquipSaveData.equipDataId;
        mapEditorInstance = homeEquipSaveData.mapEditorInstance; mapInstance = homeEquipSaveData.mapInstance;
        coordinate = homeEquipSaveData.coordinate;
        characterId = homeEquipSaveData.characterId;
    }
    public HomeEquipSaveData(HomeEquip homeEquip)
    {
        SetHomeEquip(homeEquip);
    }

    public void SetHomeEquip(HomeEquip homeEquip)
    {
        instanceId = homeEquip.instanceId;
        equipDataId = homeEquip.equipDataId;
        mapEditorInstance = homeEquip.mapEditorInstance; mapInstance = homeEquip.mapInstance;
        coordinate = homeEquip.coordinate;
        characterId = homeEquip.characterId;
    }
}

public class PlantSaveData
{
    public int dataId;
    public int fruitCount;
    public PlantSaveData() { }
    public PlantSaveData(PlantSaveData plantSaveData)
    {
        this.dataId = plantSaveData.dataId;
        this.fruitCount = plantSaveData.fruitCount;
    }
}
public class FishSaveData
{
    public int dataId;
    public int length;
    public List<int> places;
    public FishSaveData() { }
    public FishSaveData(FishSaveData fishSaveData)
    {
        dataId = fishSaveData.dataId;
        length = fishSaveData.length;
        places = new List<int>();
        places.CopyTo(fishSaveData.places.ToArray());
    }
}

public struct GameDateSaveData
{
    [NonSerialized]
    public int year;

    [NonSerialized]
    public Season season;

    [NonSerialized]
    public int day;

    [NonSerialized]
    public int hour;

    [NonSerialized]
    public int minute;

    [NonSerialized]
    public Week week;

    // 压缩字段
    public ulong packed;

    public void Pack()
    {
        packed = 0;
        packed |= ((ulong)year & 0xFFFUL) << 0; // 12位
        packed |= ((ulong)season & 0x3UL) << 12; // 2位
        packed |= ((ulong)day & 0x1FUL) << 14; // 5位
        packed |= ((ulong)hour & 0x1FUL) << 19; // 5位
        packed |= ((ulong)minute & 0x3FUL) << 24; // 6位
        packed |= ((ulong)week & 0x7UL) << 30; // 3位
    }

    public void Unpack()
    {
        year = (int)((packed >> 0) & 0xFFFUL);
        season = (Season)((packed >> 12) & 0x3UL);
        day = (int)((packed >> 14) & 0x1FUL);
        hour = (int)((packed >> 19) & 0x1FUL);
        minute = (int)((packed >> 24) & 0x3FUL);
        week = (Week)((packed >> 30) & 0x7UL);
    }

    public override string ToString()
    {
        return LanguageManage.instance.GameTimeToString(year, season, day);
    }
}


public class ChapterSave
{
    public int mapId;
    public List<int> findItems;
    public bool open;
    public bool completed;
    public ChapterSave() { }
    public ChapterSave(ChapterSave chapterSave)
    {
        mapId = chapterSave.mapId;
        open = chapterSave.open;
        completed = chapterSave.completed;
        findItems = new List<int>();
        findItems.CopyTo(chapterSave.findItems.ToArray());
    }
    public ChapterSave(FightChapter fightChapter)
    {
        SetChapterSave(fightChapter);
    }

    public void SetChapterSave(FightChapter fightChapter)
    {
        mapId = fightChapter.mapId;
        findItems = fightChapter.findItems.ToList();
        open = fightChapter.open;
        completed = fightChapter.completed;
    }
}

public class OtherSaveData
{
    public int gold;
    public List<int> playerPackages;
    public bool isMarriedFood, isAnMo;
    public bool playerStoreOpen;
    
    public int uid;
    public int newDayActionIndex, newWakeUpActionIndex;
    public List<int2> shortcutItems;
    public OtherSaveData() { }
    public OtherSaveData(OtherSaveData otherSaveData)
    {
        uid = otherSaveData.uid;
        gold = otherSaveData.gold;
        playerStoreOpen = otherSaveData.playerStoreOpen;
        
        playerPackages = new List<int>();
        playerPackages.AddRange(otherSaveData.playerPackages);
        isMarriedFood = otherSaveData.isMarriedFood;
        isAnMo = otherSaveData.isAnMo;
        newDayActionIndex = otherSaveData.newDayActionIndex;
        newWakeUpActionIndex = otherSaveData.newWakeUpActionIndex;
        if (otherSaveData.shortcutItems != null)
        {
            shortcutItems = new List<int2>();
            shortcutItems.AddRange(otherSaveData.shortcutItems);
        }
      
    } 
}

public struct FriendSaveData
{
    public List<int3> friendShips;
    public List<int4> friendAdds;
}

public class CharacterSaveData : IReferenceData
{
    // ========= 原始字段 =========
    public string name;
    public int instanceId;
    public int packageId;
    public int exp;

    [NonSerialized]
    public int dataId;

    [NonSerialized]
    public int level;

    [NonSerialized]
    public Gender gender;

    [NonSerialized]
    public BrithDay brithDay;

    [NonSerialized] public int2 weapon, clothes, shoe, headgear;

    [NonSerialized]
    public bool isMarried;

    [NonSerialized]
    public int hp, mp, power;

    [NonSerialized]
    public int sleepHour;

    // ========= 压缩存储 =========
    public ulong baseData; // 基础属性 (61 位)
    public ulong equipData1; // 武器 + 衣服 (38 位)
    public ulong equipData2; // 鞋子 + 头饰 + 生日 (45 位)

    // ========= 打包 =========
    public void Pack()
    {
        // --- 基础字段 ---
        baseData = 0;
        baseData |= ((ulong)dataId & 0x1FFFUL) << 0; // 13位
        baseData |= ((ulong)level & 0x7FUL) << 13; // 7位
        baseData |= ((ulong)gender & 0x3UL) << 20; // 2位 (animal/male/female)
        baseData |= ((ulong)hp & 0x7FFUL) << 22; // 11位
        baseData |= ((ulong)mp & 0x7FFUL) << 33; // 11位
        baseData |= ((ulong)power & 0x7FFUL) << 44; // 11位
        baseData |= ((ulong)sleepHour & 0x1FUL) << 55; // 5位
        baseData |= (isMarried ? 1UL : 0UL) << 60; // 1位

        // --- 装备字段 1 (weapon + clothes) ---
        equipData1 = 0;
        equipData1 |= ((ulong)weapon.x & 0xFFFUL) << 0; // 12位
        equipData1 |= ((ulong)weapon.y & 0x7FUL) << 12; // 7位
        equipData1 |= ((ulong)clothes.x & 0xFFFUL) << 19; // 12位
        equipData1 |= ((ulong)clothes.y & 0x7FUL) << 31; // 7位

        // --- 装备字段 2 (shoe + headgear + birthday) ---
        equipData2 = 0;
        equipData2 |= ((ulong)shoe.x & 0xFFFUL) << 0; // 12位
        equipData2 |= ((ulong)shoe.y & 0x7FUL) << 12; // 7位
        equipData2 |= ((ulong)headgear.x & 0xFFFUL) << 19; // 12位
        equipData2 |= ((ulong)headgear.y & 0x7FUL) << 31; // 7位
        equipData2 |= ((ulong)brithDay.season & 0x3UL) << 38; // 2位
        equipData2 |= ((ulong)brithDay.day & 0x1FUL) << 40; // 5位
    }

    // ========= 解包 =========
    public void Unpack()
    {
        // --- 基础字段 ---
        dataId = (int)((baseData >> 0) & 0x1FFFUL);
        level = (int)((baseData >> 13) & 0x7FUL);
        gender = (Gender)((baseData >> 20) & 0x3UL);
        hp = (int)((baseData >> 22) & 0x7FFUL);
        mp = (int)((baseData >> 33) & 0x7FFUL);
        power = (int)((baseData >> 44) & 0x7FFUL);
        sleepHour = (int)((baseData >> 55) & 0x1FUL);
        isMarried = ((baseData >> 60) & 0x1UL) != 0;

        // --- 装备字段 1 ---
        weapon = new int2(
            (int)((equipData1 >> 0) & 0xFFFUL),
            (int)((equipData1 >> 12) & 0x7FUL));
        clothes = new int2(
            (int)((equipData1 >> 19) & 0xFFFUL),
            (int)((equipData1 >> 31) & 0x7FUL));

        // --- 装备字段 2 ---
        shoe = new int2(
            (int)((equipData2 >> 0) & 0xFFFUL),
            (int)((equipData2 >> 12) & 0x7FUL));
        headgear = new int2(
            (int)((equipData2 >> 19) & 0xFFFUL),
            (int)((equipData2 >> 31) & 0x7FUL));

        // --- 生日 ---
        brithDay.season = (Season)((equipData2 >> 38) & 0x3UL);
        brithDay.day = (int)((equipData2 >> 40) & 0x1FUL);
    }
 
    public CharacterSaveData(CharacterSaveData characterSaveData)
    {
        name = characterSaveData.name;
        dataId = characterSaveData.dataId;
        level = characterSaveData.level;
        exp = characterSaveData.exp;
        gender = characterSaveData.gender;
        brithDay = characterSaveData.brithDay;
        packageId = characterSaveData.packageId;
        weapon = characterSaveData.weapon;
        clothes = characterSaveData.clothes;
        shoe = characterSaveData.shoe;
        isMarried = characterSaveData.isMarried;
        hp = characterSaveData.hp;
        mp = characterSaveData.mp;
        power = characterSaveData.power;
        sleepHour = characterSaveData.sleepHour;
        Pack();
    }
    public CharacterSaveData()
    { }

    public CharacterSaveData(Character character)
    {
        SetCharacter(character); 
    }

    public void SetCharacter(Character character,string overrideName=null)
    {
        if (string.IsNullOrEmpty(overrideName))
        {
            name = character.name;
        }
        else
        {
            name = overrideName;
        }
        
        instanceId = character.instanceId;
        dataId = character.dataId;
        level = character.Level;
        exp = character.exp.nowExp;
        packageId = character.characterPackage;
        weapon = character.Equip.weapon;
        clothes = character.Equip.clothes;
        shoe = character.Equip.shoes;
        gender = character.characterData.gender;
        hp = character.CharacterProperty.HP;
        mp = character.CharacterProperty.MP;
        power = character.CharacterProperty.Power;

        Pack();
    }
}

public struct PackageSaveData
{
    public int id; // 不压缩
    public string packageName; // 不压缩
    [NonSerialized] public int caseCount; // ≤100 
    [NonSerialized] public int dataId; // ≤5000
    [NonSerialized] public int level; // ≤8 

    [NonSerialized]
    public bool itemPackage;
    public List<Item> items;

    // 压缩字段
    public uint packed;

    public void Pack()
    {
        packed = 0;
        packed |= (uint)(caseCount & 0x7F) << 0; // 7位
        packed |= (uint)(dataId & 0x1FFF) << 7; // 13位
        packed |= (uint)(level & 0xF) << 20; // 4位
        packed |= (uint)(itemPackage ? 1 : 0) << 24; // 1位
    }

    public void Unpack()
    {
        caseCount = (int)((packed >> 0) & 0x7F);
        dataId = (int)((packed >> 7) & 0x1FFF);
        level = (int)((packed >> 20) & 0xF);
        itemPackage = ((packed >> 24) & 0x1) != 0;
    }
}

public struct BrithDay
{ 
    public Season season;
    public int day;
}