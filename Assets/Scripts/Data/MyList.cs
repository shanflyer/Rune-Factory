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

}