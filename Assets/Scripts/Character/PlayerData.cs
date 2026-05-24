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
}

public class CommonSaveData
{
    public int saveVersion;
    public int diamond;
}
[Serializable]
public class UserGameSaveData : IReferenceData
{
    public UserGameSaveData() 
    {
        otherSaveData = new OtherSaveData
        {
            playerPackages = new List<int>(),

        };
        packageSaveDatas = new List<PackageSaveData>();
        endGuideFilmIndex = -1;
        friendSaveData = new FriendSaveData
        {
            friendShips = new List<int3>(),
            friendAdds = new List<int4>()
        };
    }
    public UserGameSaveData(UserGameSaveData userGameSaveData)
    {
        playerData = new CharacterSaveData(userGameSaveData.playerData);
        otherSaveData = new OtherSaveData(userGameSaveData.otherSaveData);
        dateData = userGameSaveData.dateData;
        characterSaveDatas.CopyData(userGameSaveData.characterSaveDatas); 
        packageSaveDatas.AddRange(userGameSaveData.packageSaveDatas);
        friendSaveData = userGameSaveData.friendSaveData;
        mapItemOperates.AddRange(userGameSaveData.mapItemOperates);
        removeCollider.AddRange(userGameSaveData.removeCollider);
        mapLineSaveData.CopyData(userGameSaveData.mapLineSaveData);
        chapters.CopyData(userGameSaveData.chapters);
        fishSaveDatas.CopyData(userGameSaveData.fishSaveDatas);
        plantSaveDatas.CopyData(userGameSaveData.plantSaveDatas);
        changeMapItems.AddRange(userGameSaveData.changeMapItems);
        animationStateMapItems.AddRange(userGameSaveData.animationStateMapItems);
        mapHomeEquips.CopyData(userGameSaveData.mapHomeEquips);
        animals.CopyData(userGameSaveData.animals);
        pastures.CopyData(userGameSaveData.pastures);
        manufatures.CopyData(userGameSaveData.manufatures);
        for (var i = 0; i < userGameSaveData.storeCounters.Count; i++)
            storeCounters.Add(new StoreCounterSaveData(userGameSaveData.storeCounters[i]));
        fields.CopyData(userGameSaveData.fields);

        for (var i = 0; i < userGameSaveData.shopList.Count; i++)
            shopList.Add(new ShopListSaveData(userGameSaveData.shopList[i]));
        saveTime = userGameSaveData.saveTime;

        specialMapItemList.Clear();
        specialMapItemList.AddRange(userGameSaveData.specialMapItemList);

        nextWeathers.Clear();
        nowWeathers.Clear();
        nextWeathers.AddRange(userGameSaveData.nextWeathers);
        nowWeathers.AddRange(userGameSaveData.nowWeathers);

        openFormulas.AddRange(userGameSaveData.openFormulas);

        NpcTimeData.AddRange(userGameSaveData.NpcTimeData); 
        endGuideFilmIndex = userGameSaveData.endGuideFilmIndex;
        playerStoreOpen = userGameSaveData.playerStoreOpen;
    }
    public void NewPlayer()
    {

    }
    public string saveTime;
    public int index;

    public int endGuideFilmIndex;
    public int playerStoreOpen;
    public CharacterSaveData playerData=new CharacterSaveData();
    public OtherSaveData otherSaveData=new OtherSaveData();
    public GameDateSaveData dateData;
    public IntCharacterSaveDataDictionary characterSaveDatas = new IntCharacterSaveDataDictionary();
    public List<PackageSaveData> packageSaveDatas = new List<PackageSaveData>(); 

    public List<Weather> nowWeathers = new List<Weather>();
    public List<Weather> nextWeathers = new List<Weather>();

    public FriendSaveData friendSaveData;
    public List<int> NpcTimeData = new();
    public IntChapterSaveDictionary chapters = new IntChapterSaveDictionary();
    public IntIntDictionary mapLineSaveData = new IntIntDictionary();

    public IntPlantSaveDataDictionary plantSaveDatas = new IntPlantSaveDataDictionary();
    public IntFishSaveDataDataDictionary fishSaveDatas = new IntFishSaveDataDataDictionary(); //OK
    public IntHomeEquipSaveDataDictionary mapHomeEquips = new IntHomeEquipSaveDataDictionary();
    public IntAnimalSaveDataDictionary animals = new IntAnimalSaveDataDictionary();
    public IntPastureSaveDataDictionary pastures = new IntPastureSaveDataDictionary();
    public IntManufatureSaveDataDictionary manufatures = new IntManufatureSaveDataDictionary();
    public List<StoreCounterSaveData> storeCounters = new();

    public IntFieldSaveDataDictionary fields = new();
    public List<ShopListSaveData> shopList = new();
    public List<ulong> mapItemCoordinates = new();

    public List<int> openFormulas = new List<int>();
    public List<long> changeMapItems = new();
    public List<int> animationStateMapItems = new();

    public List<int> removeCollider = new();
    public List<int> mapItemOperates = new();
    public List<long> specialMapItemList = new();

