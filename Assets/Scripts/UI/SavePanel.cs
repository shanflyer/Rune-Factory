using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SavePanel : GamePanel<UserGameSaveDataList>
{
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    SaveReference SaveReference;
    [SerializeField]
    Button Copy, Delete, Save, Return;
    [SerializeField]
    Transform SaveDataParent;

    DisplayList<SaveReference, UserGameSaveData> saveList;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        SaveReference = FindChildGameObject<SaveReference>("SaveReference");
        Copy = FindChildGameObject<Button>("CopyButton");
        Delete = FindChildGameObject<Button>("DeleteButton");
        Save = FindChildGameObject<Button>("SaveButton");
        Return = FindChildGameObject<Button>("ReturnButton");
        SaveDataParent = FindChildGameObject("ManualParent");
        toggleGroup=GetComponent<ToggleGroup>();
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
        GameActionManager.instance.RemoveListener<RefreshGameSaveData>(RefreshGameSaveData);
    }
    void RefreshGameSaveData(RefreshGameSaveData refreshGameSaveData)
    {
        InitReferenceData(GameDataSaveManager.instance.UserGameSaveDataList);
    }

    UserGameSaveData selectGameSaveData;
    void SelectAction(UserGameSaveData userGameSaveData, bool selected)
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
        selectGameSaveData = default(UserGameSaveData);
        saveList.InitListData(v.userGameSaveDatas, SelectAction, toggleGroup);
    }
    void SaveAction()
    {
        if (GameDataSaveManager.instance.SaveData(selectGameSaveData))
        {
            //DataSaveAndLoadTest.LoadSaveData(SelectedIndex);  
            
           // Close();
        } 
    }
    void CopyData()
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
    void DeleteData()
    {
        GameDataSaveManager.instance.DeletaSaveData(selectGameSaveData);
    }

}
