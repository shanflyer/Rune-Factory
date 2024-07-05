using UnityEngine;

[CreateAssetMenu(menuName = "Data/选择事件数据")]
public class FunctionDataList : ScriptableObject, IGameData, IDataArray<FunctionData>
{
    [SerializeField]
    private FunctionData[] functionDatas;

    public FunctionData[] DataList => functionDatas;
#if UNITY_EDITOR

    public void SetReferenceData()
    {
    }

#endif

    public string GetKey()
    {
        return name;
    }
}

[System.Serializable]
public class FunctionData : IGameData, IReferenceData
{
    public int id;
    public string buttonName;
    public FunctionData[] secondFunctions;
    public GameActionData gameActionData;

    public string GetKey()
    {
        return id.ToString();
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
    }

#endif
}