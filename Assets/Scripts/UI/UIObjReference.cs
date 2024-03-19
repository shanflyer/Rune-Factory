using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IReferenceData
{
    public bool Equals(IReferenceData other)
    {
        return true;
    } 
}
public delegate void SelectAction<T>(T t,bool selected=true) where T : IReferenceData;
public class UIObjReference<T> : BaseReference where T : IReferenceData
{
    public Dictionary<string, Transform> objectDatas = new Dictionary<string, Transform>();
     
    public virtual void ClearSelect() { }
    public virtual void OnEnable()
    {
        transform.localScale = Vector3.one;
    }
    public virtual void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }
    public V FindChildGameObject<V>(string childName)
    {
        if (objectDatas.ContainsKey(childName))
        {
            return objectDatas[childName].GetComponent<V>();
        }
        return default(V);
    }
    public Transform FindChildGameObject(string childName)
    {
        if (objectDatas.ContainsKey(childName))
        {
            return objectDatas[childName];
        }
        return null;
    }
    public T t => data;
    protected T data;
    protected SelectAction<T> SelectAction;

    public virtual void SelectDefault() { }
    protected void ClickAction()
    {
        if (SelectAction != null)
        {
            SelectAction(data);
        }
    }
    public virtual void InitData(T t, SelectAction<T> SelectAction = null,ToggleGroup toggleGroup=null)
    {
        data = t;
        if (SelectAction != null)
        {
            this.SelectAction = SelectAction;
        }
       
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
    public override void SetPanelUISerializeObj()
    {
        InitChildObjData();
    }
}
