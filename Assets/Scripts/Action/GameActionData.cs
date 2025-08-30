using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GameActionData")]
[System.Serializable]
public class GameActionData : ScriptableObject, IGameData
{
    public int id;
    public string typeName;
    public List<Parameter> _parameters;

    public GameActionData(GameActionData gameActionData)
    {
        id = gameActionData.id;
        typeName = gameActionData.typeName;
        _parameters = new List<Parameter>();
        _parameters.AddRange(gameActionData._parameters);
    }

    public void Action(int source = 0, int target = 0, int value = 0, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (GameDataManager.instance.GlobalData.debug)
        {
            if (name != "440展示随机表情" && name != "ShowEmote" && name != "ShowRandomEmote")
            {
                if (name == "34新手进入商店")
                {
                    Debug.Log($"Match!!!");
                }
                string parameterStr = "";
                if (_parameters != null)
                {
                    for (int i = 0; i < _parameters.Count; i++)
                    {
                        parameterStr = $"{parameterStr}--{_parameters[i]}";
                    }
                }
               
               Debug.Log($"Action:{name}--parameters:{parameterStr}");
            }
        }

        GameActionDataManager.instance.GameAction(typeName, _parameters, source, target, value, setResult, setValue, immediately);
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
    }

#endif

    public string GetKey()
    {
        return id.ToString();
    }
}

[System.Serializable]
public class Parameter
{
    public string value;
    public List<Parameter> parameters;

    public override string ToString()
    {
        string outStr = value;
        outStr = $"{outStr}-parameters：";
        if(parameters != null)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                outStr = $"{outStr}；{parameters[i]}";
            }
        }
       
        return outStr;
    }
}