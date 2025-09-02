using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public struct NPCReferenceData
{
    public NPC npc;
    public Vector2 parentPos;
    public GetVectorForMap getVectorForMap;
    public ChangeUINPCReferenceMap changeUINPCReferenceMap;
}
public delegate bool GetVectorForMap(int mapInstance, out Vector2 pos);

public delegate void ChangeUINPCReferenceMap(int oldMap, int newMap, RectTransform rectTransform);
public class NPCHeadReference : UIObjReference<NPCReferenceData>
{
    [SerializeField]
    Image NPCHead;
    [SerializeField]
    RectTransform _rectTransform;

#if UNITY_EDITOR
    [SerializeField]
    int2 _coordinateIndex;
    [SerializeField]
    int _mapInstance;
#endif
    public override Task InitData(NPCReferenceData t, SelectAction<NPCReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        oldMapInstance = 0;
        NPCHead.sprite = t.npc.Character.characterData.head.sprite;
        return base.InitData(t, SelectAction, toggleGroup);
    }
    int oldMapInstance;
    public override void OnDisable()
    {
        base.OnDisable();
        oldMapInstance = 0;
    }
    private void Update()
    {
        if (oldMapInstance != data.npc.Character.mapInstance)
        {
            data.changeUINPCReferenceMap(oldMapInstance, data.npc.Character.mapInstance, transform as RectTransform);
            oldMapInstance = data.npc.Character.mapInstance;
            data.getVectorForMap(oldMapInstance, out data.parentPos); 
        }

        if(GameCommon.CheckDisplay(data.npc.Character.mapInstance))
        {  //_rectTransform.localScale = Vector2.one;
            int2 coordinateIndex = data.npc.Character.GetMapStartIndex();
            //RectTransformPresets.Apply(_rectTransform, RectTransformPresets.Preset.BottomLeft);
            _rectTransform.anchoredPosition = data.parentPos + new Vector2(coordinateIndex.x, coordinateIndex.y) * 2;
#if UNITY_EDITOR
            _coordinateIndex=coordinateIndex;
            _mapInstance= data.npc.Character.mapInstance;
#endif
          
        }
       
    }
}