    public Dictionary<int, int2> AnimationStateMapItemsDic
    {
        get
        {
            if (animationStateMapItemsDic.Count == 0 && animationStateMapItems.Count != 0)
                for (var i = 0; i < animationStateMapItems.Count; i++)
                {
                    var value = DataPacker.IntUnpackInt2(animationStateMapItems[i]);
                    if (value.x == 0) continue;
                    var _valueX = value.y / 10;
                    var _valueY = value.y - _valueX * 10;
                    animationStateMapItemsDic[value.x] = new int2(_valueX, _valueY);
                }

            return animationStateMapItemsDic;
        }
    }

    public Dictionary<int, ChangeMapItemCoordinate> ChangeMapItemCoordinate = new();
    private Dictionary<int, int2> animationStateMapItemsDic = new();
    private Dictionary<int, int3> changeMapItemsDic = new();
    private Dictionary<int, NpcTimeData> NpcTimeDataDic = new();
    
    private HashSet<int> RemoveMapItemColliderSet = new HashSet<int>();
    private HashSet<int2> removeMapItemOperatesSet = new HashSet<int2>();
    private HashSet<int2> addMapItemOperatesSet = new HashSet<int2>();

    private Dictionary<int, List<int>> removeMapItemOperatesDic = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> addMapItemOperatesDic = new Dictionary<int, List<int>>();
    private Dictionary<int2,int> specialMapItem = new Dictionary<int2, int>();


    public bool GetChangeMapItem(int key, out int3 value)
    {
        return changeMapItemsDic.TryGetValue(key, out value);
    }
    public bool GetSpecialMapItem(int2 key,out int value)
    {
        return specialMapItem.TryGetValue(key, out value);
    }
    public int2 GetNpcBirthDay(int npdId)
    {
        if (NpcTimeDataDic.TryGetValue(npdId, out var npcTimeData))
        {
            return new int2(npcTimeData.birthSeason, npcTimeData.birthDay);
        }
        return new int2(-1, -1);
    }

    public int GetNpcSleepHour(int npdId)
    {
        if (NpcTimeDataDic.TryGetValue(npdId, out var npcTimeData)) return npcTimeData.sleepTime;
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
        var d = AnimationStateMapItemsDic;
        playerData.Unpack();
        ChangeMapItemCoordinate = new Dictionary<int, ChangeMapItemCoordinate>();
        for (var i = 0; i < mapItemCoordinates.Count; i++)
        {
            var packed = mapItemCoordinates[i];
            var changeMapItemCoordinate = new ChangeMapItemCoordinate(packed);
            ChangeMapItemCoordinate.Add(changeMapItemCoordinate.instanceId, changeMapItemCoordinate);
        }
        NpcTimeDataDic.Clear();
        for (var i = 0; i < NpcTimeData.Count; i++)
        {
            var npcTimeData = new NpcTimeData();
            npcTimeData.packed = NpcTimeData[i];
            npcTimeData.Unpack();
            NpcTimeDataDic[npcTimeData.id] = npcTimeData;
        }
        
        removeMapItemOperatesDic.Clear();
        mapItemOperates.Clear();

        foreach (var field in fields.Values) field.Unpack();

        RemoveMapItemColliderSet.Clear();
        for (var i = 0; i < removeCollider.Count; i++) RemoveMapItemColliderSet.Add(removeCollider[i]);


        changeMapItemsDic.Clear();
        for (var i = 0; i < changeMapItems.Count; i++)
        {
            var saveData = DataPacker.LongUnpackInt3(changeMapItems[i]);
            changeMapItemsDic.Add(saveData.z, saveData);
        }
        
        removeMapItemOperatesSet.Clear();
        addMapItemOperatesSet.Clear();
        for (var i = 0; i < mapItemOperates.Count; i++)
        {
            var saveValue = mapItemOperates[i];
            if (saveValue > 0)
            {
                var value = DataPacker.IntUnpackInt2(saveValue);
                addMapItemOperatesSet.Add(value);
                if (addMapItemOperatesDic.TryGetValue(value.x, out var list))
                    list.Add(value.y);
                else
                    addMapItemOperatesDic.Add(value.x, new List<int> { value.y });
            }
            else
            {
                var value = DataPacker.IntUnpackInt2(-saveValue);
                removeMapItemOperatesSet.Add(value);
                if (removeMapItemOperatesDic.TryGetValue(value.x, out var list))
                    list.Add(value.y);
                else
                    removeMapItemOperatesDic.Add(value.x, new List<int> { value.y });
            }
            
           
        }
        
        specialMapItem.Clear();
        for(int i = 0; i < specialMapItemList.Count; i++)
        {
            var value = DataPacker.LongUnpackInt3(specialMapItemList[i]);
            specialMapItem[value.xy] = value.z;
        }
    }

