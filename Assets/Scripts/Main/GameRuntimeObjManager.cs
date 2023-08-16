using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameRuntimeObjManager:Singleton<GameRuntimeObjManager>  
{  
    Dictionary<RuntimeObjType, Transform> objParents = new Dictionary<RuntimeObjType, Transform>();
    Dictionary<RuntimeObjType, Dictionary<string, Stack<RuntimeObj>>> unusedRuntimeObjs = new Dictionary<RuntimeObjType, Dictionary<string, Stack<RuntimeObj>>>();
    public override void Init()
    {
        base.Init();
        var runtimeObjParent = new GameObject("RuntimeObjParent").transform;
        runtimeObjParent.SetParent(GameController.instance.transform);
        foreach (var type in typeof(RuntimeObjType).GetEnumValues())
        {
            RuntimeObjType runtimeObjType = (RuntimeObjType)type;
            GameObject obj = new GameObject(runtimeObjType.ToString());
            obj.transform.SetParent(runtimeObjParent);
            objParents.Add(runtimeObjType, obj.transform);
        }
    } 
    bool GetRuntimeObj(RuntimeObjType runtimeObjType,string key,out RuntimeObj runtimeObj)
    {
        if(unusedRuntimeObjs.TryGetValue(runtimeObjType,out Dictionary<string, Stack<RuntimeObj>> selectRuntimeObjs))
        {
            Stack<RuntimeObj> runtimeObjs;
            if (selectRuntimeObjs.TryGetValue(key,out runtimeObjs))
            {
               // unusedRuntimeObjs[runtimeObjType].Remove(key);
                if (runtimeObjs.Count > 0)
                {
                    runtimeObj= runtimeObjs.Pop();
                    return true;
                }

            } 
        }
        runtimeObj = new RuntimeObj();
        return false;
    }
    public async Task<RuntimeObj> CreatMapGroundRuntimeObj(string map,int instanceId)
    {
        RuntimeObj runtimeObj;
        if (!GetRuntimeObj(RuntimeObjType.MAPGROUND, map, out runtimeObj))
        { 
            //加载房间数据
            MapRoomData mapRoomData=await GameDataManager.instance.GetAsyncObjectData<MapRoomData>(map);
            runtimeObj.obj = GameObject.Instantiate(mapRoomData.mapObj, objParents[RuntimeObjType.MAPGROUND]);
            runtimeObj.runtimeObjType = RuntimeObjType.MAPGROUND; 
            runtimeObj.key = map;

        }
        runtimeObj.linkId = instanceId;
        runtimeObj.obj.SetActive(true);
        runtimeObj.use = true;
        return runtimeObj;
    }
    public async Task<RuntimeObj> CreatMapItemRuntimeObj(MapItem item)
    {
        Vector3 pos = GameCommon.GetMapPos(item.coordinate);
        pos.z = -100;

        RuntimeObj runtimeObj;
        if (!GetRuntimeObj(RuntimeObjType.MAPITEM, item.id.ToString(), out runtimeObj))
        {
            MapItemData mapItemData =await GameDataManager.instance.GetAsyncObjectData<MapItemData>(item.id);
            if (mapItemData.itemObj != null)
            { 
                runtimeObj.obj = GameObject.Instantiate(mapItemData.itemObj,objParents[RuntimeObjType.MAPITEM]);
                runtimeObj.runtimeObjType = RuntimeObjType.MAPITEM; 
            } 
        }
        runtimeObj.obj.transform.position = pos;
        runtimeObj.key = item.id.ToString();
        runtimeObj.linkId = item.instanceId;
        runtimeObj.obj.SetActive(true);
        runtimeObj.use = true;
        return runtimeObj;
    }

    public async Task<RuntimeObj> CreatMapItemRuntimeObj(RuntimeMapItem item)
    {
        Vector3 pos = GameCommon.GetMapPos(item.coordinate);
        pos.z = -100;

        RuntimeObj runtimeObj;
        if (!GetRuntimeObj(RuntimeObjType.MAPITEM, item.dataId.ToString(), out runtimeObj))
        {
            MapItemData mapItemData = await GameDataManager.instance.GetAsyncObjectData<MapItemData>(item.dataId);
            if (mapItemData.itemObj != null)
            {
                runtimeObj.obj = GameObject.Instantiate(mapItemData.itemObj, objParents[RuntimeObjType.MAPITEM]);
                runtimeObj.runtimeObjType = RuntimeObjType.MAPITEM;
            }
        }
        runtimeObj.obj.transform.position = pos;
        runtimeObj.key = item.dataId.ToString();
        runtimeObj.linkId = item.instanceId;
        runtimeObj.obj.SetActive(true);
        runtimeObj.use = true;
        return runtimeObj;
    }
    public async Task<RuntimeObj> CreatCharacterRuntimeObj(Character character)
    {
        Vector3 pos = GameCommon.GetMapPos(character.objCoordinate.coordinate);
        pos.z = -100;

        RuntimeObj runtimeObj;
        if (!GetRuntimeObj(RuntimeObjType.CHARACTER, "0", out runtimeObj))
        {  
           var playerObj=await ExtensionsResources.LoadResourceAsync<GameObject>(DataPath.characterPrefabPath);
            runtimeObj.obj= GameObject.Instantiate(playerObj,objParents[RuntimeObjType.CHARACTER]);
            runtimeObj.key = "0";
            runtimeObj.runtimeObjType = RuntimeObjType.CHARACTER;
            //test 
        }
        runtimeObj.obj.transform.position = pos;
        Sprite characterIcon = await CharacterManager.instance.GetCharacterIcon(character.name);
        SpriteRenderer modelRenderer = runtimeObj.obj.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        if (modelRenderer)
        {
            modelRenderer.sprite = characterIcon;
        }

       // runtimeObj.obj.GetComponentInChildren<TextMesh>(true).text = character.name;
        runtimeObj.linkId = character.instanceId;
        runtimeObj.obj.SetActive(true);
        runtimeObj.use = true;
        return runtimeObj;
    }

    public void RecycleRuntimeObj(RuntimeObj runtimeObj)
    {
        runtimeObj.use = false;
        runtimeObj.obj.SetActive(false);
        Dictionary<string, Stack<RuntimeObj>> objs;
        if(!unusedRuntimeObjs.TryGetValue(runtimeObj.runtimeObjType,out objs))
        {
            objs = new Dictionary<string, Stack<RuntimeObj>>();
            unusedRuntimeObjs.Add(runtimeObj.runtimeObjType, objs);
        }
        Stack<RuntimeObj> runtimeObjs;
        if(!objs.TryGetValue(runtimeObj.key,out runtimeObjs))
        {
            runtimeObjs = new Stack<RuntimeObj>();
            objs[runtimeObj.key] = runtimeObjs;
        }

        runtimeObjs.Push(runtimeObj);
    }

}
public struct RuntimeObj
{
    public GameObject obj;
    public int linkId;
    public RuntimeObjType runtimeObjType;
    public string key;
    public bool use;
}
