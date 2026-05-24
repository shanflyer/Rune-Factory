using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GridSimData
{
    public GridLayoutGroup layoutGroup;
    public List<RectTransform> items;
    public bool needRefresh;

    public bool RemoveItem(RectTransform item)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] == item)
            {
                items[i] = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return true;
            }
        }

        return false;
    }
}
public class TransmissionPanel : GamePanel<IReferenceData>
{
    [SerializeField] private Transform ChildItem;
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
    private int dirtyChildMapCount;
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
        var scaleValue = Value * 1.5f;
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
            if (oldRect.RemoveItem(rectTransform))
            {
                MarkChildMapDirty(oldRect);
            }
        }

        if (childMapRectDic.TryGetValue(newMap, out var newRect))
        {
            newRect.items.Add(rectTransform);
            MarkChildMapDirty(newRect);
        }
    }

    private void MarkChildMapDirty(GridSimData gridSimData)
    {
        if (gridSimData.needRefresh)
        {
            return;
        }

        // NPC 位置变化才需要重新排版，LateUpdate 用计数器避免每帧扫描全部地图格。
        gridSimData.needRefresh = true;
        dirtyChildMapCount++;
    }
    
    public override async Task InitData(string dataKey)
    {
        await base.InitData(dataKey);
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
                Character = npcs.npcs[i].Character,
                getVectorForMap = GetParentPos,
                changeUINPCReferenceMap = ChangeUIMapNPCReference
            }; 
            GetParentPos(npcs.npcs[i].Character.mapInstance, out nPCReferenceData.parentPos);
            nPCReferenceDatas.Add(nPCReferenceData);
        }

        var playerReferenceData = new NPCReferenceData
        {
            Character = CharacterManager.instance.controllerCharacter,
            getVectorForMap = GetParentPos,
            changeUINPCReferenceMap = ChangeUIMapNPCReference
        };
        GetParentPos(CharacterManager.instance.controllerCharacter.mapInstance, out playerReferenceData.parentPos);
        nPCReferenceDatas.Add(playerReferenceData);

        await npcList.InitListData(nPCReferenceDatas);
    }

    private void RefreshChild(Transform parent, int childCount)
    {
        for (var i = 0; i < parent.childCount; i++)
            if (i < childCount)
                parent.GetChild(i).gameObject.SetActive(true);
            else
                parent.GetChild(i).gameObject.SetActive(false);

        var addCount = childCount - parent.childCount;
        for (var i = 0; i < addCount; i++)
        {
            var item = Instantiate(ChildItem, parent);
            item.gameObject.SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);
    }

    private void LateUpdate()
    {
        if (dirtyChildMapCount <= 0)
        {
            return;
        }

        foreach (var childMapRect in childMapRectDic.Values)
        {
            if (childMapRect.needRefresh)
            {
                RefreshChild(childMapRect.layoutGroup.transform, childMapRect.items.Count);
                childMapRect.needRefresh = false;
                dirtyChildMapCount--;
                for (var i = 0; i < childMapRect.items.Count; i++)
                {
                    var itemTransform = childMapRect.items[i];
                    itemTransform.position = childMapRect.layoutGroup.transform.GetChild(i).position;
                }

                if (dirtyChildMapCount <= 0)
                {
                    break;
                }
            }
        }
    }
 
}
     
