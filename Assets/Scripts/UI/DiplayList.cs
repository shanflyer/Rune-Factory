using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class DiplayList<T,V> where T:UIObjReference
{
    Transform parent;
    T listPrefab;
    public DiplayList(T listPrefab,Transform parent)
    {
        this.parent = parent;
        this.listPrefab = listPrefab;
    }
    public void InitListData(List<V> componentData) 
    {

    }
}