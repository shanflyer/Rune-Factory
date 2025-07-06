using System;
using System.Collections;
using System.Collections.Generic;

public class MyInstance : Singleton<MyInstance>
{
    HashSet<int> instanceIds = new HashSet<int>();
    Random random = new Random();

   
    public override void Init()
    {
        base.Init();
        tempUid = 100_000;
        uid = 100_000;

    }
    public int TempUid
    {
        get
        {
            tempUid++;
            return tempUid;
        }
    }
    private int tempUid;

    public int Uid
    {
        get
        {
            uid++;
            return uid;
        }
    }
    private int uid;
    public int MaxUid => uid;
    public int CreateInstanceId()
    {
        while (true)
        {
            Guid guid = Guid.NewGuid();
            int index = random.Next(0, 12);
            int id = BitConverter.ToInt32(guid.ToByteArray(), index);
            if (!instanceIds.Contains(id))
            {
                instanceIds.Add(id);
                return id;
            }
        }
    }

 
    protected override void Clear()
    {
        instanceIds.Clear();
    }
}