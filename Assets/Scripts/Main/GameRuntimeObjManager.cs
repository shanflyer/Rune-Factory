using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

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

            objParents.Remove(runtimeObjType);
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
        runtimeObj = null;
        return false;
    }
 
    public async Task<RuntimeObj> CreatRuntimeObj<T>(string runtimeObjType,string key,T objPre,int linkId,
        Transform overrideParent=null,bool isActive=true)where T:Component
    {
        if(!objParents.TryGetValue(runtimeObjType,out Transform parent))
        {
            parent = new GameObject(runtimeObjType).transform;
            objParents[runtimeObjType] = parent;
        }
        if (overrideParent != null)
        {
            parent = overrideParent;
        }
        if (!GetRuntimeObj(runtimeObjType, key, out var runtimeObj))
        {
            runtimeObj = new RuntimeObj();
            var asyncInstantiateOperation = GameObject.InstantiateAsync(objPre, parent);
            await asyncInstantiateOperation;
            runtimeObj.obj = asyncInstantiateOperation.Result[0];
            runtimeObj.runtimeObjType = runtimeObjType;
            runtimeObj.key = key;
        }
        runtimeObj.linkId = linkId;
        try
        { 
            var obj = runtimeObj.obj as Component; 
            obj.transform.SetParent(parent, false);
            obj.gameObject.SetActive(isActive);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
         
        
        runtimeObj.use = true;
        return runtimeObj;
    }
    public void RecycleRuntimeObj(RuntimeObj runtimeObj)
    {
        if(runtimeObj.obj != null)
        {  
            runtimeObj.use = false;
            var component = runtimeObj.obj as Component;

            if (objParents.TryGetValue(runtimeObj.runtimeObjType, out Transform parent))
            {
                component.transform.SetParent(parent, false);
            }
            component.gameObject.SetActive(false);
            Dictionary<string, Stack<RuntimeObj>> objs;
            if (!unusedRuntimeObjs.TryGetValue(runtimeObj.runtimeObjType, out objs))
            {
                objs = new Dictionary<string, Stack<RuntimeObj>>();
                unusedRuntimeObjs.Add(runtimeObj.runtimeObjType, objs);
            }
            Stack<RuntimeObj> runtimeObjs;
            if (!objs.TryGetValue(runtimeObj.key, out runtimeObjs))
            {
                runtimeObjs = new Stack<RuntimeObj>();
                objs[runtimeObj.key] = runtimeObjs;
            }
            if(runtimeObjs.Count <10)
            {
                runtimeObjs.Push(runtimeObj);
            }
            else
            {
                GameObject.Destroy(component.gameObject); 
            }
           
        }
        //runtimeObj = null;
    }

    public void SetObjParent(string runtimeObjType, bool hide)
    {
        if(objParents.TryGetValue(runtimeObjType,out var parent))
        {
            parent.localPosition = hide ? new Vector3(0, 0, -10000) : Vector3.zero;
        }
    }

}
public class RuntimeObj
{
    public Object obj;
    public int linkId;
    public string runtimeObjType;
    public string key;
    public bool use;
}