    public void SaveData()
    {
        animationStateMapItems.Clear();
        foreach (var animationData in animationStateMapItemsDic)
            animationStateMapItems.Add(DataPacker.Int2PackInt(new int2(animationData.Key,
                animationData.Value.x * 10 + animationData.Value.y)));


        mapItemCoordinates.Clear();
        foreach (var changeMapItemCoordinate in ChangeMapItemCoordinate.Values)
            mapItemCoordinates.Add(changeMapItemCoordinate.Pack());
        NpcTimeData = new List<int>();
        foreach (var value in NpcTimeDataDic.Values)
        {
            value.Pack();
            NpcTimeData.Add(value.packed);
        }
        
        changeMapItems.Clear();
        foreach (var changeMapItem in changeMapItemsDic)
            changeMapItems.Add(DataPacker.Int3PackLong(changeMapItem.Value));

        removeCollider.Clear();
        foreach (var itemId in RemoveMapItemColliderSet)
        {
            removeCollider.Add(itemId);
        }

        mapItemOperates.Clear(); 
        foreach (var id in removeMapItemOperatesSet)
        {
            mapItemOperates.Add(-DataPacker.Int2PackInt(id));
        }
        foreach (var id in addMapItemOperatesSet)
        {
            mapItemOperates.Add(DataPacker.Int2PackInt(id));
        }
        specialMapItemList.Clear();
        foreach (var e in specialMapItem)
        {
            specialMapItemList.Add(DataPacker.Int3PackLong(new int3(e.Key, e.Value)));
        }
        saveTime = DateTime.Now.ToString("s");
    }

    public void SetMapLineData(int id,bool isInit)
    {
        mapLineSaveData[id] = isInit?1:0;
    }

    public void SetMapItemCoordinate(int2 editorKey, int instanceId, int mapInstance, int2 Coordinate)
    {
        AddSpecialMapItem(editorKey, instanceId);
        ChangeMapItemCoordinate[instanceId] = new ChangeMapItemCoordinate(instanceId, mapInstance, Coordinate);
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
    
    public void SetNpcBirthDay(int  npcId,Season season,int day)
    {
        if (!NpcTimeDataDic.TryGetValue(npcId, out var npcSaveData)) npcSaveData = new NpcTimeData();
        npcSaveData.id = npcId;
        npcSaveData.birthSeason = (int)season;
        npcSaveData.birthDay = day;
        NpcTimeDataDic[npcId] = npcSaveData;
    }

    public void SetNpcSleepTime(int npcId, int hour)
    {
        if (!NpcTimeDataDic.TryGetValue(npcId, out var npcSaveData)) npcSaveData = new NpcTimeData();
        npcSaveData.sleepTime = hour;
        NpcTimeDataDic[npcId] = npcSaveData;
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
        if (editorKey.y == 0 || (value.x == 0 && value.y == 0))
            return;
        if (animationStateMapItems == null)
        {
            animationStateMapItems = new List<int>();
            animationStateMapItemsDic = new Dictionary<int, int2>();
        }

        specialMapItem[editorKey] = instanceId;
        AnimationStateMapItemsDic[instanceId] = value;
    }

    public void AddChangeMapItem(int3 value,int2 editorKey,int instanceId)
    {
        if (changeMapItems == null)
        {
            changeMapItems = new List<long>();
            changeMapItemsDic = new Dictionary<int, int3>();
        }
        if (editorKey.y == 0)
            return;
        specialMapItem[editorKey] = instanceId;
        changeMapItemsDic[instanceId] = value; 
    }
}

public class ChangeMapItemCoordinate
{
    public int instanceId;
    public int newMap;
    public int2 newCoordinate;

    public ChangeMapItemCoordinate(int instanceId, int newMap, int2 newCoordinate)
    {
        this.instanceId = instanceId;
        this.newMap = newMap;
        this.newCoordinate = newCoordinate;
    }

    public ChangeMapItemCoordinate(ulong packed)
    {
        instanceId = (int)((packed >> 0) & 0xFFFFF);
        newMap = (int)((packed >> 20) & 0x3FFF);

        var x = (int)((packed >> 34) & 0x7FF);
        var y = (int)((packed >> 45) & 0x7FF);

        // 还原符号（11位二进制补码）
        if ((x & 0x400) != 0) x |= unchecked((int)0xFFFFF800);
        if ((y & 0x400) != 0) y |= unchecked((int)0xFFFFF800);

        newCoordinate = new int2(x, y);
    }

    public ulong Pack()
    {
        ulong packed = 0;

        // instanceId (20bit)
        packed |= (ulong)(instanceId & 0xFFFFF) << 0;

        // newMap (14bit)
        packed |= (ulong)(newMap & 0x3FFF) << 20;

        // newCoordinate.x (11bit, signed)
        var x = newCoordinate.x & 0x7FF; // 保留符号的低11位
        packed |= (ulong)x << 34;

        // newCoordinate.y (11bit, signed)
        var y = newCoordinate.y & 0x7FF;
        packed |= (ulong)y << 45;

        return packed;
    }
}

[Serializable]
public class NpcTimeData
{
    [NonSerialized] public int id; // ≤9999
    [NonSerialized] public int birthSeason; // 1-4
    [NonSerialized] public int birthDay; // 1-30
    [NonSerialized] public int sleepTime; // -1 或 0-24

    public int packed;

