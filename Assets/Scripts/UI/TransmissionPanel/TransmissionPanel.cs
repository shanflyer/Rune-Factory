using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TransmissionPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button closeBtn;
    [SerializeField]
    Transform NPCParent,Map, ChildMap;
    [SerializeField]
    NPCHeadReference NPC;
    DisplayList<NPCHeadReference, NPCReferenceData> npcList;

    Dictionary<int, Vector2> mapParentPosDic;
    Dictionary<int, Transform> childMapDic;
    protected override void Awake()
    {
        base.Awake();
        closeBtn.onClick.AddListener(Close);
        npcList = new DisplayList<NPCHeadReference, NPCReferenceData>(NPC, NPCParent);
        mapParentPosDic = new Dictionary<int, Vector2>();

        mapParentPosDic = new Dictionary<int, Vector2>();
        for(int i = 0; i < Map.childCount; i++)
        {
            mapParentPosDic.Add(int.Parse(Map.GetChild(i).name), (Map.GetChild(i) as RectTransform).anchoredPosition);
        }

        childMapDic = new Dictionary<int, Transform>();
        for(int i = 0; i < ChildMap.childCount; i++)
        {
            childMapDic.Add(int.Parse(ChildMap.GetChild(i).name), ChildMap.GetChild(i));
        }
    }
     bool GetParentPos(int mapInstance,out Vector2 pos)
    {
        pos = Vector2.zero;
        if(mapParentPosDic.TryGetValue(mapInstance,out pos))
        {
            return true;
        }
        return false;
    }
  
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeBtn = FindChildGameObject<Button>("Close");
    }
    public override async Task InitData(string dataKey)
    {
        base.InitData(dataKey);

        for(int i = 0; i < ChildMap.childCount; i++)
        {
            for(int j = 0; j < ChildMap.GetChild(i).childCount; j++)
            {
                var trans = ChildMap.GetChild(i).GetChild(j);
                trans.SetParent(NPCParent, false);
            }
        }
        npcList.ClearAll();


        var npcs=NPCManager.instance.GetNPCList();
        List<NPCReferenceData> nPCReferenceDatas = new List<NPCReferenceData>();
        for(int i = 0; i < npcs.npcs.Count; i++)
        {
            NPCReferenceData nPCReferenceData = new NPCReferenceData
            {
                npc = npcs.npcs[i],
                getVectorForMap = GetParentPos,
            };
            GetParentPos(npcs.npcs[i].Character.mapInstance, out nPCReferenceData.parentPos);
            nPCReferenceDatas.Add(nPCReferenceData);
        }
        npcList.InitListData(nPCReferenceDatas, SetChildMap);
    }
    void SetChildMap(NPCReferenceData data,int index,bool selected)
    {
        if(childMapDic.TryGetValue(data.npc.Character.mapInstance,out var parent))
        {
            var item = npcList.GetReference(index);
            item.transform.SetParent(parent, false);
        }
        else 
        {
            var item = npcList.GetReference(index);
            if (item.transform.parent != NPCParent)
            {
                item.transform.SetParent(NPCParent, false);
            }

        }
    }
}
     
