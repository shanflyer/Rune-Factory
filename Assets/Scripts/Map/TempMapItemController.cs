using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public class TempMapItemController : Singleton<TempMapItemController>
{
    private MyNativeData<TempMapItem> tempMapItems = new MyNativeData<TempMapItem>();
    private Dictionary<int, int> characterTempMapItems = new Dictionary<int, int>();
    private Dictionary<int, SetCoordinate> characterSetCoordinates = new Dictionary<int, SetCoordinate>();

    public override void Init()
    {
        base.Init();
        tempMapItems.Init(4);
        GameActionManager.instance.AddListener<StopSetTempMapItem>(StopSetTempMapItem);
        GameActionManager.instance.AddListener<TrySetTempMapItem>(TrySetTempMapItem);
        GameActionManager.instance.AddListener<CheckTempMapItemSet>(CheckTempMapItemSet);
        GameActionManager.instance.AddListener<RefreshTempMapItemCoordinate>(RefreshTempMapItemCoordinate);
        GameActionManager.instance.AddListener<DestoryTempMapItem>(DestoryTempMapItem);
        GameActionManager.instance.AddListener<CreatTempMapItem>(CreatTempMapItem);
        GameActionManager.instance.AddListener<CreatControllerTempMapItem>(CreatControllerTempMapItem);
        GameActionManager.instance.AddListener<SetTempMapItemCoordinate>(SetTempMapItemCoordinate);
        // InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, Test);
    }
    void SetTempMapItemCoordinate(SetTempMapItemCoordinate setTempMapItemCoordinate)
    {
        if (tempMapItems.GetData(setTempMapItemCoordinate.instanceId,out var tempMapItem))
        {
            tempMapItem.coordinate = setTempMapItemCoordinate.coordinate;
            tempMapItems.SetData(tempMapItem);
            WorldMapObjManager.instance.RefreshTempMapItem(tempMapItem);
        }
    }
    public void Test(object value)
    {
        if (CharacterManager.instance.controllerCharacter != null)
        {
            if (characterTempMapItems.TryGetValue(CharacterManager.instance.controllerCharacter.instanceId, out var itemId))
            {
                TrySetTempMapItem TrySetTempMapItem = new TrySetTempMapItem
                {
                    instanceId = itemId,
                };
                GameActionManager.instance.QueueAction(TrySetTempMapItem);
            }
        }
    }

    protected override void Clear()
    {
        base.Clear();
    }

    public List<TempMapItem> GetTempMapItems(int roomId)
    {
        List<TempMapItem> list = new List<TempMapItem>();
        foreach (TempMapItem item in tempMapItems)
        {
            if (item.roomId == roomId)
            {
                list.Add(item);
            }
        }
        return list;
    }

    public TempMapItem GetTempMapItem(int instanceId)
    {
        TempMapItem tempMapItem = default(TempMapItem);
        tempMapItems.GetData(instanceId, out tempMapItem);
        return tempMapItem;
    }

    private void StopSetTempMapItem(StopSetTempMapItem StopSetTempMapItem)
    {
        if (tempMapItems.GetData(StopSetTempMapItem.instanceId, out var tempMapItem))
        {
            DestoryTempMapItem DestoryTempMapItem = new DestoryTempMapItem
            {
                instanceId = StopSetTempMapItem.instanceId,
            };
            GameActionManager.instance.QueueAction(DestoryTempMapItem, true);
        }
    }

    private void TrySetTempMapItem(TrySetTempMapItem TrySetTempMapItem)
    {
        if (tempMapItems.GetData(TrySetTempMapItem.instanceId, out var tempMapItem))
        {
            if (tempMapItem.CanSet)
            {
                MoveMapItem moveMapItem = new MoveMapItem
                {
                    mapItemInstanceId = TrySetTempMapItem.instanceId,
                    mapInstance = tempMapItem.roomId,
                    coordinate = tempMapItem.coordinate,
                    dataId = tempMapItem.dataId,
                };
                GameActionManager.instance.QueueAction(moveMapItem, true);

                DestoryTempMapItem DestoryTempMapItem = new DestoryTempMapItem
                {
                    instanceId = TrySetTempMapItem.instanceId,
                };
                GameActionManager.instance.QueueAction(DestoryTempMapItem, true);

                if (TrySetTempMapItem.setResult != null)
                    TrySetTempMapItem.setResult(true);
                return;
            }
        }
        if (TrySetTempMapItem.setResult != null)
            TrySetTempMapItem.setResult(false);
    }

    private void CheckTempMapItemSet(CheckTempMapItemSet CheckTempMapItemSet)
    {
        if (tempMapItems.GetData(CheckTempMapItemSet.instanceId, out var tempMapItem))
        {
            tempMapItem.InitTempSet();
            tempMapItems.SetData(tempMapItem);
            WorldMapObjManager.instance.RefreshTempMapItemColor(tempMapItem);
        }
    }

    private void RefreshTempMapItemCoordinate(RefreshTempMapItemCoordinate RefreshTempMapItemCoordinate)
    {
        int tempId = RefreshTempMapItemCoordinate.instanceId;
        if (tempId == 0)
        {
            characterTempMapItems.TryGetValue(RefreshTempMapItemCoordinate.chatacterId, out tempId);
        }

        if (tempMapItems.GetData(RefreshTempMapItemCoordinate.instanceId, out var tempMapItem))
        {
            tempMapItem.InitCoordinate();
            tempMapItems.SetData(tempMapItem);
            WorldMapObjManager.instance.RefreshTempMapItem(tempMapItem);
        }
    }

    private void DestoryTempMapItem(DestoryTempMapItem destoryTempMapItem)
    {
        if (tempMapItems.GetData(destoryTempMapItem.instanceId, out var tempMapItem))
        {
            tempMapItems.RemoveData(tempMapItem.instanceId);
            if (tempMapItem.characterId != 0)
            {
                characterTempMapItems.Remove(tempMapItem.characterId);

                Character character = CharacterManager.instance.GetCharacter(tempMapItem.characterId);
                if (characterSetCoordinates.TryGetValue(character.instanceId, out var setCoordinate))
                {
                    character.RemoveSetCoordinateDele(setCoordinate);
                    characterSetCoordinates.Remove(character.instanceId);
                }
                character.CanMoveCrossMap = true;
            }
        }
    }

    private async void CreatControllerTempMapItem(CreatControllerTempMapItem creatControllerTempMapItem)
    {
        TempMapItem tempMapItem = new TempMapItem
        {
            instanceId = creatControllerTempMapItem.instanceId,
            dataId = creatControllerTempMapItem.dataId,
            roomId = WorldMapObjManager.instance.displayMap,
            coordinate = creatControllerTempMapItem.coordinate,
            characterId = -1,
            colliderCells = new NativeList<int2>(8, Allocator.TempJob)
        };
        MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(tempMapItem.dataId);
        for (int i = 0; i < mapItemData.colliderCells.Length; i++)
        {
            tempMapItem.colliderCells.Add(mapItemData.colliderCells[i]);
        }
        tempMapItems.SetData(tempMapItem);
        WorldMapObjManager.instance.RefreshTempMapItem(tempMapItem);
        if (creatControllerTempMapItem.setResult != null)
            creatControllerTempMapItem.setResult(true);
    }

    private async void CreatTempMapItem(CreatTempMapItem creatTempMapItem)
    {
        if (characterTempMapItems.ContainsKey(creatTempMapItem.characterId))
        {
            if (creatTempMapItem.setResult != null)
                creatTempMapItem.setResult(false);
            return;
        }
        if (tempMapItems.Contains(creatTempMapItem.instanceId))
        {
            if (creatTempMapItem.setResult != null)
                creatTempMapItem.setResult(false);
            return;
        }

        int mapItemInstanceId;
        int mapItemDataId;
        int2 mapItemCoordiante;

        Character character = CharacterManager.instance.GetCharacter(creatTempMapItem.characterId);
        if (character == null)
        {
            if (creatTempMapItem.setResult != null)
                creatTempMapItem.setResult(false);
            return;
        }

        if (WorldMapManager.instance.GetRuntimeMapItem(creatTempMapItem.instanceId, out var runtimeMapItem))
        {
            mapItemDataId = runtimeMapItem.dataId;
            mapItemInstanceId = runtimeMapItem.instanceId;
            mapItemCoordiante = runtimeMapItem.coordinate;
        }
        else
        {
            mapItemDataId = creatTempMapItem.dataId;
            mapItemInstanceId = creatTempMapItem.instanceId;
            mapItemCoordiante = character.coordinate + GameCommon.GetDirectionInt2(character.direction) * 4;
        }
        TempMapItem tempMapItem = new TempMapItem
        {
            characterId = character.instanceId,
            instanceId = mapItemInstanceId,
            dataId = mapItemDataId,
            roomId = character.mapInstance,
            colliderCells = new NativeList<int2>(8, Allocator.TempJob)
        };
        MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(mapItemDataId);
        for (int i = 0; i < mapItemData.colliderCells.Length; i++)
        {
            tempMapItem.colliderCells.Add(mapItemData.colliderCells[i]);
        }
        tempMapItem.offsetCoordinate = mapItemCoordiante - character.coordinate;
        tempMapItem.coordinate = mapItemCoordiante;
        tempMapItems.SetData(tempMapItem);
        characterTempMapItems[creatTempMapItem.characterId] = tempMapItem.instanceId;
        character.CanMoveCrossMap = false;
        WorldMapObjManager.instance.RefreshTempMapItem(tempMapItem);
        SetCoordinate setCoordinate = async (int3 coordinate) =>
        {
            tempMapItem.InitCoordinate();
            tempMapItems.SetData(tempMapItem);
            WorldMapObjManager.instance.RefreshTempMapItem(tempMapItem);
        };
        character.AddSetCoordinateDele(setCoordinate);
        characterSetCoordinates.Add(character.instanceId, setCoordinate);

        if (creatTempMapItem.setResult != null)
            creatTempMapItem.setResult(true);
    }
}

public struct TempMapItem : INativeData
{
    public int instanceId;
    public int dataId;
    public int characterId;
    public int roomId;
    public int2 coordinate;
    public int2 offsetCoordinate;
    public bool CanSet;

    public NativeList<int2> colliderCells;

    public void InitTempSet()
    {
        CanSet = true;
        for (int i = 0; i < colliderCells.Length; i++)
        {
            int2 cell = colliderCells[i] + coordinate;
            if (!MapCellController.instance.CheckIsWalk(cell, roomId))
            {
                CanSet = false;
                break;
            }
        }
    }

    public void InitCoordinate()
    {
        Character character = CharacterManager.instance.GetCharacter(characterId);
        if (character != null)
        {
            coordinate = character.coordinate + offsetCoordinate;
            InitTempSet();
        }
    }

    public int Key => instanceId;

    public override int GetHashCode()
    {
        return instanceId;
    }

    public void Dispose()
    {
        colliderCells.Dispose();
    }
}