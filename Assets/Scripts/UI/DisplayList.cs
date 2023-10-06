using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayList<T,V> where T:UIObjReference<V> where V:IReferenceData
{
    Transform parent;
    T listPrefab;
    List<T> list;
    public DisplayList(T listPrefab,Transform parent)
    {
        this.parent = parent;
        this.listPrefab = listPrefab;
        list = new List<T>();
    }
    public void InitListData(List<V> componentData,SelectAction<V> SelectAction = null,ToggleGroup toggleGroup=null) 
    {
        for(int i = list.Count; i < list.Count; i++)
        {
            list[i].enabled = false;
            list[i].transform.localScale = Vector3.zero;
        }
        for(int i = 0; i < componentData.Count; i++)
        {
            if (list.Count > i)
            {
                list[i].enabled = true;
                list[i].transform.localScale = Vector3.one;
                list[i].InitData(componentData[i], SelectAction);
            }
            else
            {
                T t = GameObject.Instantiate(listPrefab, parent, toggleGroup);
                t.InitData(componentData[i], SelectAction);
                list.Add(t);
            }
        }
    }
    public void ClearAll()
    {
        for (int i = list.Count; i < list.Count; i++)
        {
            list[i].enabled = false;
            list[i].transform.localScale = Vector3.zero;
        }
    }
}