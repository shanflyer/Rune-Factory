using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookPanel : GamePanel<IReferenceData> 
{
    [SerializeField]
    Button plantButton;
    [SerializeField]
    Button fishButton; 
    [SerializeField]
    Button characterButton;
    [SerializeField]
    Button formulaButton;
    [SerializeField]
    Button closeButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        plantButton = FindChildGameObject<Button>("Plant");
        fishButton = FindChildGameObject<Button>("Fish");
        characterButton = FindChildGameObject<Button>("Character");
        closeButton = FindChildGameObject<Button>("ReturnButton");
        formulaButton = FindChildGameObject<Button>("Create");
    }
    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(Close);
        characterButton.onClick.AddListener(async () =>
        {
          await  UIManager.instance.ShowGamePanel<NPCPanel,NPCList>(NPCManager.instance.GetNPCList());
            Close();
        });
        fishButton.onClick.AddListener(async () =>
        {
          await  UIManager.instance.ShowGamePanel<FishPanel>();
        });
        formulaButton.onClick.AddListener(() =>
        {
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<FormulaPanel>(), nameof(FormulaPanel));
        });
        plantButton.onClick.AddListener(() =>
        {
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<PlantPanel>(), nameof(PlantPanel));
        });
    }
}
