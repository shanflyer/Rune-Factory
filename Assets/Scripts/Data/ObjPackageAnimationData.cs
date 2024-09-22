using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/地图物体背包显示数据")]
public class ObjPackageAnimationData : ScriptableObject, IGameData
{
    public int id;
    public int packageId;
    public int mapItemId;
    [Header("道具id,数量上限,动画key.x,动画key.y")]
    public List<int4> itemAnimationDataList=new List<int4>();
    public string GetKey()
    {
        return id.ToString();
    }

    public int2 GetAnimationKey(int itemData, int count)
    {
        int2 key = new int2();
        for (int i = 0; i < itemAnimationDataList.Count; i++)
        {
            if (itemAnimationDataList[i].x == itemData&& itemAnimationDataList[i].y>count)
            {
                key.xy = itemAnimationDataList[i].zw; 
                break;
            }
        }
        return key;
    }
    public void SetReferenceData()
    { 
    }
}
 