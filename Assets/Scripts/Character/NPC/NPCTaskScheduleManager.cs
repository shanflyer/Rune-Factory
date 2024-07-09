using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPCTaskScheduleManager:Singleton<NPCTaskScheduleManager>
{
    Dictionary<int, NPCTaskScheduleData> NPCTaskScheduleDatas = new Dictionary<int, NPCTaskScheduleData>();
    public override async void Init()
    {
        base.Init();
        var datas=await GameDataManager.instance.GetAllAsyncData<NPCTaskScheduleData>();
        for(int i = 0; i < datas.Count; i++)
        {
            NPCTaskScheduleDatas[datas[i].id] = datas[i];
        }
    }
    public bool GetTaskScheduleData(int id,out NPCTaskScheduleData data)
    {
        return NPCTaskScheduleDatas.TryGetValue(id, out data);
    }
}