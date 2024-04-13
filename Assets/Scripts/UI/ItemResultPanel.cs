using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemResultPanel :GamePanel<ItemResultInfo>
{
    [SerializeField]
    Image icon;
    [SerializeField]
    TextMeshProUGUI info0, info1;
    [SerializeField]
    GameObject effect;
    [SerializeField]
    Button closeButton;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        icon = FindChildGameObject<Image>("ItemIcon");
        info0 = FindChildGameObject<TextMeshProUGUI>("Info0");
        info1 = FindChildGameObject<TextMeshProUGUI>("Info1");
        effect = FindChildGameObject("Effect").gameObject;
        closeButton = FindChildGameObject<Button>("Close");
    }
    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(Close);
    }
    public override void Close()
    {
        base.Close();
        icon.enabled=false;
        effect.SetActive(false);
    }
    public override void InitReferenceData(ItemResultInfo v)
    {
        base.InitReferenceData(v);
        if (v.icon == null)
        {
            icon.enabled = false;
            effect.SetActive(false);
        }
        else
        {
            icon.enabled = true;
            icon.sprite = v.icon;
            effect.SetActive(true);
        }
        info0.text = v.info0;
        info1.text = v.info1;
    }
}
public struct ItemResultInfo : IReferenceData
{
    public Sprite icon;
    public string info0, info1;
}