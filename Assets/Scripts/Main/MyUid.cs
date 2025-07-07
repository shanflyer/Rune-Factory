public class MyUid
{
    public int Uid
    {
        get
        {
            uid++;
            return uid;
        }
    }

    private int uid;

    public MyUid()
    {
        uid = 1_00_000;
    }

    public void Clear()
    {
        uid = 1_00_000;
    }
}