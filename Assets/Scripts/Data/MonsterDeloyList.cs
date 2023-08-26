using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/怪物分布")]
public class MonsterDeloyList : ScriptableObject, IGameData, IDataArray<MonsterDeploy>
{
    [SerializeField]
    List<MonsterDeploy> monsterDeploys = new List<MonsterDeploy>();

    List<MonsterDeploy> IDataArray<MonsterDeploy>.DataList => monsterDeploys;

    string IGameData.GetKey()
    {
        return name;
    }

    void IGameData.SetReferenceData()
    { 
    }
}
[System.Serializable]
public struct MonsterDeploy:IGameData
{
    public int id;
    public int refreshId;
    public int beforeActionId;
    public int afterActionId;
    public int victoryId;
    public int failedId;
    public string text;

    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}
