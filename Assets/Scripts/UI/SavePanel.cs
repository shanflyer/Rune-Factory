using UnityEngine;
using UnityEngine.UI;

public class SavePanel : GamePanel<UserGameSaveDataList>
{
    [SerializeField]
    private ToggleGroup toggleGroup;

    [SerializeField]
    private SaveReference SaveReference;

    [SerializeField]
    private Button Copy, Delete, Save, Return;

    [SerializeField]
    private Transform SaveDataParent;

    private DisplayList<SaveReference, UserGameSaveData> saveList;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Delete = FindChildGameObject<Button>("DeleteButton");
        Save = FindChildGameObject<Button>("SaveButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup = GetComponent<ToggleGroup>();
    }

    protected override void Awake()
    {
        base.Awake();
        Copy.onClick.AddListener(CopyData);
        Delete.onClick.AddListener(DeleteData);
        Save.onClick.AddListener(SaveAction);
        Return.onClick.AddListener(() =>
        {
            UIManager.instance.ShowGamePanel<ZeroPanel>();
            Close();
        });

        saveList = new DisplayList<SaveReference, UserGameSaveData>(SaveReference, SaveDataParent);
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

    private UserGameSaveData selectGameSaveData;

    private void SelectAction(UserGameSaveData userGameSaveData, bool selected)
    {
        if (selected)
        {
            selectGameSaveData = userGameSaveData; 
            bool dataIsNull = string.IsNullOrEmpty(userGameSaveData.saveTime);
            Copy.interactable = !dataIsNull;
            Delete.interactable = userGameSaveData.index > 0 && !dataIsNull;
        }
    }

    public override void InitReferenceData(UserGameSaveDataList v)
    {
        base.InitReferenceData(v);
        selectGameSaveData = null;
        saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup);
    }

    private void SaveAction()
    {
        if (GameDataSaveManager.instance.SaveData())
        {
            //DataSaveAndLoadTest.LoadSaveData(SelectedIndex);

            // Close();
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