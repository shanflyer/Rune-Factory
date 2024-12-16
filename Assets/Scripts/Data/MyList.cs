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
    public T GetValueForIndex(int index)
    {
        return list[index];
    }
    public K GetKeyForIndex(int index)
    {
        return keys[index];
    }
    public List<T> GetValueList(bool native=false)
    {
        if (native)
        {
            return list;
        }
        List<T> result = new List<T>();
        result.AddRange(list);
        return result;
    }
    public List<K> GetKeyList(bool native = false)
    {
        if (native)
        {
            return keys;
        }
        List<K> result = new List<K>();
        result.AddRange(keys);
        return result;
    }
    public bool ContainsKey(K key)
    {
        return indexDic.ContainsKey(key);
    }
    public void TrySetValue(K key , T t)
    {
        bool ishave = indexDic.TryGetValue(key, out int index);
        if (ishave)
        {
            list[index] = t;
        }
        else
        {
            Add(key, t);
        }
    }
    public bool TryGetValue(K key,out T t)
    {
        bool ishave=indexDic.TryGetValue(key, out int index);
        if (ishave)
        {
            t = list[index];
        }
        else
        {
            t = default(T);
        }
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
            else if (index == list.Count - 1)
            {
                indexDic.Remove(keys[length - 1]);
                list.RemoveAt(length - 1);
                keys.RemoveAt(length - 1);

                length--;
            }
            else
            {
                indexDic.Remove(keys[index]);
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
        if (!indexDic.ContainsKey(key))
        {
            return;
        }
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
            else if (index == list.Count - 1)
            {
                indexDic.Remove(keys[length - 1]);
                list.RemoveAt(length - 1);
                keys.RemoveAt(length - 1); 
                length--;
            }
            else
            {
                indexDic.Remove(keys[index]);
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

public class MySet<T>
{
    private List<T> list; 
    private Dictionary<T, int> indexDic;
    public int length { get; private set; }
    public T this[int index]
    {
        get { return list[index]; }
    }
    public MySet()
    {
        list = new List<T>();
        indexDic = new Dictionary<T, int>(); 
        length = 0;
    }
    public T GetValueForIndex(int index)
    {
        return list[index];
    } 
    public List<T> GetValueList(bool native = false)
    {
        if (native)
        {
            return list;
        }
        List<T> result = new List<T>();
        result.AddRange(list);
        return result;
    }
    
    public bool Contains(T t)
    {
        return indexDic.ContainsKey(t);
    }
    public void TrySetValue(T t)
    {
        bool ishave = indexDic.TryGetValue(t, out int index);
        if (ishave)
        {
            list[index] = t;
        }
        else
        {
            Add( t);
        }
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
        indexDic.Add(item, length);
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
                indexDic.Clear();
            }
            else if (index == list.Count - 1)
            {
                indexDic.Remove(list[index]);
                list.RemoveAt(length - 1);  
                length--;
            }
            else
            {
                indexDic.Remove(list[index]);
                indexDic[list[length - 1]] = index;
                list[index] = list[length - 1];
                list.RemoveAt(length - 1);  
                length--;
            }
        }
        else
        {
            throw new IndexOutOfRangeException();
        }
    }
    public void Remove(T t)
    {
        if (!indexDic.ContainsKey(t))
        {
            return;
        }
        int index = indexDic[t];
        indexDic.Remove(t);

        if (length > index)
        {
            if (length == 1)
            {
                length = 0;
                list.Clear(); 
                indexDic.Clear();
            }
            else if (index == list.Count - 1)
            {
                list.RemoveAt(length - 1);  
                length--;
            }
            else
            {
               
                indexDic[list[length - 1]] = index;
                list[index] = list[length - 1];
                list.RemoveAt(length - 1);  
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
        indexDic.Clear();
    }
}