using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/生长模型")]
public class GrowModelDataList: ScriptableObject, IDataArray<GrowModelData>
{
    [SerializeField]
    List<GrowModelData> growModelDatas;

    public List<GrowModelData> DataList => growModelDatas;
}
[System.Serializable]
public struct GrowModelData : IGameData
{
    public int id;
    public string name;
    public AnimationCurve curve;
    public string GetKey()
    {
        return id.ToString();
    }
}