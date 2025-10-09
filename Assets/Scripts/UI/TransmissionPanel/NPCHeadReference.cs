using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public struct NPCReferenceData
{
    public Character Character;
    public Vector2 parentPos;
    public GetVectorForMap getVectorForMap;
    public ChangeUINPCReferenceMap changeUINPCReferenceMap;
}
public delegate bool GetVectorForMap(int mapInstance, out Vector2 pos);

public delegate void ChangeUINPCReferenceMap(int oldMap, int newMap, RectTransform rectTransform);
public class NPCHeadReference : UIObjReference<NPCReferenceData>
{
    [SerializeField] private Image NPCHead, PlayerMask;
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
        oldMapInstance = -1;
        NPCHead.sprite = t.Character.characterData.head.sprite;
        PlayerMask.enabled = t.Character == CharacterManager.instance.controllerCharacter;
        // t.changeUINPCReferenceMap(0, data.npc.Character.mapInstance, transform as RectTransform);
        return base.InitData(t, SelectAction, toggleGroup);
    }
    int oldMapInstance;
    public override void OnDisable()
    {
        base.OnDisable();
        oldMapInstance = -1;
    }
    private void Update()
    {
        if (oldMapInstance < 0 || oldMapInstance != data.Character.mapInstance)
        {
            data.changeUINPCReferenceMap(oldMapInstance, data.Character.mapInstance, transform as RectTransform);
            oldMapInstance = data.Character.mapInstance;

            data.getVectorForMap(oldMapInstance, out data.parentPos);

#if UNITY_EDITOR
            _mapInstance = data.Character.mapInstance;
#endif
        }

        if (GameCommon.CheckDisplay(data.Character.mapInstance))
        {  //_rectTransform.localScale = Vector2.one;
            var coordinateIndex = data.Character.GetMapStartIndex();
          
            _rectTransform.anchoredPosition = data.parentPos + new Vector2(coordinateIndex.x, coordinateIndex.y) * 2;
#if UNITY_EDITOR
            _coordinateIndex=coordinateIndex;
            _mapInstance = data.Character.mapInstance;
#endif
          
        }
       
    }
}
