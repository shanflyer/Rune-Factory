using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public struct ObjCoordinate
{
    public int mapInstance;
    public int x,y;
     
    public int2 coordinate
    {
        get
        {
            return new int2(x, y);
        } 
    }
    public void SetObjCoordinate(int mapInstance, int2 coordinate)
    {
        this.mapInstance = mapInstance;
        x = coordinate.x;
        y = coordinate.y;
    }
    public static bool operator ==(ObjCoordinate obj0, ObjCoordinate obj1)
    {
        return obj0.mapInstance == obj1.mapInstance &&
            obj0.x == obj1.x && obj0.y == obj1.y;
    }
    public override bool Equals(object obj)
    {
        try
        {
            return (ObjCoordinate)(obj) == this;
        }
        catch
        { 
        }
        return false;
    }
    public override int GetHashCode()
    {
        return mapInstance*100+ coordinate.x+ coordinate.y;
    }
    public static bool operator !=(ObjCoordinate obj0, ObjCoordinate obj1)
    {
        return obj0.mapInstance != obj1.mapInstance || obj0.x != obj1.x
            || obj0.y != obj1.y;
    }
}
public struct CharacterProperty
{
    public int HP, MP, Power, MaxHP,MaxMP,MaxPower, AT, DF, Crit, Dodge;
    public int Other;
    public int GetValue(CharacterPropertyType characterPropertyType)
    {
        switch (characterPropertyType)
        {
            case CharacterPropertyType.体力:
                return Power; 
            case CharacterPropertyType.生命:
                return HP;
            case CharacterPropertyType.法力:
                return MP;
            case CharacterPropertyType.攻击:
                return AT;
            case CharacterPropertyType.防御:
                return DF;
            case CharacterPropertyType.暴击:
                return Crit;
            case CharacterPropertyType.闪避:
                return Dodge;
            case CharacterPropertyType.最大体力:
                return MaxPower;
            case CharacterPropertyType.最大生命:
                return MaxHP;
            case CharacterPropertyType.最大法力:
                return MaxMP;
            default:
                return Other;
        }
        return -1;
    }
}
public class Character
{
    public CharacterProperty characterProperty;

    public string name;
    public ObjCoordinate objCoordinate;
    public int instanceId;
    public Direction direction;

    public IEnumerator moveEnumerator;

