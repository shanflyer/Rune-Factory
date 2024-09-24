using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookPanel : GamePanel<IReferenceData> 
{ 
    [SerializeField]
    Button fishButton; 
    [SerializeField]
    Button characterButton;
    [SerializeField]
    Button closeButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        fishButton = FindChildGameObject<Button>("Fish");
        characterButton = FindChildGameObject<Button>("Character");
        closeButton = FindChildGameObject<Button>("ReturnButton");
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
        
    }
}
