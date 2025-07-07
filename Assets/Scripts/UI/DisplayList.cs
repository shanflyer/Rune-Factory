using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayList<T, V> where T : UIObjReference<V>
{
    private Transform parent;
    private T listPrefab;
    private List<T> list;

    public int dataCount => _dataCount;
    private int _dataCount;

    public DisplayList(T listPrefab, Transform parent)
    {
        this.parent = parent;
        this.listPrefab = listPrefab;
        list = new List<T>();
    }

    public void SelectIndex(int index)
    {
        if (index < dataCount)
        {
            list[index].Selected();
        }
    }

    public virtual void ClearSelect()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].ClearSelect();
        }
    }

    public virtual void ClearSelect(V v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].t.Equals(v))
            {
                list[i].ClearSelect();
            }
        }
    }

    public void Select(V v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].t.Equals(v))
            {
                list[i].SelectDefault();
            }
        }
    }

    public void SelectDefault()
    {
        if (list.Count > 0)
        {
            list[0].SelectDefault();
        }
    }

    public T GetReference(V v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].t.Equals(v))
            {
                return list[i];
            }
        }
        return null;
    }

    public T GetReference(int index)
    {
        if (index < dataCount)
        {
            return list[index];
        }
        return null;
    }

    public void SetSelectData(V v, SelectAction<V> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].t.Equals(v))
            {
                list[i].InitData(v, SelectAction, toggleGroup);
            }
        }
    }

    public void InitListData(List<V> componentData, SelectAction<V> SelectAction = null, ToggleGroup toggleGroup = null, bool Async = true)
    {
        if (componentData == null || componentData.Count == 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearData();
                list[i].enabled = false;
                list[i].transform.localScale = Vector3.zero;
            }
            _dataCount = 0;
            return;
        }
        _dataCount = componentData.Count;
        for (int i = list.Count - 1; i > componentData.Count - 1; i--)
        {
            list[i].ClearData();
            list[i].enabled = false;
            list[i].transform.localScale = Vector3.zero;
        }

        for (int i = 0; i < componentData.Count; i++)
        {
            if (list.Count > i)
            {
                list[i].enabled = true;
                list[i].transform.localScale = Vector3.one;
                list[i].InitData(componentData[i], SelectAction, toggleGroup);
            }
            else
            {
                T t;
                /*
                if (Async)
                {
                    var async = GameObject.InstantiateAsync(listPrefab);
                    await async;
                    t = async.Result[0];
                }
                else*/
                {
                    t = GameObject.Instantiate(listPrefab);
                }
                try
                {
                    t.enabled = true;
                    t.transform.SetParent(parent);
                    t.transform.localScale = Vector3.one;
                    t.InitData(componentData[i], SelectAction, toggleGroup);
                    list.Add(t);
                }
                catch (Exception e)
                {
                    Debug.LogError($"i:{i}-count:{componentData.Count}--{e}");
                }
            }
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].ClearData();
            list[i].enabled = false;
            list[i].transform.localScale = Vector3.zero;
        }
        _dataCount = 0;
    }
}