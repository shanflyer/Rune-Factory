using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameRuntimeObjManager:Singleton<GameRuntimeObjManager>  
{  
    Dictionary<string, Transform> objParents = new Dictionary<string, Transform>();
    Dictionary<string, Dictionary<string, Stack<RuntimeObj>>> unusedRuntimeObjs = 
        new Dictionary<string, Dictionary<string, Stack<RuntimeObj>>>();
    public override void Init()
    {
        base.Init();
    } 

    public void ClearRuntime<T>() where T : Enum
    { 
        foreach (var type in typeof(T).GetEnumValues())
        {
            string runtimeObjType = type.ToString(); 
            unusedRuntimeObjs.Remove(runtimeObjType);
        } 
    }
    public void CreatParent<T>(Transform parent )where T:Enum
    {
        var runtimeObjParent = new GameObject("RuntimeObjParent").transform;
        runtimeObjParent.SetParent(parent);
        foreach (var type in typeof(T).GetEnumValues())
        {
           string runtimeObjType = type.ToString(); 
            GameObject obj = new GameObject(runtimeObjType);
            obj.transform.SetParent(runtimeObjParent);
            objParents.Add(runtimeObjType, obj.transform);
        }
    }
    
    bool GetRuntimeObj(string runtimeObjType,string key,out RuntimeObj runtimeObj)
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

    public RuntimeObj CreatRuntimeObj(string runtimeObjType,string key,GameObject objPre,int linkId)
    {
        RuntimeObj runtimeObj;
        if (!GetRuntimeObj(runtimeObjType,key, out runtimeObj))
        { 
            runtimeObj.obj = GameObject.Instantiate(objPre, objParents[runtimeObjType]);
            runtimeObj.runtimeObjType = runtimeObjType;
            runtimeObj.key = key;
        }
        runtimeObj.linkId = linkId;
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

    public void SetObjParent(string runtimeObjType, bool hide)
    {
        if(objParents.TryGetValue(runtimeObjType,out var parent))
        {
            parent.localPosition = hide ? new Vector3(0, 0, -10000) : Vector3.zero;
        }
    }

}
public struct RuntimeObj
{
    public GameObject obj;
    public int linkId;
    public string runtimeObjType;
    public string key;
    public bool use;
}
