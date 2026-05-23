using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SleepPanel : GamePanel<MyInt>
{
    [SerializeField]
    private Transform sleepParent;

    [SerializeField]
    private SleepReference sleepReference;

    [SerializeField]
    private Button returnButton;

    private DisplayList<SleepReference, SleepSetData> sleepSetList;

    protected override void Awake()
    {
        base.Awake();
        returnButton.onClick.AddListener(Close);
        sleepSetList = new DisplayList<SleepReference, SleepSetData>(sleepReference, sleepParent);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        sleepParent = FindChildGameObject("Sleeps");
        sleepReference = FindChildGameObject<SleepReference>("Sleep");
        returnButton = FindChildGameObject<Button>("ReturnButton");
    }

    public override async Task InitData(string dataKey)
    {
        mapItemInstance = int.Parse(dataKey);
        var SleepSetDataList = await GameDataManager.instance.GetAsyncData<SleepSetDataList>("SleepSetDataList");
        var SleepList = SleepSetDataList.GetNowSleepSetData(GameTimeManager.instance.Hour,GameTimeManager.instance.GameDay);
        await sleepSetList.InitListData(SleepList, SelectAction);
    }

    private int mapItemInstance = 0;

    private void SelectAction(SleepSetData data, int index, bool value)
    {
        CharacterManager.instance.controllerCharacter.linkItem = mapItemInstance;

        CloseMapObjTips closeMapObjTips = new CloseMapObjTips
        {
            id = mapItemInstance,
        };
        GameActionManager.instance.QueueAction(closeMapObjTips);

        OpenOrCloseInputMap openOrCloseInputMap = new OpenOrCloseInputMap
        {
            open = false,
        };
        GameActionManager.instance.QueueAction(openOrCloseInputMap);

        if (data.SleepToTime)
        {
            int targetHour = data.hour;
            int targetMinue = data.minute;
            PlayerSleep playerSleep = new PlayerSleep
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                targetHour = targetHour,
                targetMinute = targetMinue,
            };
            GameActionManager.instance.QueueAction(playerSleep);
        }
        else
        {
            int targetHour = GameTimeManager.instance.Hour + data.hour;
            int targetMinue = GameTimeManager.instance.Minute + data.minute;
            PlayerSleep playerSleep = new PlayerSleep
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                targetHour = targetHour,
                targetMinute = targetMinue,
            };
            GameActionManager.instance.QueueAction(playerSleep);
        }
        /*
        if (WorldMapManager.instance.GetRuntimeMapItem(mapItemInstance, out var mapItem))
        {
            var coordinate = mapItem.coordinate;
            SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                coordinate = new Unity.Mathematics.int3(coordinate.xy, mapItem.mapInstanceId)
            };
            GameActionManager.instance.QueueAction(setCharacterCoordinate);

            SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                parameter = "State",
                parameterType = ParameterType.INT,
                intValue = 1
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator);

            var sleepPos= mapItem.mapItemData.offsetLinkPos;
            SetCharacterTempPos SetCharacterTempPos = new SetCharacterTempPos
            {
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                pos = sleepPos
            };
            GameActionManager.instance.QueueAction(SetCharacterTempPos);

            SetDirection setDirection = new SetDirection
            {
                directionEnum = Direction.DOWN,
                characterId = CharacterManager.instance.controllerCharacter.instanceId,
            };
            GameActionManager.instance.QueueAction(setDirection);
        }*/

        Close();
    }

    public override void InitReferenceData(MyInt v)
    {
        base.InitReferenceData(v);
    }
}
