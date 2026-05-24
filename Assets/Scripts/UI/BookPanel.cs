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
        characterButton.onClick.AddListener(() =>
        {
            RunLifecycleTask(async token =>
            {
                await UIManager.instance.ShowGamePanel<NPCPanel, NPCList>(NPCManager.instance.GetNPCList());
                if (ShouldStopLifecycleTask(token))
                {
                    return;
                }

                Close();
            }, nameof(NPCPanel));
        });
        fishButton.onClick.AddListener(() =>
        {
            RunLifecycleTask(_ => UIManager.instance.ShowGamePanel<FishPanel>(), nameof(FishPanel));
        });
        formulaButton.onClick.AddListener(() =>
        {
            RunLifecycleTask(_ => UIManager.instance.ShowGamePanel<FormulaPanel>(), nameof(FormulaPanel));
        });
        plantButton.onClick.AddListener(() =>
        {
            RunLifecycleTask(_ => UIManager.instance.ShowGamePanel<PlantPanel>(), nameof(PlantPanel));
        });
    }
}
