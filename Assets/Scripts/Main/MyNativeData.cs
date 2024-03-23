using System.Collections;
using Unity.Collections;

public interface INativeData
{
    public void Dispose();

    public int Key { get; }
}

public struct MyNativeData<T> where T : unmanaged, INativeData
{
    private NativeList<T> datas;
    private NativeHashMap<int, int> itemIndexes;
    private NativeQueue<int> nullIndexes;
    private int nowIndex;
    private T nullData;

    public IEnumerator GetEnumerator()
    {
        foreach (var itemIndex in itemIndexes)
        {
            yield return datas[itemIndex.Value];
        }
    }

    public void Dispose()
    {
        try
        {
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i].Dispose();
            }

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
        int id = data.Key;
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
        if (itemIndexes.IsEmpty) return false;
        if (itemIndexes.TryGetValue(id, out int index))
        {
            nullIndexes.Enqueue(index);
            itemIndexes.Remove(id);
            return true;
        }
        return false;
    }

    public bool GetData(int id, out T t)
    {
        t = nullData;
        if (itemIndexes.IsEmpty) return false;
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
        int id = t.Key;
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
        if (itemIndexes.IsEmpty)
        {
            return false;
        }
        return itemIndexes.ContainsKey(id);
    }
}