    public NpcTimeData()
    {
        sleepTime = -1;
    }
    public void Pack()
    {
        packed = 0;
        packed |= (id & 0x3FFF) << 0; // 14
        packed |= (birthSeason & 0x3) << 14; // 2
        packed |= (birthDay & 0x1F) << 16; // 5

        // sleepTime: 6 位带符号数
        var st = sleepTime;
        if (st < -1 || st > 24)
            throw new ArgumentOutOfRangeException(nameof(sleepTime), "必须在 -1 到 24 之间");

        var encoded = st & 0x3F; // 保留 6 位
        packed |= encoded << 21;
    }

    public void Unpack()
    {
        id = (packed >> 0) & 0x3FFF;
        birthSeason = (packed >> 14) & 0x3;
        birthDay = (packed >> 16) & 0x1F;

        var encoded = (packed >> 21) & 0x3F;

        // 还原 -1 (补码 6 位 111111 = -1)
        if ((encoded & 0x20) != 0) // 6位符号位
            sleepTime = encoded | unchecked((int)0xFFFFFFC0); // 符号扩展
        else
            sleepTime = encoded;
    }
}
public class AnimalSaveData
{
    public string name;
    [NonSerialized] public int instaceId; // ≤ 999999
    [NonSerialized] public int pasture; // ≤ 999999
    [NonSerialized] public int dataId; // ≤ 9999
    [NonSerialized] public int growthStage; // ≤ 9
    [NonSerialized] public int growthDay; // ≤ 999

    [NonSerialized]
    public bool setFood;

    [NonSerialized] public AnimalState animalState; // ≤ 9
    [NonSerialized] public int nowCD; // ≤ 99
    [NonSerialized] public int linkCharacterData; // ≤ 9999

    // 压缩字段
    public ulong data1;
    public ulong data2;

    public void Pack()
    {
        data1 = data2 = 0;

        // data1
        data1 |= (ulong)(instaceId & 0xFFFFF) << 0; // 20
        data1 |= (ulong)(pasture & 0xFFFFF) << 20; // 20
        data1 |= (ulong)(dataId & 0x3FFF) << 40; // 14
        data1 |= (ulong)(growthStage & 0xF) << 54; // 4
        data1 |= (ulong)(growthDay & 0x3F) << 58; // 低6位
        data2 |= (ulong)((growthDay >> 6) & 0xF) << 0; // 高4位

        // data2
        data2 |= (setFood ? 1UL : 0UL) << 4;
        data2 |= ((ulong)animalState & 0xF) << 5;
        data2 |= (ulong)(nowCD & 0x7F) << 9;
        data2 |= (ulong)(linkCharacterData & 0x3FFF) << 16;
    }

    public void Unpack()
    {
        // data1
        instaceId = (int)((data1 >> 0) & 0xFFFFF);
        pasture = (int)((data1 >> 20) & 0xFFFFF);
        dataId = (int)((data1 >> 40) & 0x3FFF);
        growthStage = (int)((data1 >> 54) & 0xF);

        var gdLow = (int)((data1 >> 58) & 0x3F);
        var gdHigh = (int)((data2 >> 0) & 0xF);
        growthDay = (gdHigh << 6) | gdLow;

        // data2
        setFood = ((data2 >> 4) & 0x1) != 0;
        animalState = (AnimalState)((data2 >> 5) & 0xF);
        nowCD = (int)((data2 >> 9) & 0x7F);
        linkCharacterData = (int)((data2 >> 16) & 0x3FFF);
    }

    public AnimalSaveData() { }
    public AnimalSaveData(AnimalSaveData animalSaveData)
    { 
        name = animalSaveData.name;
        data1 = animalSaveData.data1;
        data2 = animalSaveData.data2;
        Unpack();
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
        Pack();
    }
}

public class PastureSaveData
{
    [NonSerialized] public int instanceId; // ≤ 999999
    [NonSerialized] public int linkItem; // ≤ 999999
    [NonSerialized] public PastureState pastureState; // ≤ 9
    [NonSerialized] public int dataId; // ≤ 9999
    [NonSerialized] public int foodPackage; // ≤ 999999
    [NonSerialized] public int productPackage; // ≤ 999999
    [NonSerialized] public int level; // ≤ 9
    [NonSerialized] public int index; // ≤ 9
    [NonSerialized] public int animalCase; // ≤ 99
    [NonSerialized] public int linkRoom; // ≤ 9999
    public string name;

    // 压缩字段
    public ulong data1;
    public ulong data2;

    public void Pack()
    {
        data1 = data2 = 0;

        // data1
        data1 |= (ulong)(instanceId & 0xFFFFF) << 0; // 20
        data1 |= (ulong)(linkItem & 0xFFFFF) << 20; // 20
        data1 |= (ulong)((int)pastureState & 0xF) << 40; // 4
        data1 |= (ulong)(dataId & 0x3FFF) << 44; // 14
        data1 |= (ulong)(foodPackage & 0x3F) << 58; // 低6位
        data2 |= (ulong)((foodPackage >> 6) & 0x3FFF) << 0; // 高14位

        // data2
        data2 |= (ulong)(productPackage & 0xFFFFF) << 14; // 20
        data2 |= (ulong)(level & 0xF) << 34; // 4
        data2 |= (ulong)(index & 0xF) << 38; // 4
        data2 |= (ulong)(animalCase & 0x7F) << 42; // 7
        data2 |= (ulong)(linkRoom & 0x3FFF) << 49; // 14
    }

