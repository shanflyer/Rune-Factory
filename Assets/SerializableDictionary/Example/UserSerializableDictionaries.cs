using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[Serializable]
public class StringLanguageSwitchDataDictionary : SerializableDictionary<string, LanguageSwitchData>
{
}
[Serializable]
public class StringListDictionary : SerializableDictionary<string, List<string>>
{
}

[Serializable]
public class IntTaskScheduleModelDataDictionary : SerializableDictionary<int, TaskScheduleModelData>
{
}

[Serializable]
public class GameTimeKeyIntDataDictionary : SerializableDictionary<GameTimeKey, int>
{
    internal bool TryGetValue(int2 testKey, out int value)
    {
        return TryGetValue((GameTimeKey)testKey, out value);
    }
}

[Serializable]
public class GameTimeKeyInt2DataDictionary : SerializableDictionary<GameTimeKey, int2>
{
    internal bool TryGetValue(int2 testKey, out int2 value)
    {
        return TryGetValue((GameTimeKey)testKey, out value);
    }
}

[Serializable]
public class IntCharacterSaveDataDictionary : SerializableDictionary<int, CharacterSaveData>
{
    public IntCharacterSaveDataDictionary()
    { }

    public void CopyData(IntCharacterSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new CharacterSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntChapterSaveDictionary : SerializableDictionary<int, ChapterSave>
{
    public IntChapterSaveDictionary()
    { }

    public void CopyData(IntChapterSaveDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new ChapterSave(kvp.Value);
        }
    }
}

[Serializable]
public class IntFishSaveDataDataDictionary : SerializableDictionary<int, FishSaveData>
{
    public IntFishSaveDataDataDictionary()
    { }

    public void CopyData(IntFishSaveDataDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new FishSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntAnimalSaveDataDictionary : SerializableDictionary<int, AnimalSaveData>
{
    public IntAnimalSaveDataDictionary()
    { }

    public void CopyData(IntAnimalSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new AnimalSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntPastureSaveDataDictionary : SerializableDictionary<int, PastureSaveData>
{
    public IntPastureSaveDataDictionary()
    { }

    public void CopyData(IntPastureSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new PastureSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntShopSaveDataDictionary : SerializableDictionary<int, ShopSaveData>
{
    public IntShopSaveDataDictionary()
    { }

    public void CopyData(IntShopSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new ShopSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class StringShopListSaveDataDictionary : SerializableDictionary<int, ShopListSaveData>
{
    public StringShopListSaveDataDictionary()
    { }

    public void CopyData(StringShopListSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new ShopListSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntFieldSaveDataDictionary : SerializableDictionary<int, FieldSaveData>
{
    public IntFieldSaveDataDictionary()
    { }

    public void CopyData(IntFieldSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new FieldSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntStoreCounterSaveDataDictionary : SerializableDictionary<int, StoreCounterSaveData>
{
    public IntStoreCounterSaveDataDictionary()
    { }

    public void CopyData(IntStoreCounterSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new StoreCounterSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntManufatureSaveDataDictionary : SerializableDictionary<int, ManufatureSaveData>
{
    public IntManufatureSaveDataDictionary()
    { }

    public void CopyData(IntManufatureSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new ManufatureSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntHomeEquipSaveDataDictionary : SerializableDictionary<int, HomeEquipSaveData>
{
    public IntHomeEquipSaveDataDictionary()
    { }

    public void CopyData(IntHomeEquipSaveDataDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = new HomeEquipSaveData(kvp.Value);
        }
    }
}

[Serializable]
public class IntIntDictionary : SerializableDictionary<int, int>
{
    public IntIntDictionary()
    { }

    public void CopyData(IntIntDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = kvp.Value;
        }
    }
}

[Serializable]
public class IntInt2Dictionary : SerializableDictionary<int, int2>
{
    public IntInt2Dictionary()
    { }

    public void CopyData(IntInt2Dictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = kvp.Value;
        }
    }
}

[Serializable]
public class Int2IntDictionary : SerializableDictionary<int2, int>
{
    public Int2IntDictionary()
    { }

    public void CopyData(Int2IntDictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = kvp.Value;
        }
    }
}

[Serializable]
public class IntInt3Dictionary : SerializableDictionary<int, int3>
{
    public IntInt3Dictionary()
    { }

    public void CopyData(IntInt3Dictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = kvp.Value;
        }
    }
}

[Serializable]
public class IntInt4Dictionary : SerializableDictionary<int, int4>
{
    public IntInt4Dictionary()
    { }

    public void CopyData(IntInt4Dictionary data)
    {
        Clear();
        foreach (var kvp in data)
        {
            this[kvp.Key] = kvp.Value;
        }
    }
}

[Serializable]
public class IntWorldMapDictionary : SerializableDictionary<int, WorldMap>
{ }

public class StringFightChapterListDictionary : SerializableDictionary<string, List<FightChapterReference>>
{ }

[Serializable]
public class StringTimelineAssetDataDictionary : SerializableDictionary<string, TimelineAssetData>
{ }

[Serializable]
public class SeasonRandomDictionary : SerializableDictionary<Season, int>
{ }

[Serializable]
public class ItemAnimationDictionary : SerializableDictionary<int2, AnimationStateData>
{ }

[Serializable]
public class IntBehaviorDictionarys : SerializableDictionary<int, IntBehaviorDictionary>
{ }

[Serializable]
public class IntBehaviorDictionary : SerializableDictionary<int, ExternalBehaviorTree>
{ }

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string>
{ }

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color>
{ }

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]>
{ }

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage>
{ }

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass>
{ }