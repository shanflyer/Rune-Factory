using System.Collections;
using UnityEngine;
public class MapController :Singleton<MapController>
{
    public int targetMap;
    public int nowMap;
    public bool MapRunning => mapRunning;
    private bool mapRunning;
    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }

}