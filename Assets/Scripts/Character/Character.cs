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
    public int energy;
    public int health;
    public int satiety;

    public int GetValue(CharacterPropertyType characterPropertyType)
    {
        switch (characterPropertyType)
        {
            case CharacterPropertyType.体力:
                return  energy; 
            case CharacterPropertyType.生命:
                return health;
            case CharacterPropertyType.饱食:
                return satiety;
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
    public void SetProperty(int energy, int health, int satiety)
    {
        characterProperty.energy = energy;
        characterProperty.health = health;
        characterProperty.satiety = satiety;

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
                characterProperty.energy = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.生命:
                characterProperty.health = setCharacterProperty.setValue;
                break;
            case CharacterPropertyType.饱食:
                characterProperty.satiety = setCharacterProperty.setValue;
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
                characterProperty.energy += changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.生命:
                characterProperty.health += changeCharacterProperty.changeValue;
                break;
            case CharacterPropertyType.饱食:
                characterProperty.satiety += changeCharacterProperty.changeValue;
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
        characterProperty = new CharacterProperty
        {
            energy = 100,
            health = 100,
            satiety = 100
        };
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