using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIObjReference : MonoBehaviour
{
    public Dictionary<string, Transform> objectDatas = new Dictionary<string, Transform>();

    public virtual void OnEnable()
    {
        transform.localScale = Vector3.one;
    }
    public virtual void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }
    public T FindChildGameObject<T>(string childName)
    {
        if (objectDatas.ContainsKey(childName))
        {
            return objectDatas[childName].GetComponent<T>();
        }
        return default(T);
    }
    public Transform FindChildGameObject(string childName)
    {
        if (objectDatas.ContainsKey(childName))
        {
            return objectDatas[childName];
        }
        return null;
    }

    public virtual void InitChildObjData()
    {
        objectDatas.Clear();
        var children = gameObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            try
            {
                objectDatas.Add(children[i].name, children[i]);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{gameObject.name}:{e}");
            }
            
        }
        

    }
    public virtual void SetPanelUISerializeObj()
    {
        InitChildObjData();
    }
}
