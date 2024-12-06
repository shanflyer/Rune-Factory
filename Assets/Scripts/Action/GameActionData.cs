using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
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
        Debug.Log($"Action:{name}");
        GameActionDataManager.instance.GameAction(typeName, _parameters, source, target, value, setResult, setValue);
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
}