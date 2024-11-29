using System;
using System.Collections;
using System.Collections.Generic; 

public class MyInstance:Singleton<MyInstance>  
{
    HashSet<int> instanceIds = new HashSet<int>();
    Random random = new Random();

    public int  uid => CreateInstanceId();
    private int CreateInstanceId()
    { 
        while (true)
        {
            Guid guid = Guid.NewGuid();
            int index = random.Next(0, 12);
            int id = BitConverter.ToInt32(guid.ToByteArray(), index);
            if(!instanceIds.Contains(id))
            {
                instanceIds.Add(id);
                return id; 
            }
        } 
    }
    public void AddInstance(int id)
    {
        instanceIds.Add(id);
    }
    public void RemoveInstance(int id)
    {
        instanceIds.Remove(id);
    } 
    protected override void Clear()
    {
        instanceIds.Clear();
    }
}