using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;

public class UserGameSaveDataList : IReferenceData
{
    public UserGameSaveData nowSaveData;
    public List<UserGameSaveData> userGameSaveDatas = new List<UserGameSaveData>();
}

public class UserGameSaveData : IReferenceData
{

    public UserGameSaveData() { }
    public UserGameSaveData(UserGameSaveData userGameSaveData)
    {
        playerData = new CharacterSaveData(userGameSaveData.playerData);
        otherSaveData = new OtherSaveData(userGameSaveData.otherSaveData);
        dateData = userGameSaveData.dateData;
        characterSaveDatas.Clear();
        foreach(var data in userGameSaveData.characterSaveDatas)
        {
            characterSaveDatas.Add(new CharacterSaveData(data));
        }
        packageSaveDatas.CopyTo(userGameSaveData.packageSaveDatas.ToArray());
        friendSaveData = userGameSaveData.friendSaveData;
        removeMapItemOperates.CopyTo(userGameSaveData.removeMapItemOperates.ToArray());
        addMapItemOperates.CopyTo(userGameSaveData.addMapItemOperates.ToArray());
        RemoveMapItemCollider.CopyTo(userGameSaveData.RemoveMapItemCollider.ToArray());
        deleteMapLine.CopyTo(userGameSaveData.deleteMapLine.ToArray());
        chapters.CopyData(userGameSaveData.chapters);
        fishSaveDatas.CopyData(userGameSaveData.fishSaveDatas);
        changeMapItems.CopyData(userGameSaveData.changeMapItems);
        SetAnimationStateMapItems.CopyData(userGameSaveData.SetAnimationStateMapItems);
        mapHomeEquips.CopyData(userGameSaveData.mapHomeEquips);
        animals.CopyData(userGameSaveData.animals);
        pastures.CopyData(userGameSaveData.pastures);
        manufatures.CopyData(userGameSaveData.manufatures);
        storeCounters.CopyData(userGameSaveData.storeCounters);
        fields.CopyData(userGameSaveData.fields);
        saveTime = userGameSaveData.saveTime;
    }

    public string saveTime;
    public int index;

    public CharacterSaveData playerData;
    public OtherSaveData otherSaveData;
    public GameDateSaveData dateData;
    public List<CharacterSaveData> characterSaveDatas = new List<CharacterSaveData>();
    public List<PackageSaveData> packageSaveDatas = new List<PackageSaveData>();

    public FriendSaveData friendSaveData;
    public IntChapterSaveDictionary chapters = new IntChapterSaveDictionary();
    public IntFishSaveDataDataDictionary fishSaveDatas = new IntFishSaveDataDataDictionary();
    public IntInt4Dictionary changeMapItems=new IntInt4Dictionary();
    public IntInt3Dictionary SetAnimationStateMapItems=new IntInt3Dictionary();
    public IntHomeEquipSaveDataDictionary mapHomeEquips = new IntHomeEquipSaveDataDictionary();
    public IntAnimalSaveDataDictionary animals = new IntAnimalSaveDataDictionary();
    public IntPastureSaveDataDictionary pastures = new IntPastureSaveDataDictionary();
    public IntManufatureSaveDataDictionary manufatures = new IntManufatureSaveDataDictionary();
    public IntStoreCounterSaveDataDictionary storeCounters = new IntStoreCounterSaveDataDictionary();
    public IntFieldSaveDataDictionary fields = new IntFieldSaveDataDictionary();
    public List<int2> removeMapItemOperates = new List<int2>();
    public List<int2> addMapItemOperates = new List<int2>();

    public List<int> RemoveMapItemCollider = new List<int>();
    public List<int> deleteMapLine = new List<int>();

   

    private HashSet<int> RemoveMapItemColliderSet = new HashSet<int>();
    private HashSet<int2> removeMapItemOperatesSet = new HashSet<int2>();
    private HashSet<int2> addMapItemOperatesSet = new HashSet<int2>();

    public void Init()
    {
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
        }
        for (int i = 0; i < addMapItemOperates.Count; i++)
        {
            addMapItemOperatesSet.Add(addMapItemOperates[i]);
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
        saveTime = DateTime.Now.ToSafeString();
    }

    public void RemoveMapItemOperate(int2 itemOperate)
    {
        removeMapItemOperatesSet.Add(itemOperate);
        addMapItemOperatesSet.Remove(itemOperate);
    }

    public void AddMapItemOperate(int2 itemOperate)
    {
        addMapItemOperatesSet.Add(itemOperate);
        removeMapItemOperatesSet.Remove(itemOperate);
    }

    public void AddRemoveMapItemColliderData(int id)
    {
        RemoveMapItemColliderSet.Add(id);
    }

