using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public interface IReferenceData
{ 
    public bool Equals(IReferenceData other)
    {
        return this == other;
    }
}

public delegate void SelectUIAction<T>(T t, bool selected = true) where T : BaseReference;

public delegate void SelectAction<T>(T t, int index, bool selected = true);

public class UIObjReference<T> : BaseReference 
{
    public Dictionary<string, Transform> objectDatas = new Dictionary<string, Transform>();
    public Selectable guideSelectable;
    public virtual void ClearData() { }
    public virtual void ClearSelect()
    {
        
    }
    public virtual void Selected() { }
    public virtual void OnEnable()
    {
        transform.localScale = Vector3.one;
    }

    public virtual void OnDisable()
    {
        transform.localScale = Vector3.zero;
        ClearData();
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

    public virtual void SelectDefault()
    { }

    protected void ClickAction()
    {
        if (SelectAction != null)
        {
            SelectAction(data,index);
        }
    }

    public virtual async Task InitData(T t, SelectAction<T> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        if (guideSelectable != null)
            guideSelectable.InitListSelectable(transform.GetSiblingIndex());
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
        Type type = this.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        SetFieldValue(fields);
        var baseFields = type.BaseType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        SetFieldValue(baseFields);
        void SetFieldValue(FieldInfo[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                string keyName = field.Name;
                if (field.Name.Contains("_"))
                {
                    keyName = field.Name.Split('_')[0];
                }
                if (string.IsNullOrEmpty(keyName))
                {
                    var component = gameObject.GetComponent(field.FieldType);
                    field.SetValue(this, component);
                }
                else
                if (objectDatas.TryGetValue(keyName, out var transform))
                {
                    try
                    {
                        var component = transform.GetComponent(field.FieldType);
                        field.SetValue(this, component);
                    }
                    finally { }
                }
            }
        }
    }

    public override void SetPanelUISerializeObj()
    {
        InitChildObjData();
    }
}