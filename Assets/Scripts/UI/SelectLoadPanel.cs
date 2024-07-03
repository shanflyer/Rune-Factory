using UnityEngine;
using UnityEngine.UI;

public class SelectLoadPanel : GamePanel<UserGameSaveDataList>
{
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private SaveReference SaveReference;

    [SerializeField]
    private Button Copy, Delete, Start, Return;

    [SerializeField]
    private Transform SaveDataParent;

    private DisplayList<SaveReference, UserGameSaveData> saveList;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Delete = FindChildGameObject<Button>("DeleteButton");
        Start = FindChildGameObject<Button>("StartButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup = GetComponent<ToggleGroup>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshGameSaveData>(RefreshGameSaveData);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshGameSaveData>(RefreshGameSaveData);
    }

    private void RefreshGameSaveData(RefreshGameSaveData refreshGameSaveData)
    {
        InitReferenceData(GameDataSaveManager.instance.UserGameSaveDataList);
    }

    protected override void Awake()
    {
        base.Awake();
        Copy.onClick.AddListener(CopyData);
        Delete.onClick.AddListener(DeleteData);
        Start.onClick.AddListener(StartAction);
        Return.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<ZeroPanel>();
            Close();
        });

        saveList = new DisplayList<SaveReference, UserGameSaveData>(SaveReference, SaveDataParent);
    }

    private void SelectAction(UserGameSaveData userGameSaveData, bool selected)
    {
        if (selected)
        {
            selectGameSaveData = userGameSaveData;
            bool dataIsNull = string.IsNullOrEmpty(userGameSaveData.saveTime);
            Copy.interactable = !dataIsNull;
            Delete.interactable = userGameSaveData.index >= 0 && !dataIsNull;
            Start.interactable = !dataIsNull;
        }
    }

    private UserGameSaveData selectGameSaveData;

    public override void InitReferenceData(UserGameSaveDataList v)
    {
        base.InitReferenceData(v);
        selectGameSaveData = v.nowSaveData;
        SaveReference.InitData(v.nowSaveData, SelectAction, toggleGroup);
        saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup);
    }

    private void StartAction()
    {
        if (selectGameSaveData!=null)
        {
            GameDataSaveManager.instance.loadingIndex = selectGameSaveData.index;
            SceneManager.instance.SwitchScene("World");
           // UIManager.instance.ShowGamePanel<LoadingPanel>();

            Close();
        }
    }

    private void CopyData()
    {
        if (!string.IsNullOrEmpty(selectGameSaveData.saveTime))
        {
            if (GameDataSaveManager.instance.CopySaveData(selectGameSaveData))
            {
            }
            else
            {
            }
        }
        else
        {
        }
    }

    private void DeleteData()
    {
        GameDataSaveManager.instance.DeletaSaveData(selectGameSaveData);
    }
}