    public void AddReSetMapItemColliderData(int id)
    {
        RemoveMapItemColliderSet.Remove(id);
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
        if (animals.TryGetValue(animal.instaceId, out var animalSaveData))
        {
            animalSaveData.SetAnimal(animal);
        }
        else
        {
            animalSaveData = new AnimalSaveData(animal);
            animals.Add(animal.instaceId, animalSaveData);
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
            playerPackages = new List<int>()
        };

        UserGameSaveData userGameSaveData = new UserGameSaveData
        {
            otherSaveData = otherSaveData,
            characterSaveDatas = new List<CharacterSaveData>(),
            packageSaveDatas = new List<PackageSaveData>(), 
            index = index
        };

        return userGameSaveData;
    }

    public void AddAnimationStateMapItem(int3 data)
    {
        if (SetAnimationStateMapItems == null)
        {
            SetAnimationStateMapItems = new IntInt3Dictionary();
        }
        SetAnimationStateMapItems[data.x] = data;
    }

    public void AddChangeMapItem(int4 value)
    {
        if (changeMapItems == null)
        {
            changeMapItems = new IntInt4Dictionary();
        }

        changeMapItems[value.x] = value;
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
    public int nowCycle;
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
        nowCycle = animalSaveData.nowCycle;
        linkCharacterData = animalSaveData.linkCharacterData;
        dataId = animalSaveData.dataId;
    }
    public AnimalSaveData(Animal animal)
    {
        SetAnimal(animal);
    }

    public void SetAnimal(Animal animal)
    {
        instaceId = animal.instaceId;
        name = animal.name;
        pasture = animal.pasture;
        growthStage = animal.growthStage;
        growthDay = animal.growthDay;
        setFood = animal.setFood;
        animalState = animal.animalState;
        nowCycle = animal.nowCycle;
        linkCharacterData = animal.linkCharacterData;
        dataId = animal.animalData.id;
    }
}

public class PastureSaveData
{
    public int instanceId;
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

public class FieldSaveData
{
    public int instanceId;
    public int mapInstance;
    public int editorInstanceId;
    public FieldState fieldState;
    public bool isSetWater;

    public int PlantinstaceId;
    public int PlantDataId;
    public int growthStage;
    public int growthDay;
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

        PlantinstaceId = fieldSaveData.PlantinstaceId;
        PlantDataId = fieldSaveData.PlantDataId;
        growthStage = fieldSaveData.growthStage;
        growthDay = fieldSaveData.growthDay;
        plantState = fieldSaveData.plantState;
        nowCycle = fieldSaveData.nowCycle;
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

        if (field.plant == null)
        {
            PlantinstaceId = 0;
            PlantDataId = 0;
        }
        else
        {
            PlantinstaceId = field.plant.instaceId;
            PlantDataId = field.plant.PlantData.id;
            growthStage = field.plant.growthStage;
            growthDay = field.plant.growthDay;
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
    public List<Formula> formulas = new List<Formula>();

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
        formulas = manufatureSaveData.formulas.ToList();
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
        matchFormula = manufature.matchFormula;
        formulas = manufature.formulas.Values.ToList();
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
}

public class ChapterSave
{
    public int mapId;
    public List<int> findItems;
    public bool open;
    public ChapterSave() { }
    public ChapterSave(ChapterSave chapterSave)
    {
        mapId = chapterSave.mapId;
        open = chapterSave.open;
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
    }
}

public class OtherSaveData
{
    public int gold, diamond;
    public List<int> playerPackages;
    public bool isMarriedFood, isAnMo;
    public OtherSaveData() { }
    public OtherSaveData(OtherSaveData otherSaveData)
    {
        gold = otherSaveData.gold;
        diamond = otherSaveData.diamond;
        playerPackages = new List<int>();
        playerPackages.CopyTo(otherSaveData.playerPackages.ToArray());
        isMarriedFood = otherSaveData.isMarriedFood;
        isAnMo = otherSaveData.isAnMo;
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
    public int dataId;
    public int level;
    public int exp;
    public Gender gender;
    public BrithDay brithDay;
    public int packageId;
    public int2 weapon, clothes, shoe;
    public bool isMarried;

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
    }
    public CharacterSaveData()
    { }

    public CharacterSaveData(Character character)
    {
        SetCharacter(character);
    }

    public void SetCharacter(Character character)
    {
        name = character.name;
        dataId = character.dataId;
        level = character.Level;
        exp = character.exp.nowExp;
        packageId = character.characterPackage;
        weapon = character.Equip.weapon;
        clothes = character.Equip.clothes;
        shoe = character.Equip.shoes;
        gender = character.characterData.gender;
    }
}

public struct PackageSaveData
{
    public int caseCount;
    public int id;
    public int dataId;
    public int level;
    public PackageType packageType;
    public string packageName;
    public bool itemPackage;
    public List<Item> items;
}

public struct BrithDay
{
    public int year;
    public Season season;
    public int day;
}