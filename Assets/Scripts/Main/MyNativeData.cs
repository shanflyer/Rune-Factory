using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

public struct MyNativeData<T>where T : unmanaged
{
    private NativeList<T> datas;
    private NativeHashMap<int, int> itemIndexes;
    private NativeQueue<int> nullIndexes;
    private int nowIndex;
    private T nullData;

    public IEnumerator GetEnumerator()
    {
        foreach(var itemIndex in itemIndexes)
        {
            yield return datas[itemIndex.Value];
        }
    }

    public void Dispose()
    {
        try
        {  
            datas.Dispose();
            itemIndexes.Dispose();
            nullIndexes.Dispose();
        }
        catch { }
       
    } 
    public void Init(int count)
    {
        datas = new NativeList<T>(count, Allocator.TempJob);
        itemIndexes = new NativeHashMap<int, int>(count, Allocator.TempJob);
        nullIndexes = new NativeQueue<int>(Allocator.TempJob);
    }

    public void AddData(T data)
    {
        int id = data.GetHashCode();
        int index = nowIndex;
        if (nullIndexes.Count > 0)
        {
            index = nullIndexes.Dequeue();
            datas[index] = data;
        }
        else
        {
            datas.Add(data);
            nowIndex++;
        }
        itemIndexes.Add(id, index);
    }
    public bool RemoveData(int id)
    {
        if (itemIndexes.TryGetValue(id, out int index))
        {
            nullIndexes.Enqueue(index);
            return true;
        }
        return false;
    }
    public bool GetData(int id, out T t)
    {
        if (itemIndexes.TryGetValue(id, out int index))
        {
            t = datas[index];
            return true;
        }
        t = nullData;
        return false;
    }
    public void SetData(T t)
    {
        int id = t.GetHashCode();
        if (itemIndexes.TryGetValue(id, out int index))
        {
            datas[index] = t;
        }
        else
        {
            AddData(t);
        }
    }

    public bool Contains(int id)
    {
        return itemIndexes.ContainsKey(id);
    }

   
}