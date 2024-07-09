using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MyList<T>
{
    private List<T> list;
    public int length { get; private set; }
    public T this[int index]
    {
        get { return list[index]; }
    }
    public MyList(List<T> values)
    {
        list = new List<T>();
        list.AddRange(values);
        length = values.Count;
    }
    public void SetList(List<T> values)
    {
        list.Clear();
        list.AddRange(values);
        length = values.Count;
    }
    public MyList()
    {
        list = new List<T>();
    }
    public void Add(T item)
    {
        if (list.Count > length)
        {
            list[length] = item;
        }
        else
        {
            list.Add(item);
        }
        length++;
    }
    public void RemoveAt(int index)
    {
        if (length > index)
        { 
            if (length == 1)
            {
                length = 0;
                list.Clear();
            }
            else
            {
                var t = list[index];
                list[index] = list[length - 1];
                list.RemoveAt(length-1);
                length--;
            }
        }
        else
        {
           throw new IndexOutOfRangeException();
        }
      
    }
    public void Clear()
    {
        length = 0;
        list.Clear();
    }

}

public class MyDic<K,T>
{
    private List<T> list;
    private List<K> keys;
    private Dictionary<K, int> indexDic;
    public int length { get; private set; }
    public T this[int index]
    {
        get { return list[index]; }
    }
    public MyDic()
    {
        list = new List<T>();
        indexDic = new Dictionary<K, int>();
        keys = new List<K>();
        length = 0;
    }
    
    public List<T> GetValueList()
    {
        return list;
    }
    public List<K> GetKeyList()
    {
        return keys;
    }
    public bool TryGetValue(K key,out T t)
    {
        bool ishave=indexDic.TryGetValue(key, out int index);
        t = list[index];
        return ishave;
    }
    public void Add(K key,T item)
    {
        if (list.Count > length)
        {
            list[length] = item;
            keys[length] = key;
        }
        else
        {
            list.Add(item);
            keys.Add(key);
        }
        indexDic.Add(key, length);
        length++;
    }
    public void RemoveAt(int index)
    {
        if (length > index)
        {
            if (length == 1)
            {
                length = 0;
                list.Clear();
                keys.Clear();
                indexDic.Clear();
            }
            else
            {
                list[index] = list[length - 1];
                keys[index] = keys[length - 1];
                indexDic[keys[index]] = index;
                list.RemoveAt(length - 1);
                keys.RemoveAt(length - 1);

                length--;
            }
        }
        else
        {
            throw new IndexOutOfRangeException();
        }
    }
    public void Remove(K key)
    {
        int index = indexDic[key];
        indexDic.Remove(key);

        if (length > index)
        {
            if (length == 1)
            {
                length = 0;
                list.Clear();
                keys.Clear();
                indexDic.Clear();
            }
            else
            {
                list[index] = list[length - 1];
                keys[index] = keys[length - 1];
                indexDic[keys[index]] = index;
                list.RemoveAt(length - 1);
                keys.RemoveAt(length - 1);

                length--;
            }
        }
        else
        {
            throw new IndexOutOfRangeException();
        }

    } 
    public void Clear()
    {
        length = 0;
        list.Clear();
        keys.Clear();
        indexDic.Clear();
    }
}