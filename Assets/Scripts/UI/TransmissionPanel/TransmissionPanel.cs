using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public struct GridSimData
{
    public GridLayoutGroup layoutGroup;
    public List<RectTransform> items;
    public bool needRefresh;

    public void RemoveItem(RectTransform item)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            items[i] = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
        }
    }
}
public class TransmissionPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    Button closeBtn;
    [SerializeField]
    Transform NPCParent,Map, ChildMap;
    [SerializeField]
    NPCHeadReference NPC;
    [SerializeField]
    ScrollRect Scroll;
    [SerializeField]
    Slider ViewSlider;
    [SerializeField]
    Transform MapImage;
    [SerializeField]
    Transform FightMap;
    DisplayList<NPCHeadReference, NPCReferenceData> npcList;

    private Dictionary<int, Vector2> mapParentPosDic;

    private readonly Dictionary<int, GridSimData> childMapRectDic = new();
    protected override void Awake()
    {
        base.Awake();
        ViewSlider.onValueChanged.AddListener(SetScale);

        closeBtn.onClick.AddListener(Close);
        npcList = new DisplayList<NPCHeadReference, NPCReferenceData>(NPC, NPCParent);
        mapParentPosDic = new Dictionary<int, Vector2>();

        mapParentPosDic = new Dictionary<int, Vector2>();
        for(int i = 0; i < Map.childCount; i++)
        {
            mapParentPosDic.Add(int.Parse(Map.GetChild(i).name), (Map.GetChild(i) as RectTransform).anchoredPosition);
        }

        childMapRectDic.Clear();
        for(int i = 0; i < ChildMap.childCount; i++)
        {
            var GridSimData = new GridSimData
            {
                layoutGroup = ChildMap.GetChild(i).GetComponent<GridLayoutGroup>(),
                items = new List<RectTransform>()
            };
            childMapRectDic.Add(int.Parse(ChildMap.GetChild(i).name), GridSimData);
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
    void SetScale(float  Value)
    {
        float scaleValue = Value + 1;
        MapImage.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
  
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        closeBtn = FindChildGameObject<Button>("Close");
    }

    private void ChangeUIMapNPCReference(int oldMap, int newMap, RectTransform rectTransform)
    {
        if (childMapRectDic.TryGetValue(oldMap, out var oldRect))
        {
            oldRect.needRefresh = true;
            oldRect.RemoveItem(rectTransform);
        }

        if (childMapRectDic.TryGetValue(newMap, out var newRect))
        {
            oldRect.needRefresh = true;
            newRect.items.Add(rectTransform);
        }
    }
    
    public override async Task InitData(string dataKey)
    {
        base.InitData(dataKey);
        FightMap.transform.localScale = Vector3.zero;
        try
        {
            int dValue = int.Parse(dataKey);
            if (dValue > 0)
            {
                FightMap.transform.localScale = Vector3.one;
            }
        }
        catch
        {

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
                changeUINPCReferenceMap = ChangeUIMapNPCReference
            };
            GetParentPos(npcs.npcs[i].Character.mapInstance, out nPCReferenceData.parentPos);
            nPCReferenceDatas.Add(nPCReferenceData);
        }

        npcList.InitListData(nPCReferenceDatas);
    }

    private void Update()
    {
        foreach (var childMapRect in childMapRectDic.Values)
        {
            if (childMapRect.needRefresh)
                GridSimLayout.Apply(childMapRect.layoutGroup, childMapRect.items, NPCParent as RectTransform);
        }
        
    }
}
     
