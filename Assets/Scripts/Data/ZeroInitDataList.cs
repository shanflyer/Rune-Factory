using System;
using System.Collections.Generic; 
using UnityEngine;

[Serializable]
public struct InitDataPath
{
    public string type;
    public List<string> pathes;
}
[CreateAssetMenu(menuName = "Data/ZeroInitDataList")]
public class ZeroInitDataList : ScriptableObject, IGameData
{
    public List<string> types = new List<string>();

    public List<InitDataPath> datas = new List<InitDataPath>(); 
    public string GetKey()
    {
        return "ZeroInitDataList";
    }

    public void SetReferenceData()
    {
        datas.Clear();
        for(int i = 0; i < types.Count; i++)
        {
            Type type = Type.GetType(types[i]);
            if (type == null)
            {
                Debug.LogError("Type not found: " + types[i]);
                continue;
            }
            string parentPath = DataPath.GetDataPath(type);
            if (!string.IsNullOrEmpty(parentPath))
            {
                var all = Resources.LoadAll(parentPath);
                foreach (var data in all)
                {
                    if (datas.Count<=i)
                    {
                        datas.Add(new InitDataPath
                        {
                            type = types[i],
                            pathes=new List<string>(),
                        });
                    }
                    datas[i].pathes.Add($"{parentPath}/{data.name}");
                }
            }
        }
    }
} 