    public void StopMove()
    {
        GameController.instance.StopCoroutine(moveEnumerator);
    }
    public void SetProperty(int HP=-1, int MP = -1, int Power = -1, int MaxHP = -1, int MaxMP = -1, int MaxPower = -1, int AT = -1, int DF = -1, int Crit = -1, int Dodge = -1
        ,int Other=-1)
    {
        if(HP>=0)
            characterProperty.HP = HP;
        if (MP >= 0)
            characterProperty.MP = MP;
        if (Power >= 0)
            characterProperty.Power = Power;
        if (MaxHP >= 0)
            characterProperty.MaxHP = MaxHP;
        if (MaxMP >= 0)
            characterProperty.MaxMP = MaxMP;
        if (MaxPower >= 0)
            characterProperty.MaxPower = MaxPower;
        if (AT >= 0)
            characterProperty.AT = AT;
        if (DF >= 0)
            characterProperty.DF = DF;
        if (Crit >= 0)
            characterProperty.Crit = Crit;
        if (Dodge >= 0)
            characterProperty.Dodge = Dodge;
        if (Other >= 0)
            characterProperty.Other = Other;

        CharacterPropertyTrigger characterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = characterProperty
        };
        GameActionManager.instance.QueueAction(characterPropertyTrigger);
    }
    public void SetProperty(SetCharacterProperty setCharacterProperty)
    {
        switch (setCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                characterProperty.Power = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.生命:
                characterProperty.HP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.法力:
                characterProperty.MP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.最大体力:
                characterProperty.MaxPower = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.最大法力:
                characterProperty.MaxMP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.最大生命:
                characterProperty.MaxHP = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.攻击:
                characterProperty.AT = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.防御:
                characterProperty.DF = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.闪避:
                characterProperty.Crit = setCharacterProperty.setValue;
                break; 
            case CharacterPropertyType.暴击:
                characterProperty.Dodge = setCharacterProperty.setValue;
                break; 
            case CharacterPropertyType.自定义值:
                characterProperty.Other = setCharacterProperty.setValue;
                break; 
        }
        CharacterPropertyTrigger characterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = characterProperty
        };
        GameActionManager.instance.QueueAction(characterPropertyTrigger);
    }
    public void AddProperty(ChangeCharacterProperty changeCharacterProperty)
    {
        switch (changeCharacterProperty.propertyType)
        {
            case CharacterPropertyType.体力:
                characterProperty.Power = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.生命:
                characterProperty.HP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.法力:
                characterProperty.MP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.最大体力:
                characterProperty.MaxPower = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.最大法力:
                characterProperty.MaxMP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.最大生命:
                characterProperty.MaxHP = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.攻击:
                characterProperty.AT = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.防御:
                characterProperty.DF = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.闪避:
                characterProperty.Crit = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.暴击:
                characterProperty.Dodge = changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.自定义值:
                characterProperty.Other = changeCharacterProperty.changeValue;
                break;
        }
    }

    private void TriggerEventAction(int eventid, int reference, bool enter)
    {
        if (enter)
        {
            TriggerEnter triggerEnter = new TriggerEnter
            {
                eventId = reference
            };
            GameActionManager.instance.QueueAction(triggerEnter);
        }
        else
        {
            TriggerExit triggerExit = new TriggerExit
            {
                eventId = reference
            };
            GameActionManager.instance.QueueAction(triggerExit);
        }

        List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>(2);
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.characterTriggerRenferenceName,
            value = instanceId
        });
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.triggerRenferenceName,
            value = reference
        });

        GameEventManager.instance.AddGameEvent(eventid, eventReferenceDatas);
    }

    public void SetObjCoordinate(int mapInstance, int2 coordinate)
    {
        MapCellController.instance.CheckTriggerEvent(instanceId, EntityType.角色, mapInstance, objCoordinate.coordinate, coordinate,
           TriggerEventAction);

        objCoordinate.SetObjCoordinate(mapInstance, coordinate);
        CharacterCoordinateTrigger characterCoordinateTrigger = new CharacterCoordinateTrigger
        {
            characterId = instanceId,
            mapId = mapInstance,
            coordinate = coordinate
        };
        GameActionManager.instance.QueueAction(characterCoordinateTrigger);
    }
    public void MoveCrossMap(int targetMap, int2 targetCoordinate)
    {
        Queue<int> moveRoomList = new Queue<int>();
        bool result = false;
        Queue<int> resultList = MapCellController.instance.FindRoomList(objCoordinate.mapInstance, targetMap, moveRoomList, ref result);
        if (result)
        {
            MoveCrossMap(resultList, targetCoordinate);
        }
    }


    void MoveCrossMap(Queue<int> moveRoomList, int2 targetCoordinate)
    {
        int nowMap = objCoordinate.mapInstance;
        if (moveRoomList.Count > 0)
        {
            int target = moveRoomList.Dequeue();

            int2 inCoordinate = int2.zero;
            if (MapCellController.instance.GetLinkMapInCoordinate(nowMap, target, ref inCoordinate))
            {
                Stack<int2> pathNodes =MapCellController.instance.FindPathNode(objCoordinate.coordinate, inCoordinate,nowMap);
                PlayerMove(pathNodes, () => {
                    MoveCrossMap(moveRoomList, targetCoordinate);
                });
            }
        }
        else
        {
            Stack<int2> pathNodes = MapCellController.instance.FindPathNode(objCoordinate.coordinate, targetCoordinate, nowMap); 
            PlayerMove(pathNodes, null);
        }


    }

    public void PlayerMove(Stack<int2> pathNodes, MoveEndAction endAction = null)
    {
        if (pathNodes.Count > 0)
        {
            CharacterManager.instance.CharacterMoveTarget(this, pathNodes, endAction);
        }
    }

   
}



public class NPC : Character
{

}
public class Player : Character
{
    
    public int bag;

    public Player(string name)
    {
        this.name = name;
        bag = PackageManager.instance.CreatGamePackage(10, "PlayerBag",instanceId);
        //临时
    } 
    public Player(CharacterSaveData characterSaveData)
    {
        this.name = characterSaveData.name;
        //SetProperty(characterSaveData.energy, characterSaveData.health, characterSaveData.satiety); 
        //bag = PackageManager.instance.CreatGamePackage(characterSaveData.packageCount, characterSaveData.packageName, instanceId);
        //foreach(var item in characterSaveData.items)
        //{
        //    PackageManager.instance.SetItemInPackage(item, bag); 
        //}
    }


}