    public void Unpack()
    {
        // data1
        instanceId = (int)((data1 >> 0) & 0xFFFFF);
        linkItem = (int)((data1 >> 20) & 0xFFFFF);
        pastureState = (PastureState)((data1 >> 40) & 0xF);
        dataId = (int)((data1 >> 44) & 0x3FFF);
        var foodLow = (int)((data1 >> 58) & 0x3F);
        var foodHigh = (int)((data2 >> 0) & 0x3FFF);
        foodPackage = (foodHigh << 6) | foodLow;

        // data2
        productPackage = (int)((data2 >> 14) & 0xFFFFF);
        level = (int)((data2 >> 34) & 0xF);
        index = (int)((data2 >> 38) & 0xF);
        animalCase = (int)((data2 >> 42) & 0x7F);
        linkRoom = (int)((data2 >> 49) & 0x3FFF);
    }
    public PastureSaveData() { }
    public PastureSaveData(PastureSaveData pastureSaveData)
    {
        name = pastureSaveData.name;
        data1 = pastureSaveData.data1;
        data2 = pastureSaveData.data2;
        Unpack();
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
        Pack();
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
 
public class FieldSaveData
{
    [NonSerialized]
    public int instanceId;

    [NonSerialized]
    public int mapInstance;

    [NonSerialized]
    public int2 coordinate;

    [NonSerialized]
    public int editorInstanceId;

    [NonSerialized]
    public FieldState fieldState;

    [NonSerialized]
    public bool isSetWater;

    [NonSerialized]
    public int waterHour;

    [NonSerialized]
    public int PlantinstaceId;

    [NonSerialized]
    public int PlantDataId;

    [NonSerialized]
    public int growthStage;

    [NonSerialized]
    public float growthHour;

    [NonSerialized]
    public PlantState plantState;

    [NonSerialized]
    public int nowCycle;


    // 压缩存储
    public ulong data1;
    public ulong data2;
    public ulong data3;

    // ========= Pack =========
    public void Pack()
    {
        data1 = 0;
        data2 = 0;
        data3 = 0;

        // data1
        data1 |= ((ulong)instanceId & 0xFFFFFUL) << 0; // 20位
        data1 |= ((ulong)mapInstance & 0x3FFFUL) << 20; // 14位
        data1 |= ((ulong)coordinate.x & 0x3FFUL) << 34; // 10位
        data1 |= ((ulong)coordinate.y & 0x3FFUL) << 44; // 10位
        data1 |= (ulong)(editorInstanceId & 0x3FF) << 54; // 低10位

        // data2
        data2 |= ((ulong)(editorInstanceId >> 10) & 0x3FFFUL) << 0; // 高14位
        data2 |= ((ulong)fieldState & 0xFUL) << 14; // 4位
        data2 |= (isSetWater ? 1UL : 0UL) << 18; // 1位
        data2 |= ((ulong)waterHour & 0x7FUL) << 19; // 7位
        data2 |= ((ulong)PlantinstaceId & 0xFFFFFUL) << 26; // 20位
        data2 |= ((ulong)PlantDataId & 0x3FFFUL) << 46; // 14位
        data2 |= ((ulong)growthStage & 0xFUL) << 60; // 4位

        // data3
        var gHourInt = (int)Math.Round(growthHour * 10); // 保留1位小数 → 0~9999
        data3 |= ((ulong)gHourInt & 0x3FFFUL) << 10; // 14位
        data3 |= ((ulong)plantState & 0xFUL) << 24; // 4位
        data3 |= ((ulong)nowCycle & 0xFUL) << 28; // 4位
    }

    // ========= Unpack =========
    public void Unpack()
    {
        // data1
        instanceId = (int)((data1 >> 0) & 0xFFFFFUL);
        mapInstance = (int)((data1 >> 20) & 0x3FFFUL);
        coordinate = new int2(
            (int)((data1 >> 34) & 0x3FFUL),
            (int)((data1 >> 44) & 0x3FFUL));
        var editorLow = (int)((data1 >> 54) & 0x3FF);

        // data2
        var editorHigh = (int)((data2 >> 0) & 0x3FFFUL);
        editorInstanceId = (editorHigh << 10) | editorLow;
        fieldState = (FieldState)((data2 >> 14) & 0xFUL);
        isSetWater = ((data2 >> 18) & 0x1UL) != 0;
        waterHour = (int)((data2 >> 19) & 0x7FUL);
        PlantinstaceId = (int)((data2 >> 26) & 0xFFFFFUL);
        PlantDataId = (int)((data2 >> 46) & 0x3FFFUL);
        growthStage = (int)((data2 >> 60) & 0xFUL);

        // data3
        var gHourInt = (int)((data3 >> 10) & 0x3FFFUL);
        growthHour = gHourInt / 10f;
        plantState = (PlantState)((data3 >> 24) & 0xFUL);
        nowCycle = (int)((data3 >> 28) & 0xFUL);
    }
    public FieldSaveData() { }
    public FieldSaveData(FieldSaveData fieldSaveData)
    {
        data1 = fieldSaveData.data1;
        data2 = fieldSaveData.data2;
        data3 = fieldSaveData.data3;
        Unpack(); 
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

        Pack();
    }
}

public class StoreCounterSaveData
{
    [NonSerialized]
    public int instanceId;

    [NonSerialized]
    public int dataId;

    [NonSerialized]
    public int itemDataId;

    [NonSerialized]
    public int count;

    public long packed;

    public void Pack()
    {
        packed = 0;
        packed |= (count & 0x1FFFFFFL) << 0; // 25位
        packed |= ((long)instanceId & 0xFFFFF) << 25; // 20位
        packed |= ((long)dataId & 0x1F) << 45; // 5位
        packed |= ((long)itemDataId & 0x3FFF) << 50; // 14位
    }

    public void Unpack()
    {
        count = (int)((packed >> 0) & 0x1FFFFFFL);
        instanceId = (int)((packed >> 25) & 0xFFFFF);
        dataId = (int)((packed >> 45) & 0x1F);
        itemDataId = (int)((packed >> 50) & 0x3FFF);
    }
    public StoreCounterSaveData() { }
    public StoreCounterSaveData(StoreCounterSaveData storeCounterSaveData)
    {
        instanceId = storeCounterSaveData.instanceId;
        dataId = storeCounterSaveData.dataId;
        itemDataId = storeCounterSaveData.itemDataId;
        count = storeCounterSaveData.count;
        Pack();
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
        Pack();
    }
}

public class ManufatureSaveData
{
    [NonSerialized] public int instanceId; // ≤999999
    [NonSerialized] public int dataId; // ≤9999
    [NonSerialized] public int2[] materials; // 固定4个, x≤9999, y≤999999
    [NonSerialized] public int3 product; // x≤9999, y≤99999, z≤9999
    [NonSerialized] public int waitTime; // ≤9999999
    [NonSerialized] public int startTime; // ≤9999
    [NonSerialized] public int matchFormula; // ≤9999

    public ulong d1, d2, d3, d4, d5;

    public void Pack()
    {
        d1 = d2 = d3 = d4 = d5 = 0;
        materials = new int2[4];
        // d1
        d1 |= (ulong)(instanceId & 0xFFFFF) << 0; // 20位
        d1 |= (ulong)(dataId & 0x3FFF) << 20; // 14位
        d1 |= (ulong)(materials[0].x & 0x3FFF) << 34; // 14位
        d1 |= (ulong)(materials[0].y & 0xFFFF) << 48; // y低16位
        d2 |= (ulong)((materials[0].y >> 16) & 0xF) << 0; // y高4位

        // d2
        d2 |= (ulong)(materials[1].x & 0x3FFF) << 4;
        d2 |= (ulong)(materials[1].y & 0xFFFFF) << 18;
        d2 |= (ulong)(materials[2].x & 0x3FFF) << 38;
        d2 |= (ulong)(materials[2].y & 0xFFF) << 52; // y低12位
        d3 |= (ulong)((materials[2].y >> 12) & 0xFF) << 0; // y高8位

        // d3
        d3 |= (ulong)(materials[3].x & 0x3FFF) << 8;
        d3 |= (ulong)(materials[3].y & 0xFFFFF) << 22;
        d3 |= (ulong)(product.x & 0x3FFF) << 42;
        d3 |= (ulong)(product.y & 0xFF) << 56; // y低8位
        d4 |= (ulong)((product.y >> 8) & 0x1FF) << 0; // y高9位

        // d4
        d4 |= (ulong)(product.z & 0x3FFF) << 9;
        d4 |= (ulong)(waitTime & 0xFFFFFF) << 23;
        d4 |= (ulong)(startTime & 0x3FFF) << 47;
        d4 |= (ulong)(matchFormula & 0x7) << 61; // 低3位
        d5 |= (ulong)((matchFormula >> 3) & 0x7FF) << 0; // 高11位
    }

    public void Unpack()
    {
        materials = new int2[4];
        // d1
        instanceId = (int)((d1 >> 0) & 0xFFFFF);
        dataId = (int)((d1 >> 20) & 0x3FFF);
        materials[0].x = (int)((d1 >> 34) & 0x3FFF);
        var m0yLow = (int)((d1 >> 48) & 0xFFFF);
        var m0yHigh = (int)((d2 >> 0) & 0xF);
        materials[0].y = (m0yHigh << 16) | m0yLow;

        // d2
        materials[1].x = (int)((d2 >> 4) & 0x3FFF);
        materials[1].y = (int)((d2 >> 18) & 0xFFFFF);
        materials[2].x = (int)((d2 >> 38) & 0x3FFF);
        var m2yLow = (int)((d2 >> 52) & 0xFFF);
        var m2yHigh = (int)((d3 >> 0) & 0xFF);
        materials[2].y = (m2yHigh << 12) | m2yLow;

        // d3
        materials[3].x = (int)((d3 >> 8) & 0x3FFF);
        materials[3].y = (int)((d3 >> 22) & 0xFFFFF);
        product.x = (int)((d3 >> 42) & 0x3FFF);
        var pyLow = (int)((d3 >> 56) & 0xFF);
        var pyHigh = (int)((d4 >> 0) & 0x1FF);
        product.y = (pyHigh << 8) | pyLow;

        // d4
        product.z = (int)((d4 >> 9) & 0x3FFF);
        waitTime = (int)((d4 >> 23) & 0xFFFFFF);
        startTime = (int)((d4 >> 47) & 0x3FFF);
        var mfLow = (int)((d4 >> 61) & 0x7);
        var mfHigh = (int)((d5 >> 0) & 0x7FF);
        matchFormula = (mfHigh << 3) | mfLow;
    }

    public ManufatureSaveData() { }
    public ManufatureSaveData(ManufatureSaveData manufatureSaveData)
    {
        d1 = manufatureSaveData.d1;
        d2 = manufatureSaveData.d2;
        d3 = manufatureSaveData.d3;
        d4 = manufatureSaveData.d4;
        Unpack();
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
        Pack();
    }
}

public class HomeEquipSaveData
{
    [NonSerialized] public int instanceId; // ≤ 999999
    [NonSerialized] public int equipDataId; // ≤ 9999
    [NonSerialized] public int mapEditorInstance; // ≤ 9999999
    [NonSerialized] public int mapInstance; // ≤ 9999
    [NonSerialized] public int2 coordinate; // x,y ≤ 999
    [NonSerialized] public int characterId; // ≤ 999999

    public ulong data1;
    public ulong data2;

    public void Pack()
    {
        data1 = data2 = 0;

        //    data1
        data1 |= (ulong)(instanceId & 0xFFFFF) << 0; // 20
        data1 |= (ulong)(equipDataId & 0x3FFF) << 20; // 14
        data1 |= (ulong)(mapEditorInstance & 0xFFFFFF) << 34; // 24
        data1 |= (ulong)(mapInstance & 0x3F) << 58; // 低6位

        // d2
        data2 |= (ulong)((mapInstance >> 6) & 0xFF) << 0; // 高8位

        // 坐标 (11位二进制补码)
        var x = coordinate.x & 0x7FF;
        var y = coordinate.y & 0x7FF;

        data2 |= (ulong)x << 8; // 11位
        data2 |= (ulong)y << 19; // 11位
        data2 |= (ulong)(characterId & 0xFFFFF) << 30; // 20
    }

    public void Unpack()
    {
        //    data1
        instanceId = (int)((data1 >> 0) & 0xFFFFF);
        equipDataId = (int)((data1 >> 20) & 0x3FFF);
        mapEditorInstance = (int)((data1 >> 34) & 0xFFFFFF);

        var miLow = (int)((data1 >> 58) & 0x3F);
        var miHigh = (int)((data2 >> 0) & 0xFF);
        mapInstance = (miHigh << 6) | miLow;

        // 坐标 (11位补码还原)
        var x = (int)((data2 >> 8) & 0x7FF);
        var y = (int)((data2 >> 19) & 0x7FF);

        if ((x & 0x400) != 0) x |= unchecked((int)0xFFFFF800);
        if ((y & 0x400) != 0) y |= unchecked((int)0xFFFFF800);

        coordinate = new int2(x, y);

        characterId = (int)((data2 >> 30) & 0xFFFFF);
    }

    public HomeEquipSaveData() { }
    public HomeEquipSaveData(HomeEquipSaveData homeEquipSaveData)
    {
        data1 = homeEquipSaveData.data1;
        data2 = homeEquipSaveData.data2;
        Unpack();
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
        Pack();
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
    public int year;
    public Season season;
    public int day;
    public int hour;
    public int minute;
    public Week week;

    public override string ToString()
    {
      return  LanguageManage.instance.GameTimeToString(year, season, day); 
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
    public string name;
    [NonSerialized] public int instanceId; // ≤999999 (20bit)
    [NonSerialized] public int dataId; // ≤9999   (14bit)
    [NonSerialized] public int level; // ≤100    (7bit)
    [NonSerialized] public int exp; // ≤99,999,999 (27bit)
    [NonSerialized] public Gender gender; // 0/1/2   (2bit)
    [NonSerialized] public BrithDay brithDay; // year(14)+season(2)+day(5)
    [NonSerialized] public int packageId; // ≤999999 (20bit)
    [NonSerialized] public int2 weapon; // x(14)+y(7)
    [NonSerialized] public int2 clothes; // x(14)+y(7)
    [NonSerialized] public int2 shoe; // x(14)+y(7)
    [NonSerialized] public int2 headgear; // x(14)+y(7)
    [NonSerialized] public bool isMarried; // 1bit
    [NonSerialized] public int hp; // ≤9999 (14bit)
    [NonSerialized] public int mp; // ≤9999 (14bit)
    [NonSerialized] public int power; // ≤9999 (14bit)
    [NonSerialized] public int sleepHour; // ≤99 (7bit)

    // 压缩存储
    public ulong d1, d2, d3, d4, d5;

    public void Pack()
    {
        d1 = d2 = d3 = d4 = d5 = 0;

        // d1
        d1 |= (ulong)(instanceId & 0xFFFFF) << 0; // 20
        d1 |= (ulong)(dataId & 0x3FFF) << 20; // 14
        d1 |= (ulong)(level & 0x7F) << 34; // 7
        d1 |= (ulong)(exp & 0x7FFFFFF) << 41; // 27

        // d2
        d2 |= ((ulong)gender & 0x3) << 0; // 2
        d2 |= (ulong)(brithDay.year & 0x3FFF) << 2; // 14
        d2 |= (ulong)((int)brithDay.season & 0x3) << 16; // 2
        d2 |= (ulong)(brithDay.day & 0x1F) << 18; // 5
        d2 |= (ulong)(packageId & 0xFFFFF) << 23; // 20
        d2 |= (ulong)(weapon.x & 0x3FFF) << 43; // 14
        d2 |= (ulong)(weapon.y & 0x7F) << 57; // 7

        // d3
        d3 |= (ulong)(clothes.x & 0x3FFF) << 0; // 14
        d3 |= (ulong)(clothes.y & 0x7F) << 14; // 7
        d3 |= (ulong)(shoe.x & 0x3FFF) << 21; // 14
        d3 |= (ulong)(shoe.y & 0x7F) << 35; // 7
        d3 |= (ulong)(headgear.x & 0x3FFF) << 42; // 14
        d3 |= (ulong)(headgear.y & 0x7F) << 56; // 7

        // d4
        d4 |= (isMarried ? 1UL : 0UL) << 0; // 1
        d4 |= (ulong)(hp & 0x3FFF) << 1; // 14
        d4 |= (ulong)(mp & 0x3FFF) << 15; // 14
        d4 |= (ulong)(power & 0x3FFF) << 29; // 14
        d4 |= (ulong)(sleepHour & 0x7F) << 43; // 7
    }

    public void Unpack()
    {
        // d1
        instanceId = (int)((d1 >> 0) & 0xFFFFF);
        dataId = (int)((d1 >> 20) & 0x3FFF);
        level = (int)((d1 >> 34) & 0x7F);
        exp = (int)((d1 >> 41) & 0x7FFFFFF);

        // d2
        gender = (Gender)((d2 >> 0) & 0x3);
        brithDay.year = (int)((d2 >> 2) & 0x3FFF);
        brithDay.season = (Season)((d2 >> 16) & 0x3);
        brithDay.day = (int)((d2 >> 18) & 0x1F);
        packageId = (int)((d2 >> 23) & 0xFFFFF);
        weapon.x = (int)((d2 >> 43) & 0x3FFF);
        weapon.y = (int)((d2 >> 57) & 0x7F);

        // d3
        clothes.x = (int)((d3 >> 0) & 0x3FFF);
        clothes.y = (int)((d3 >> 14) & 0x7F);
        shoe.x = (int)((d3 >> 21) & 0x3FFF);
        shoe.y = (int)((d3 >> 35) & 0x7F);
        headgear.x = (int)((d3 >> 42) & 0x3FFF);
        headgear.y = (int)((d3 >> 56) & 0x7F);

        // d4
        isMarried = ((d4 >> 0) & 0x1) != 0;
        hp = (int)((d4 >> 1) & 0x3FFF);
        mp = (int)((d4 >> 15) & 0x3FFF);
        power = (int)((d4 >> 29) & 0x3FFF);
        sleepHour = (int)((d4 >> 43) & 0x7F);
    }
 
    public CharacterSaveData(CharacterSaveData characterSaveData)
    {
        name = characterSaveData.name;
        d1 = characterSaveData.d1;
        d2 = characterSaveData.d2;
        d3 = characterSaveData.d3;
        d4 = characterSaveData.d4;
        d5 = characterSaveData.d5;
        Unpack();
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

public class PackageSaveData
{
    [NonSerialized] public int caseCount; // ≤200
    [NonSerialized] public int id; // ≤999999 (6位十进制)
    [NonSerialized] public int dataId; // ≤99 (2位十进制)
    [NonSerialized] public int level; // ≤8
    [NonSerialized] public PackageType packageType; // 0/1/2

    [NonSerialized] public bool itemPackage; // bool

    public string packageName; // 不压缩

    // 压缩字段
    public ulong packed;

    public void Pack()
    {
        packed = 0;
        packed |= (ulong)(caseCount & 0xFF) << 0; // 8
        packed |= (ulong)(id & 0xFFFFF) << 8; // 20
        packed |= (ulong)(dataId & 0x7F) << 28; // 7
        packed |= (ulong)(level & 0x7) << 35; // 3
        packed |= (ulong)((int)packageType & 0x3) << 38; // 2
        packed |= (itemPackage ? 1UL : 0UL) << 40; // 1
    }

    public void Unpack()
    {
        caseCount = (int)((packed >> 0) & 0xFF);
        id = (int)((packed >> 8) & 0xFFFFF);
        dataId = (int)((packed >> 28) & 0x7F);
        level = (int)((packed >> 35) & 0x7);
        packageType = (PackageType)((packed >> 38) & 0x3);
        itemPackage = ((packed >> 40) & 0x1) != 0;
    }

    public List<ulong> items;
}

public struct BrithDay
{
    public int year;
    public Season season;
    public int day;
}
