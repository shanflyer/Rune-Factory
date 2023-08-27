using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyInstance  
{
    HashSet<int> instanceIds = new HashSet<int>();
    public int CreatInstanceId()
    {
        Guid guid = Guid.NewGuid();
        int id=guid.GetHashCode();
        while (instanceIds.Contains(id))
        {
            guid = Guid.NewGuid();
            id = guid.GetHashCode();
        }
        instanceIds.Add(id);
        return id;
    }
    public void Clear()
    {
        instanceIds.Clear();
    }
}